using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartGrocer.Data;
using SmartGrocer.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace SmartGrocer.Scripts
{
    public class DatabaseVerifier
    {
        public static void Execute()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json");
            var config = builder.Build();

            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(config.GetConnectionString("DefaultConnection")));

            using var serviceProvider = services.BuildServiceProvider();
            using var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            Console.WriteLine("--- USERS ---");
            foreach (var u in context.Users.ToList()) {
                Console.WriteLine($"ID: {u.Id}, Email: {u.Email}, Role: {u.Role}, Status: {u.Status}");
            }

            Console.WriteLine("\n--- SHOPS ---");
            foreach (var s in context.Shops.Include(x => x.Owner).ToList()) {
                Console.WriteLine($"ID: {s.ShopId}, Name: {s.ShopName}, Owner: {s.Owner?.Email}, Status: {s.Status}");
            }
        }
    }
}
