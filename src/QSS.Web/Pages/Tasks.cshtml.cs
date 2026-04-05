using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QSS.Web.Pages;

[Authorize]
public class TasksModel : PageModel
{
    public void OnGet() { }
}
