using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QSS.Web.Pages;

[Authorize(Roles = "Superadmin,Admin,Dentist")]
public class ReportsModel : PageModel
{
    public IActionResult OnGet() => Page();
}
