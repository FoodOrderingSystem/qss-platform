using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace QSS.Web.Pages;

[Authorize]
public class TasksModel : PageModel
{
    public string UserRole { get; private set; } = string.Empty;
    public bool CanCreateTasks { get; private set; }
    public bool CanEditTasks { get; private set; }
    public bool CanDeleteTasks { get; private set; }
    public string CurrentUserId { get; private set; } = string.Empty;

    public void OnGet()
    {
        UserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        CurrentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        CanCreateTasks = User.IsInRole("Superadmin") || User.IsInRole("Admin") || User.IsInRole("Dentist");
        CanEditTasks = User.IsInRole("Superadmin") || User.IsInRole("Admin") || User.IsInRole("Dentist");
        CanDeleteTasks = User.IsInRole("Superadmin") || User.IsInRole("Admin");
    }
}
