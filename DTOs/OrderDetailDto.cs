namespace OrdersApi.DTOs;
public record OrderDetailItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record OrderDetailDto(
    int Id,
    int CustomerId,
    DateTime CreatedAt,
    decimal Total,
    List<OrderDetailItemDto> Items);