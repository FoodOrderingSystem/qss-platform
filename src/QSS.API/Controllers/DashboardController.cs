using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QSS.Infrastructure.Services;

namespace QSS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(DashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var dashboard = await _dashboardService.GetDashboardAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in GET /api/dashboard");
            return Ok(new QSS.Application.DTOs.DashboardDto());
        }
    }
}
