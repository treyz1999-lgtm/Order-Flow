using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController(CustomerService customers) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CustomerResponse>>> GetCustomers(CancellationToken cancellationToken) =>
        Ok(await customers.GetCustomersAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> GetCustomer(int id, CancellationToken cancellationToken)
    {
        var customer = await customers.GetCustomerAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> CreateCustomer(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await customers.CreateCustomerAsync(request, cancellationToken);
            return customer is null
                ? Conflict("A customer with that email already exists.")
                : CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }
        catch (DbUpdateException)
        {
            return Conflict("A customer with that email already exists.");
        }
    }
}
