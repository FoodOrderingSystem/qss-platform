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
public class LearningController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public LearningController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var materials = await _db.LearningMaterials
            .Include(l => l.UploadedBy)
            .Include(l => l.LearningProgress.Where(p => p.UserId == userId))
            .Select(l => new LearningMaterialDto
            {
                Id = l.Id,
                Title = l.Title,
                Description = l.Description,
                FileUrl = l.FileUrl,
                FileName = l.FileName,
                FileType = l.FileType,
                FileSizeBytes = l.FileSizeBytes,
                Category = l.Category,
                UploadedByUserId = l.UploadedByUserId,
                UploadedByName = l.UploadedBy != null ? l.UploadedBy.FullName : null,
                CreatedAt = l.CreatedAt,
                UserProgressPercent = l.LearningProgress.FirstOrDefault() != null
                    ? l.LearningProgress.First().ProgressPercent : 0,
                UserCompleted = l.LearningProgress.FirstOrDefault() != null
                    ? l.LearningProgress.First().IsCompleted : false
            }).ToListAsync();
        return Ok(materials);
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Upload([FromForm] UploadMaterialDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        string fileUrl = "/uploads/placeholder.pdf";
        string fileName = dto.Title;
        long fileSize = 0;
        string fileType = "pdf";

        if (dto.File != null && dto.File.Length > 0)
        {
            var uploadsPath = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsPath);
            var ext = Path.GetExtension(dto.File.FileName).ToLower();
            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsPath, uniqueName);
            using var stream = System.IO.File.Create(filePath);
            await dto.File.CopyToAsync(stream);
            fileUrl = $"/uploads/{uniqueName}";
            fileName = dto.File.FileName;
            fileSize = dto.File.Length;
            fileType = ext.TrimStart('.');
        }

        var material = new LearningMaterial
        {
            Title = dto.Title,
            Description = dto.Description,
            FileUrl = fileUrl,
            FileName = fileName,
            FileSizeBytes = fileSize,
            FileType = fileType,
            Category = dto.Category,
            UploadedByUserId = userId
        };
        _db.LearningMaterials.Add(material);
        await _db.SaveChangesAsync();
        return Ok(new { material.Id });
    }

    [HttpPatch("{id}/progress")]
    public async Task<IActionResult> UpdateProgress(int id, [FromBody] UpdateProgressDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var progress = await _db.LearningProgress
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LearningMaterialId == id);

        if (progress == null)
        {
            progress = new LearningProgress { UserId = userId, LearningMaterialId = id };
            _db.LearningProgress.Add(progress);
        }

        progress.ProgressPercent = dto.ProgressPercent;
        if (dto.ProgressPercent >= 100)
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("progress")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> GetAllProgress()
    {
        var progress = await _db.LearningProgress
            .Include(p => p.User)
            .Include(p => p.LearningMaterial)
            .Select(p => new
            {
                UserId = p.UserId,
                UserName = p.User != null ? p.User.FullName : "Unknown",
                MaterialTitle = p.LearningMaterial != null ? p.LearningMaterial.Title : "Unknown",
                p.ProgressPercent,
                p.IsCompleted,
                p.CompletedAt
            }).ToListAsync();
        return Ok(progress);
    }
}

public class UploadMaterialDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public IFormFile? File { get; set; }
}

public class UpdateProgressDto
{
    public int ProgressPercent { get; set; }
}
