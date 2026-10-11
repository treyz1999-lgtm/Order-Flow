namespace OrderFlow.Api.DTOs;

public record ProductResponse(int Id, string Name, decimal Price, int StockQuantity);
