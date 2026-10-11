using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Services;

public class CustomerService(OrderFlowDbContext db)
{
    public async Task<List<CustomerResponse>> GetCustomersAsync(CancellationToken cancellationToken) =>
        await db.Customers.AsNoTracking().OrderBy(c => c.Id)
            .Select(c => new CustomerResponse(c.Id, c.Name, c.Email))
            .ToListAsync(cancellationToken);

    public async Task<CustomerResponse?> GetCustomerAsync(int id, CancellationToken cancellationToken) =>
        await db.Customers.AsNoTracking().Where(c => c.Id == id)
            .Select(c => new CustomerResponse(c.Id, c.Name, c.Email))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<CustomerResponse?> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Customers.AnyAsync(c => c.Email == email, cancellationToken)) return null;
        var customer = new Customer { Name = request.Name.Trim(), Email = email };
        db.Customers.Add(customer);
        await db.SaveChangesAsync(cancellationToken);
        return new CustomerResponse(customer.Id, customer.Name, customer.Email);
    }
}
