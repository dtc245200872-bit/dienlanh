using dienlanh.Data;
using dienlanh.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSession();


// Add services
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Seed a default admin account (only when no admin exists yet).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var hasAdmin = db.Users.Any(u => u.Role != null && u.Role.ToLower() == "admin");
    if (!hasAdmin)
    {
        db.Users.Add(new User
        {
            Name = "System Admin",
            Email = "admin@dienlanh.local",
            Password = "Admin@123",
            Role = "admin"
        });
        db.SaveChanges();
    }

    if (!db.Devices.Any())
    {
        db.Devices.AddRange(
            new Device { Name = "Máy lạnh", Description = "Thiết bị làm lạnh không khí" },
            new Device { Name = "Tủ lạnh", Description = "Thiết bị bảo quản thực phẩm" },
            new Device { Name = "Máy giặt", Description = "Thiết bị giặt quần áo" }
        );
        db.SaveChanges();
    }

    if (!db.Components.Any())
    {
        var mayLanhId = db.Devices.FirstOrDefault(d => d.Name == "Máy lạnh")?.Id;
        var tuLanhId = db.Devices.FirstOrDefault(d => d.Name == "Tủ lạnh")?.Id;

        db.Components.AddRange(
            // Refrigeration components
            new Component { Name = "Block máy nén", Price = 1800000m, DeviceId = mayLanhId },
            new Component { Name = "Dàn nóng", Price = 1200000m, DeviceId = mayLanhId },
            new Component { Name = "Dàn lạnh", Price = 900000m, DeviceId = mayLanhId },
            new Component { Name = "Gas R32", Price = 450000m, DeviceId = mayLanhId },
            new Component { Name = "Quạt dàn lạnh", Price = 250000m, DeviceId = mayLanhId },
            new Component { Name = "Thermostat tủ lạnh", Price = 320000m, DeviceId = tuLanhId },
            new Component { Name = "Ron cửa tủ lạnh", Price = 180000m, DeviceId = tuLanhId },
            new Component { Name = "Cảm biến nhiệt", Price = 180000m, DeviceId = tuLanhId }
        );
        db.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Repair}/{action=Index}/{id?}");

app.Run();
