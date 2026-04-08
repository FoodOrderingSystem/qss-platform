using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QSS.Web.Pages;

[Authorize]
public class ProcessesModel : PageModel
{
    public bool CanManageProcesses { get; private set; }

    public void OnGet()
    {
        CanManageProcesses = User.IsInRole("Superadmin") || User.IsInRole("Admin");
    }
}
