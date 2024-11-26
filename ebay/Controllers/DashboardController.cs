using ebay.services;
using Microsoft.AspNetCore.Mvc;

namespace ebay.Controllers;

public class DashboardController : Controller
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var data = await _dashboardService.GetDashboardDataAsync();
        return View(data);
    }
}