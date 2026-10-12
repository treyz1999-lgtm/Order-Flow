using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Services;
using OrderFlow.Api.ViewModels;

namespace OrderFlow.Api.Controllers;

public class HomeController(ProductService products, CustomerService customers,
    OrderService orders, ReportService reports) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var productList = await products.GetProductsAsync(cancellationToken);
        var customerList = await customers.GetCustomersAsync(cancellationToken);
        var orderList = await orders.GetOrdersAsync(cancellationToken);
        var sales = await reports.GetSalesSummaryAsync(cancellationToken);
        var lowStock = await reports.GetLowStockAsync(10, cancellationToken);
        return View(new HomeViewModel(productList.Count, customerList.Count,
            orderList.Count, sales, lowStock));
    }
}
