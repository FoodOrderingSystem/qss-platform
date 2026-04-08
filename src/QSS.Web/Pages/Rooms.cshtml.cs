using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QSS.Web.Pages;

[Authorize(Roles = "Superadmin,Admin,Dentist,DentalAssistant")]
public class RoomsModel : PageModel
{
    public bool CanManageRooms { get; private set; }

    public IActionResult OnGet()
    {
        CanManageRooms = User.IsInRole("Superadmin") || User.IsInRole("Admin");
        return Page();
    }
}
