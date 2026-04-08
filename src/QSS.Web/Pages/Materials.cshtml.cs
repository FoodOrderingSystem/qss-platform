using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QSS.Web.Pages;

[Authorize(Roles = "Superadmin,Admin,Dentist,DentalAssistant")]
public class MaterialsModel : PageModel
{
    public bool CanManageMaterials { get; private set; }

    public IActionResult OnGet()
    {
        CanManageMaterials = User.IsInRole("Superadmin") || User.IsInRole("Admin");
        return Page();
    }
}
