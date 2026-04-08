using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QSS.Web.Pages;

[Authorize(Roles = "Superadmin,Admin")]
public class UsersModel : PageModel
{
    public bool IsSuperadmin { get; private set; }

    public IActionResult OnGet()
    {
        IsSuperadmin = User.IsInRole("Superadmin");
        return Page();
    }
}
