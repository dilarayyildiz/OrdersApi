using Microsoft.EntityFrameworkCore;
using OrdersApi.Data;
using OrdersApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("ecommerce"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// --- otomatik seed ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Products.Any())
    {
        db.Products.AddRange(
            new Product { Name = "Laptop",  Price = 1500m, Stock = 5  },
            new Product { Name = "Telefon", Price =  700m, Stock = 10 },
            new Product { Name = "Kulaklık",Price =  120m, Stock = 20 }
        );
        db.SaveChanges();
    }
}
// --- otomatik seed son ---
app.MapControllers();

app.Run();