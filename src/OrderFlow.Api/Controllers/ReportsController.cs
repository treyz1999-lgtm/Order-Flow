using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(ReportService reports) : ControllerBase
{
    [HttpGet("low-stock")]
    public async Task<ActionResult<List<ProductResponse>>> GetLowStock([FromQuery] int threshold = 10,
        CancellationToken cancellationToken = default)
    {
        if (threshold < 0) return BadRequest("Threshold must be zero or greater.");
        return Ok(await reports.GetLowStockAsync(threshold, cancellationToken));
    }

    [HttpGet("sales")]
    public async Task<ActionResult<SalesSummaryResponse>> GetSales(CancellationToken cancellationToken) =>
        Ok(await reports.GetSalesSummaryAsync(cancellationToken));
}
