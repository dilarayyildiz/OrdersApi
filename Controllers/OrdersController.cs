using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersApi.Data;
using OrdersApi.DTOs;
using OrdersApi.Models;

namespace OrdersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(AppDbContext db) : ControllerBase
{
    // POST /api/orders  → Yeni sipariş ekle (stok kontrol + stok düşme)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        // İstenen ürünleri al
        var ids = req.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await db.Products.Where(p => ids.Contains(p.Id)).ToListAsync();

        // Ürün ve stok kontrolleri
        foreach (var item in req.Items)
        {
            var p = products.FirstOrDefault(x => x.Id == item.ProductId);
            if (p is null) return BadRequest($"Ürün bulunamadı: {item.ProductId}");
            if (p.Stock < item.Quantity) return BadRequest($"Stok yetersiz: {p.Name} (stok: {p.Stock})");
        }

        // Siparişi oluştur
        var order = new Order { CustomerId = req.CustomerId };

        foreach (var item in req.Items)
        {
            var p = products.First(x => x.Id == item.ProductId);
            p.Stock -= item.Quantity; // stoktan düş

            order.Items.Add(new OrderItem
            {
                ProductId = p.Id,
                Quantity = item.Quantity,
                UnitPrice = p.Price
            });
        }

        order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, new { order.Id, order.Total });
    }

    // GET /api/orders?customerId=42  → kullanıcının siparişleri
    [HttpGet]
    public async Task<IActionResult> GetByCustomer([FromQuery] int customerId)
    {
        var list = await db.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummaryDto(o.Id, o.CustomerId, o.CreatedAt, o.Total))
            .ToListAsync();

        return Ok(list);
    }

    // GET /api/orders/5  → sipariş detayı
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await db.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();

        var productIds = order.Items.Select(i => i.ProductId).ToList();
        var names = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name);

        var dto = new OrderDetailDto(
            order.Id,
            order.CustomerId,
            order.CreatedAt,
            order.Total,
            order.Items.Select(i =>
                new OrderDetailItemDto(
                    i.ProductId,
                    names.GetValueOrDefault(i.ProductId, "Unknown"),
                    i.Quantity,
                    i.UnitPrice
                )).ToList()
        );

        return Ok(dto);
    }

    // DELETE /api/orders/5  → sipariş sil
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        db.OrderItems.RemoveRange(order.Items);
        db.Orders.Remove(order);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
