using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QSS.Infrastructure.Services;

namespace QSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _dashboardService.GetDashboardAsync();
        return Ok(dashboard);
    }
}
