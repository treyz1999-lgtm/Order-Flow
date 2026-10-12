using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Services;
using OrderFlow.Api.ViewModels;

namespace OrderFlow.Api.Controllers;

[Route("customers")]
public class WebCustomersController(CustomerService customers) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await customers.GetCustomersAsync(cancellationToken));

    [HttpGet("new")]
    public IActionResult Create() => View(new CustomerForm());

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(form);
        try
        {
            var customer = await customers.CreateCustomerAsync(
                new CreateCustomerRequest(form.Name, form.Email), cancellationToken);
            if (customer is null)
            {
                ModelState.AddModelError(nameof(form.Email), "That email is already in use.");
                return View(form);
            }
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(form.Email), "That email is already in use.");
            return View(form);
        }
        TempData["Message"] = "Customer added.";
        return RedirectToAction(nameof(Index));
    }
}
