using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QSS.Application.DTOs;
using QSS.Domain.Entities;
using QSS.Infrastructure.Data;
using QSS.Infrastructure.Services;

namespace QSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly QrCodeService _qrService;

    public RoomsController(ApplicationDbContext db, QrCodeService qrService)
    {
        _db = db;
        _qrService = qrService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _db.Rooms
            .Include(r => r.Devices)
            .Include(r => r.Tasks)
            .Select(r => new RoomDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Location = r.Location,
                DeviceCount = r.Devices.Count,
                OpenTaskCount = r.Tasks.Count(t => t.Status != QSS.Domain.Enums.QssTaskStatus.Completed)
            }).ToListAsync();
        return Ok(rooms);
    }

    [HttpPost]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateRoomDto dto)
    {
        var room = new Room { Name = dto.Name, Description = dto.Description, Location = dto.Location };
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = room.Id }, new { room.Id });
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
        var devices = await _db.Devices
            .Include(d => d.Room)
            .Select(d => new DeviceDto
            {
                Id = d.Id,
                Name = d.Name,
                SerialNumber = d.SerialNumber,
                Manufacturer = d.Manufacturer,
                Model = d.Model,
                NextMaintenanceDue = d.NextMaintenanceDue,
                QrCode = d.QrCode,
                RoomId = d.RoomId,
                RoomName = d.Room != null ? d.Room.Name : null,
                MaintenanceOverdue = d.NextMaintenanceDue.HasValue && d.NextMaintenanceDue < DateTime.UtcNow
            }).ToListAsync();
        return Ok(devices);
    }

    [HttpPost]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDeviceDto dto)
    {
        var device = new Device
        {
            Name = dto.Name,
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

        return CreatedAtAction(nameof(GetAll), new { id = device.Id }, new { device.Id, device.QrCode });
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
