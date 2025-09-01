namespace OrdersApi.DTOs;

public record OrderSummaryDto(
    int Id,
    int CustomerId,
    DateTime CreatedAt,
    decimal Total
);