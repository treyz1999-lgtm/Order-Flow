using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Controllers;

[Route("reports")]
public class WebReportsController(ReportService reports) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] int threshold = 10,
        CancellationToken cancellationToken = default)
    {
        threshold = Math.Clamp(threshold, 0, 1000000);
        ViewBag.Threshold = threshold;
        ViewBag.LowStock = await reports.GetLowStockAsync(threshold, cancellationToken);
        return View(await reports.GetSalesSummaryAsync(cancellationToken));
    }
}
