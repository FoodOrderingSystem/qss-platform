using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QSS.API.Authorization;
using QSS.Application.DTOs;
using QSS.Domain.Entities;
using QSS.Infrastructure.Data;

namespace QSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProcessCategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProcessCategoriesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.ProcessCategories
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new ProcessCategoryItemDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                SortOrder = c.SortOrder,
                ProcessCount = c.Processes.Count(p => !p.IsDeleted)
            }).ToListAsync();
        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.CategoriesManage)]
    public async Task<IActionResult> Create([FromBody] CreateProcessCategoryItemDto dto)
    {
        var category = new ProcessCategoryItem
        {
            Name = dto.Name,
            Description = dto.Description,
            SortOrder = dto.SortOrder
        };
        _db.ProcessCategories.Add(category);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = category.Id }, new { category.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Permissions.CategoriesManage)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProcessCategoryItemDto dto)
    {
        var category = await _db.ProcessCategories.FindAsync(id);
        if (category == null) return NotFound();
        category.Name = dto.Name;
        category.Description = dto.Description;
        category.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/toggle")]
    [Authorize(Policy = Permissions.CategoriesManage)]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var category = await _db.ProcessCategories.FindAsync(id);
        if (category == null) return NotFound();
        category.IsActive = !category.IsActive;
        await _db.SaveChangesAsync();
        return Ok(new { category.IsActive });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.CategoriesManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.ProcessCategories.FindAsync(id);
        if (category == null) return NotFound();
        var inUse = await _db.Processes.AnyAsync(p => p.ProcessCategoryId == id && !p.IsDeleted);
        if (inUse)
            return BadRequest(new { message = "Cannot delete a category that is in use by existing processes." });
        category.IsDeleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
