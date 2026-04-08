using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QSS.Web.Pages;

[Authorize(Roles = "Superadmin,Admin,Dentist,DentalAssistant")]
public class MedicationsModel : PageModel
{
    public bool CanManageMedications { get; private set; }

    public IActionResult OnGet()
    {
        CanManageMedications = User.IsInRole("Superadmin") || User.IsInRole("Admin") || User.IsInRole("Dentist");
        return Page();
    }
}
