using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QSS.Application.DTOs;
using QSS.Domain.Entities;
using QSS.Infrastructure.Data;

namespace QSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MaterialsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public MaterialsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var materials = await _db.Materials.Select(m => new MaterialDto
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description,
            SupplierName = m.SupplierName,
            SupplierLink = m.SupplierLink,
            StockQuantity = m.StockQuantity,
            MinimumStockLevel = m.MinimumStockLevel,
            Unit = m.Unit.ToString(),
            ExpirationDate = m.ExpirationDate,
            Category = m.Category,
            IsLowStock = m.StockQuantity <= m.MinimumStockLevel,
            IsExpiringSoon = m.ExpirationDate.HasValue && m.ExpirationDate < DateTime.UtcNow.AddDays(30)
        }).ToListAsync();
        return Ok(materials);
    }

    [HttpPost]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateMaterialDto dto)
    {
        var material = new Material
        {
            Name = dto.Name, Description = dto.Description, SupplierName = dto.SupplierName,
            SupplierLink = dto.SupplierLink, StockQuantity = dto.StockQuantity,
            MinimumStockLevel = dto.MinimumStockLevel, Unit = dto.Unit,
            ExpirationDate = dto.ExpirationDate, Category = dto.Category
        };
        _db.Materials.Add(material);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = material.Id }, new { material.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateMaterialDto dto)
    {
        var m = await _db.Materials.FindAsync(id);
        if (m == null) return NotFound();
        m.Name = dto.Name; m.Description = dto.Description; m.SupplierName = dto.SupplierName;
        m.SupplierLink = dto.SupplierLink; m.StockQuantity = dto.StockQuantity;
        m.MinimumStockLevel = dto.MinimumStockLevel; m.Unit = dto.Unit;
        m.ExpirationDate = dto.ExpirationDate; m.Category = dto.Category;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await _db.Materials.FindAsync(id);
        if (m == null) return NotFound();
        m.IsDeleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public MedicationsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var meds = await _db.Medications.Select(m => new MedicationDto
        {
            Id = m.Id,
            Name = m.Name,
            ActiveIngredient = m.ActiveIngredient,
            Dosage = m.Dosage,
            Quantity = m.Quantity,
            MinimumQuantity = m.MinimumQuantity,
            ExpirationDate = m.ExpirationDate,
            IsLowStock = m.Quantity <= m.MinimumQuantity,
            IsExpiringSoon = m.ExpirationDate.HasValue && m.ExpirationDate < DateTime.UtcNow.AddDays(30)
        }).ToListAsync();
        return Ok(meds);
    }

    [HttpPost]
    [Authorize(Roles = "Superadmin,Admin,Dentist")]
    public async Task<IActionResult> Create([FromBody] CreateMedicationDto dto)
    {
        var med = new Medication
        {
            Name = dto.Name, Description = dto.Description, ActiveIngredient = dto.ActiveIngredient,
            Dosage = dto.Dosage, Quantity = dto.Quantity, MinimumQuantity = dto.MinimumQuantity,
            ExpirationDate = dto.ExpirationDate, StorageInstructions = dto.StorageInstructions,
            BatchNumber = dto.BatchNumber
        };
        _db.Medications.Add(med);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = med.Id }, new { med.Id });
    }

    [HttpPost("{id}/log-usage")]
    [Authorize(Roles = "Superadmin,Admin,Dentist,DentalAssistant")]
    public async Task<IActionResult> LogUsage(int id, [FromBody] LogUsageDto dto)
    {
        var med = await _db.Medications.FindAsync(id);
        if (med == null) return NotFound();
        if (med.Quantity < dto.QuantityUsed) return BadRequest("Insufficient stock");

        med.Quantity -= dto.QuantityUsed;
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        _db.MedicationUsageLogs.Add(new MedicationUsageLog
        {
            MedicationId = id,
            QuantityUsed = dto.QuantityUsed,
            UsedByUserId = userId,
            Notes = dto.Notes
        });
        await _db.SaveChangesAsync();
        return Ok(new { remaining = med.Quantity });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await _db.Medications.FindAsync(id);
        if (m == null) return NotFound();
        m.IsDeleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class LogUsageDto
{
    public int QuantityUsed { get; set; }
    public string? Notes { get; set; }
}
