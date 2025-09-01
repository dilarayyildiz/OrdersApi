using System.ComponentModel.DataAnnotations;

namespace OrdersApi.DTOs;

public class CreateOrderItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class CreateOrderDto
{
    [Required]
    public int CustomerId { get; set; }

    [MinLength(1, ErrorMessage = "En az bir kalem olmalı.")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}