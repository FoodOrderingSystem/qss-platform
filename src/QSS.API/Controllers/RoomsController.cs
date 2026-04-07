using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QSS.Application.DTOs;
using QSS.Domain.Entities;
using QSS.Domain.Enums;
using QSS.Infrastructure.Data;
using QSS.Infrastructure.Services;
using System.Security.Claims;

namespace QSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public RoomsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var now = DateTime.UtcNow;
        var rooms = await _db.Rooms
            .Include(r => r.Devices)
            .Include(r => r.Tasks)
            .Select(r => new RoomDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Location = r.Location,
                DeviceCount = r.Devices.Count(d => !d.IsDeleted),
                OpenTaskCount = r.Tasks.Count(t => !t.IsDeleted && t.Status != QssTaskStatus.Completed),
                OverdueTaskCount = r.Tasks.Count(t => !t.IsDeleted && (t.Status == QssTaskStatus.Overdue ||
                    (t.DueDate.HasValue && t.DueDate < now && t.Status != QssTaskStatus.Completed)))
            }).ToListAsync();
        return Ok(rooms);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var room = await _db.Rooms
            .Include(r => r.Devices.Where(d => !d.IsDeleted))
            .Include(r => r.Tasks.Where(t => !t.IsDeleted)).ThenInclude(t => t.Assignee)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (room == null) return NotFound();

        var now = DateTime.UtcNow;
        var tasks = room.Tasks.ToList();
        var devices = room.Devices.ToList();

        return Ok(new RoomDetailDto
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            Location = room.Location,
            DeviceCount = devices.Count,
            OpenTaskCount = tasks.Count(t => t.Status != QssTaskStatus.Completed),
            OverdueTaskCount = tasks.Count(t => t.Status == QssTaskStatus.Overdue ||
                (t.DueDate.HasValue && t.DueDate < now && t.Status != QssTaskStatus.Completed)),
            Devices = devices.Select(d => new RoomDeviceSummaryDto
            {
                Id = d.Id,
                Name = d.Name,
                Manufacturer = d.Manufacturer,
                Model = d.Model,
                SerialNumber = d.SerialNumber,
                NextMaintenanceDue = d.NextMaintenanceDue,
                MaintenanceOverdue = d.NextMaintenanceDue.HasValue && d.NextMaintenanceDue < now
            }).ToList(),
            Tasks = tasks.Select(t => new RoomTaskSummaryDto
            {
                Id = t.Id,
                Name = t.Name,
                Status = t.Status.ToString(),
                DueDate = t.DueDate,
                AssigneeName = t.Assignee?.FullName,
                IsOverdue = t.Status == QssTaskStatus.Overdue ||
                    (t.DueDate.HasValue && t.DueDate < now && t.Status != QssTaskStatus.Completed)
            }).OrderBy(t => t.DueDate).ToList()
        });
    }

    [HttpPost]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
    {
        var room = new Room { Name = dto.Name, Description = dto.Description, Location = dto.Location };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, new { room.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateRoomDto dto)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room == null) return NotFound();
        room.Name = dto.Name;
        room.Description = dto.Description;
        room.Location = dto.Location;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room == null) return NotFound();
        room.IsDeleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly QrCodeService _qrService;

    public DevicesController(ApplicationDbContext db, QrCodeService qrService)
    {
        _db = db;
        _qrService = qrService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var now = DateTime.UtcNow;
        var devices = await _db.Devices
            .Include(d => d.Room)
            .Select(d => new DeviceDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                SerialNumber = d.SerialNumber,
                Manufacturer = d.Manufacturer,
                Model = d.Model,
                PurchaseDate = d.PurchaseDate,
                LastMaintenanceDate = d.LastMaintenanceDate,
                NextMaintenanceDue = d.NextMaintenanceDue,
                QrCode = d.QrCode,
                RoomId = d.RoomId,
                RoomName = d.Room != null ? d.Room.Name : null,
                MaintenanceOverdue = d.NextMaintenanceDue.HasValue && d.NextMaintenanceDue < now
            }).ToListAsync();
        return Ok(devices);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var device = await _db.Devices
            .Include(d => d.Room)
            .Include(d => d.Tasks.Where(t => !t.IsDeleted)).ThenInclude(t => t.Assignee)
            .Include(d => d.History.Where(h => !h.IsDeleted)).ThenInclude(h => h.PerformedBy)
            .Include(d => d.History.Where(h => !h.IsDeleted)).ThenInclude(h => h.LinkedTask)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (device == null) return NotFound();

        var now = DateTime.UtcNow;
        var tasks = device.Tasks.ToList();

        return Ok(new DeviceDetailDto
        {
            Id = device.Id,
            Name = device.Name,
            Description = device.Description,
            SerialNumber = device.SerialNumber,
            Manufacturer = device.Manufacturer,
            Model = device.Model,
            PurchaseDate = device.PurchaseDate,
            LastMaintenanceDate = device.LastMaintenanceDate,
            NextMaintenanceDue = device.NextMaintenanceDue,
            QrCode = device.QrCode,
            RoomId = device.RoomId,
            RoomName = device.Room?.Name,
            MaintenanceOverdue = device.NextMaintenanceDue.HasValue && device.NextMaintenanceDue < now,
            Tasks = tasks.Select(t => new DeviceTaskSummaryDto
            {
                Id = t.Id,
                Name = t.Name,
                Status = t.Status.ToString(),
                DueDate = t.DueDate,
                AssigneeName = t.Assignee?.FullName,
                IsOverdue = t.Status == QssTaskStatus.Overdue ||
                    (t.DueDate.HasValue && t.DueDate < now && t.Status != QssTaskStatus.Completed)
            }).OrderBy(t => t.DueDate).ToList(),
            History = device.History.OrderByDescending(h => h.EventDate).Select(h => new DeviceHistoryDto
            {
                Id = h.Id,
                EventType = h.EventType,
                Title = h.Title,
                Notes = h.Notes,
                EventDate = h.EventDate,
                PerformedByName = h.PerformedBy?.FullName,
                LinkedTaskName = h.LinkedTask?.Name,
                LinkedTaskId = h.LinkedTaskId
            }).ToList()
        });
    }

    [HttpPost]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDeviceDto dto)
    {
        var device = new Device
        {
            Name = dto.Name,
            Description = dto.Description,
            SerialNumber = dto.SerialNumber,
            Manufacturer = dto.Manufacturer,
            Model = dto.Model,
            PurchaseDate = dto.PurchaseDate,
            NextMaintenanceDue = dto.NextMaintenanceDue,
            RoomId = dto.RoomId
        };
        _db.Devices.Add(device);
        await _db.SaveChangesAsync();

        // Generate QR code after ID is assigned
        var qrContent = $"https://qss.dental/devices/{device.Id}";
        device.QrCode = _qrService.GenerateQrCodeBase64(qrContent);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = device.Id }, new { device.Id, device.QrCode });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateDeviceDto dto)
    {
        var device = await _db.Devices.FindAsync(id);
        if (device == null) return NotFound();

        var prevMaintenanceDate = device.NextMaintenanceDue;

        device.Name = dto.Name;
        device.Description = dto.Description;
        device.SerialNumber = dto.SerialNumber;
        device.Manufacturer = dto.Manufacturer;
        device.Model = dto.Model;
        device.PurchaseDate = dto.PurchaseDate;
        device.RoomId = dto.RoomId;

        // If maintenance date changed, record it and update LastMaintenanceDate
        if (dto.NextMaintenanceDue.HasValue && prevMaintenanceDate != dto.NextMaintenanceDue)
        {
            device.LastMaintenanceDate = DateTime.UtcNow;
            device.NextMaintenanceDue = dto.NextMaintenanceDue;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var history = new DeviceHistory
            {
                DeviceId = id,
                EventType = "Maintenance",
                Title = "Maintenance schedule updated",
                Notes = $"Next maintenance set to {dto.NextMaintenanceDue:d}",
                EventDate = DateTime.UtcNow,
                PerformedByUserId = userId
            };
            _db.DeviceHistories.Add(history);
        }
        else
        {
            device.NextMaintenanceDue = dto.NextMaintenanceDue;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/qrcode")]
    public async Task<IActionResult> GetQrCode(int id)
    {
        var device = await _db.Devices.FindAsync(id);
        if (device == null) return NotFound();
        if (string.IsNullOrEmpty(device.QrCode))
        {
            device.QrCode = _qrService.GenerateQrCodeBase64($"https://qss.dental/devices/{id}");
            await _db.SaveChangesAsync();
        }
        return Ok(new { qrCode = device.QrCode });
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var history = await _db.DeviceHistories
            .Where(h => h.DeviceId == id && !h.IsDeleted)
            .Include(h => h.PerformedBy)
            .Include(h => h.LinkedTask)
            .OrderByDescending(h => h.EventDate)
            .Select(h => new DeviceHistoryDto
            {
                Id = h.Id,
                EventType = h.EventType,
                Title = h.Title,
                Notes = h.Notes,
                EventDate = h.EventDate,
                PerformedByName = h.PerformedBy != null ? h.PerformedBy.FirstName + " " + h.PerformedBy.LastName : null,
                LinkedTaskName = h.LinkedTask != null ? h.LinkedTask.Name : null,
                LinkedTaskId = h.LinkedTaskId
            }).ToListAsync();
        return Ok(history);
    }

    [HttpPost("{id}/history")]
    [Authorize(Roles = "Superadmin,Admin,Dentist,DentalAssistant")]
    public async Task<IActionResult> AddHistory(int id, [FromBody] AddDeviceHistoryDto dto)
    {
        var device = await _db.Devices.FindAsync(id);
        if (device == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var history = new DeviceHistory
        {
            DeviceId = id,
            EventType = dto.EventType,
            Title = dto.Title,
            Notes = dto.Notes,
            EventDate = dto.EventDate ?? DateTime.UtcNow,
            PerformedByUserId = userId
        };
        _db.DeviceHistories.Add(history);

        // If it's a maintenance event, update LastMaintenanceDate
        if (dto.EventType == "Maintenance")
        {
            device.LastMaintenanceDate = history.EventDate;
        }

        await _db.SaveChangesAsync();
        return Ok(new { history.Id });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var device = await _db.Devices.FindAsync(id);
        if (device == null) return NotFound();
        device.IsDeleted = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
