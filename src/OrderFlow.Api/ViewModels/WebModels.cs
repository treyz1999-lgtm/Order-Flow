using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrderFlow.Api.DTOs;

namespace OrderFlow.Api.ViewModels;

public class ProductForm
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }
}

public class CustomerForm
{
    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = "";
}

public class OrderForm
{
    [Range(1, int.MaxValue, ErrorMessage = "Choose a customer.")]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a product.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    public List<SelectListItem> Customers { get; set; } = [];
    public List<SelectListItem> Products { get; set; } = [];
}

public record HomeViewModel(int ProductCount, int CustomerCount, int OrderCount,
    SalesSummaryResponse Sales, List<ProductResponse> LowStock);
