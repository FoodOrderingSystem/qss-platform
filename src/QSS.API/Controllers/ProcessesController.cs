using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QSS.Application.DTOs;
using QSS.Domain.Entities;
using QSS.Infrastructure.Data;
using System.Security.Claims;

namespace QSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProcessesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProcessesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var processes = await _db.Processes
            .Include(p => p.Tasks)
            .Select(p => new ProcessDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category.ToString(),
                IsActive = p.IsActive,
                TaskCount = p.Tasks.Count,
                CreatedAt = p.CreatedAt
            }).ToListAsync();
        return Ok(processes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Processes.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id);
        if (p == null) return NotFound();
        return Ok(new ProcessDto
        {
            Id = p.Id, Name = p.Name, Description = p.Description,
            Category = p.Category.ToString(), IsActive = p.IsActive,
            TaskCount = p.Tasks.Count, CreatedAt = p.CreatedAt
        });
    }

    [HttpPost]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProcessDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var process = new QssProcess
        {
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            CreatedByUserId = userId
        };
        _db.Processes.Add(process);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = process.Id }, new { process.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProcessDto dto)
    {
        var process = await _db.Processes.FindAsync(id);
        if (process == null) return NotFound();
        process.Name = dto.Name;
        process.Description = dto.Description;
        process.Category = dto.Category;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var process = await _db.Processes.FindAsync(id);
        if (process == null) return NotFound();
        process.IsDeleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
