using Microsoft.EntityFrameworkCore;
using SmartGrocer.Models;
using System.Linq;

namespace SmartGrocer.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.Migrate();

            // Check if admin exists
            if (context.Users.Any(u => u.Role == UserRole.Admin))
            {
                return; // Admin already exists
            }

            var admin = new User
            {
                Name = "System Admin",
                Email = "admin@grocery.com",
                Password = "Admin123!", // In production, this should be hashed
                Role = UserRole.Admin,
                Status = UserStatus.Approved
            };

            context.Users.Add(admin);
            context.SaveChanges();
        }
    }
}
