using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QSS.Web.Pages;

[Authorize(Roles = "Superadmin,Admin,Dentist,DentalAssistant")]
public class DevicesModel : PageModel
{
    public bool CanManageDevices { get; private set; }

    public IActionResult OnGet()
    {
        CanManageDevices = User.IsInRole("Superadmin") || User.IsInRole("Admin");
        return Page();
    }
}
