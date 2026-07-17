using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartGrocer.Data;
using SmartGrocer.Models;
using System.Linq;

namespace SmartGrocer.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsEmployee() => HttpContext.Session.GetString("Role") == "Employee";
        private int GetShopId() => int.Parse(HttpContext.Session.GetString("ShopId") ?? "0");

        public async Task<IActionResult> Dashboard()
        {
            if (!IsEmployee()) return RedirectToAction("Login", "Account");

            int shopId = GetShopId();
            var shop = await _context.Shops.FindAsync(shopId);
            if (shop == null) return RedirectToAction("Login", "Account");

            ViewBag.ShopName = shop.ShopName;
            ViewBag.TotalProducts = await _context.Products.CountAsync(p => p.ShopId == shopId);
            ViewBag.LowStockCount = await _context.Products.CountAsync(p => p.ShopId == shopId && p.Quantity < 10);
            
            // Total sales handled for this shop
            ViewBag.TotalSalesCount = await _context.Sales.CountAsync(s => s.ShopId == shopId);

            return View();
        }

        public async Task<IActionResult> Stock()
        {
            if (!IsEmployee()) return RedirectToAction("Login", "Account");

            int shopId = GetShopId();
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.ShopId == shopId)
                .ToListAsync();

            return View(products);
        }
    }
}
