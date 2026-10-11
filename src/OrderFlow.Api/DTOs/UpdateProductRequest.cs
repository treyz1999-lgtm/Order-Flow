using System.ComponentModel.DataAnnotations;

namespace OrderFlow.Api.DTOs;

public record UpdateProductRequest(
    [Required, StringLength(100)] string Name,
    [Range(typeof(decimal), "0.01", "1000000")] decimal Price,
    [Range(0, int.MaxValue)] int StockQuantity);
