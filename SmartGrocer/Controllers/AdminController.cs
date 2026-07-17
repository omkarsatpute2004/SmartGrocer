using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartGrocer.Data;
using SmartGrocer.Models;
using System.Linq;

namespace SmartGrocer.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";

        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.TotalOwners = await _context.Users.CountAsync(u => u.Role == UserRole.Owner);
            ViewBag.PendingOwners = await _context.Users.CountAsync(u => u.Role == UserRole.Owner && u.Status == UserStatus.Pending);
            ViewBag.TotalShops = await _context.Shops.CountAsync(s => s.Status == UserStatus.Approved);
            ViewBag.PendingShops = await _context.Shops.CountAsync(s => s.Status == UserStatus.Pending);
            ViewBag.TotalEmployees = await _context.Users.CountAsync(u => u.Role == UserRole.Employee);

            return View();
        }

        public async Task<IActionResult> ApproveOwners()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var pendingOwners = await _context.Users
                .Where(u => u.Role == UserRole.Owner && u.Status == UserStatus.Pending)
                .ToListAsync();

            return View(pendingOwners);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessOwner(int id, string action)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var owner = await _context.Users.FindAsync(id);
            if (owner != null)
            {
                if (action == "Approve") owner.Status = UserStatus.Approved;
                else if (action == "Reject") owner.Status = UserStatus.Rejected;
                else if (action == "Block") owner.Status = UserStatus.Blocked;
                else if (action == "Unblock") owner.Status = UserStatus.Approved;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ManageOwners");
        }

        public async Task<IActionResult> ManageOwners()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var owners = await _context.Users
                .Where(u => u.Role == UserRole.Owner)
                .ToListAsync();

            return View(owners);
        }

        public async Task<IActionResult> ApproveShops()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var pendingShops = await _context.Shops
                .Include(s => s.Owner)
                .Where(s => s.Status == UserStatus.Pending)
                .ToListAsync();

            return View(pendingShops);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessShop(int id, string action)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var shop = await _context.Shops.FindAsync(id);
            if (shop != null)
            {
                if (action == "Approve")
                {
                    shop.Status = UserStatus.Approved;
                }
                else if (action == "Reject")
                {
                    shop.Status = UserStatus.Rejected;
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ApproveShops");
        }

        public async Task<IActionResult> Shops()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var shops = await _context.Shops
                .Include(s => s.Owner)
                .ToListAsync();

            return View(shops);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveShop(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var shop = await _context.Shops.FindAsync(id);
            if (shop != null)
            {
                // Explicitly delete sales associated with this shop to avoid foreign key constraints
                var sales = _context.Sales.Where(s => s.ShopId == id);
                _context.Sales.RemoveRange(sales);

                // Explicitly delete products associated with this shop
                var products = _context.Products.Where(p => p.ShopId == id);
                _context.Products.RemoveRange(products);

                // Explicitly delete categories associated with this shop
                var categories = _context.Categories.Where(c => c.ShopId == id);
                _context.Categories.RemoveRange(categories);

                // Unlink employees from this shop
                var employees = _context.Users.Where(u => u.ShopId == id);
                foreach (var emp in employees)
                {
                    emp.ShopId = null;
                }

                _context.Shops.Remove(shop);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Shops");
        }
    }
}
