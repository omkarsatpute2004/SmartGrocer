using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartGrocer.Data;
using SmartGrocer.Models;

class Program2 {
    static void Main(string[] args) {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite("Data Source=SmartGrocer.db");
        
        using (var context = new ApplicationDbContext(optionsBuilder.Options)) {
            Console.WriteLine("=========================================================================");
            Console.WriteLine("Role | Email | Password | Name | Status");
            Console.WriteLine("-------------------------------------------------------------------------");
            foreach (var user in context.Users) {
                Console.WriteLine($"{user.Role} | {user.Email} | {user.Password} | {user.Name} | {user.Status}");
            }
            Console.WriteLine("=========================================================================");
        }
    }
}
