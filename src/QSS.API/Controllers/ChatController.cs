using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QSS.Application.DTOs;
using QSS.Domain.Entities;
using QSS.Infrastructure.Data;
using QSS.API.Hubs;
using System.Security.Claims;

namespace QSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IHubContext<ChatHub> _chatHub;

    public ChatController(ApplicationDbContext db, IHubContext<ChatHub> chatHub)
    {
        _db = db;
        _chatHub = chatHub;
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages([FromQuery] string? recipientId, [FromQuery] string? team)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        IQueryable<ChatMessage> query = _db.ChatMessages.Include(m => m.Sender);

        if (!string.IsNullOrEmpty(team))
            query = query.Where(m => m.TeamName == team);
        else if (!string.IsNullOrEmpty(recipientId))
            query = query.Where(m => (m.SenderId == userId && m.RecipientId == recipientId) ||
                                      (m.SenderId == recipientId && m.RecipientId == userId));

        var messages = await query.OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                Content = m.Content,
                SenderId = m.SenderId,
                SenderName = m.Sender != null ? m.Sender.FullName : "Unknown",
                RecipientId = m.RecipientId,
                TeamName = m.TeamName,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt
            }).ToListAsync();
        return Ok(messages);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var msg = new ChatMessage
        {
            Content = dto.Content,
            SenderId = userId,
            RecipientId = dto.RecipientId,
            TeamName = dto.TeamName
        };
        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync();

        var sender = await _db.Users.FindAsync(userId);
        var msgDto = new ChatMessageDto
        {
            Id = msg.Id,
            Content = msg.Content,
            SenderId = msg.SenderId,
            SenderName = sender?.FullName ?? "Unknown",
            RecipientId = msg.RecipientId,
            TeamName = msg.TeamName,
            CreatedAt = msg.CreatedAt
        };

        // Push via SignalR
        if (!string.IsNullOrEmpty(dto.RecipientId))
            await _chatHub.Clients.User(dto.RecipientId).SendAsync("ReceiveMessage", msgDto);
        else if (!string.IsNullOrEmpty(dto.TeamName))
            await _chatHub.Clients.Group(dto.TeamName).SendAsync("ReceiveMessage", msgDto);

        return Ok(msgDto);
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();
        return Ok(notifications);
    }

    [HttpPatch("notifications/{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var n = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (n == null) return NotFound();
        n.IsRead = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
