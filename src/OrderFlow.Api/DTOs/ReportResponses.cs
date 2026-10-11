namespace OrderFlow.Api.DTOs;

public record SalesSummaryResponse(int OrderCount, int UnitsOrdered, decimal BookedRevenue);
