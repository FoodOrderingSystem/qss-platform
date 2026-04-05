using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QSS.Application.DTOs;
using QSS.Domain.Entities;

namespace QSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        await _userManager.AddToRoleAsync(user, dto.Role);
        return Ok(new { user.Id });
    }

    [HttpPut("{id}/deactivate")]
    [Authorize(Roles = "Superadmin,Admin")]
    public async Task<IActionResult> Deactivate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        user.IsActive = false;
        await _userManager.UpdateAsync(user);
        return NoContent();
    }

    [HttpPut("{id}/role")]
    [Authorize(Roles = "Superadmin")]
    public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeRoleDto dto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, dto.Role);
        return NoContent();
    }
}

public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "DentalAssistant";
}

public class ChangeRoleDto
{
    public string Role { get; set; } = string.Empty;
}
