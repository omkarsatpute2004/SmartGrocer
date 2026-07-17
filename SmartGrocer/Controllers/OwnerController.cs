using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SmartGrocer.Data;
using SmartGrocer.Models;
using System.Linq;

namespace SmartGrocer.Controllers
{
    public class OwnerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OwnerController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsOwner() => HttpContext.Session.GetString("Role") == "Owner";
        private int GetOwnerId() => int.Parse(HttpContext.Session.GetString("UserId") ?? "0");

        private async Task<Shop?> GetMyShop()
        {
            int ownerId = GetOwnerId();
            string? sessionShopId = HttpContext.Session.GetString("ShopId");

            if (!string.IsNullOrEmpty(sessionShopId) && int.TryParse(sessionShopId, out int shopId))
            {
                var shop = await _context.Shops.FirstOrDefaultAsync(s => s.ShopId == shopId && s.OwnerId == ownerId);
                if (shop != null) return shop;
            }

            // Fallback to the first approved shop if none selected or selection invalid
            var firstShop = await _context.Shops.FirstOrDefaultAsync(s => s.OwnerId == ownerId && s.Status == UserStatus.Approved);
            if (firstShop != null)
            {
                HttpContext.Session.SetString("ShopId", firstShop.ShopId.ToString());
            }
            return firstShop;
        }

        public async Task<IActionResult> MyShops()
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");

            var shops = await _context.Shops
                .Where(s => s.OwnerId == GetOwnerId())
                .ToListAsync();

            return View(shops);
        }

        public IActionResult SelectShop(int id)
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            
            // We'll verify ownership in the subsequent actions via GetMyShop, 
            // but setting it here is the first step.
            HttpContext.Session.SetString("ShopId", id.ToString());
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");

            var shops = await _context.Shops.Where(s => s.OwnerId == GetOwnerId()).ToListAsync();
            if (!shops.Any()) return RedirectToAction("ShopProfile");

            var shop = await GetMyShop();
            if (shop == null) 
            {
                if (shops.Any(s => s.Status == UserStatus.Pending))
                {
                    TempData["Info"] = "Your shop registration is pending Admin approval.";
                }
                return RedirectToAction("MyShops");
            }

            var sales = await _context.Sales.Where(s => s.ShopId == shop.ShopId).ToListAsync();
            ViewBag.TotalSales = sales.Sum(s => s.TotalAmount);
            ViewBag.TotalProfit = sales.Sum(s => s.Profit);
            ViewBag.LowStockCount = await _context.Products.CountAsync(p => p.ShopId == shop.ShopId && p.Quantity < 10);
            ViewBag.TotalProducts = await _context.Products.CountAsync(p => p.ShopId == shop.ShopId);
            ViewBag.CurrentShopName = shop.ShopName;

            // Graph data will be fetched dynamically via GetChartData endpoint

            var lowStockProducts = await _context.Products
                .Where(p => p.ShopId == shop.ShopId && p.Quantity < 10)
                .ToListAsync();

            return View(lowStockProducts);
        }

        [HttpGet]
        public async Task<IActionResult> ShopProfile(int? id)
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            
            if (id.HasValue)
            {
                var shop = await _context.Shops.FirstOrDefaultAsync(s => s.ShopId == id && s.OwnerId == GetOwnerId());
                return View(shop);
            }
            
            return View(); // New shop registration
        }

        [HttpPost]
        public async Task<IActionResult> ShopProfile(Shop shop)
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");

            if (shop.ShopId == 0)
            {
                shop.OwnerId = GetOwnerId();
                shop.Status = UserStatus.Pending; // New shops must be approved
                _context.Shops.Add(shop);
                TempData["Success"] = "Shop registration submitted for Admin approval.";
            }
            else
            {
                var existingShop = await _context.Shops.FirstOrDefaultAsync(s => s.ShopId == shop.ShopId && s.OwnerId == GetOwnerId());
                if (existingShop != null)
                {
                    existingShop.ShopName = shop.ShopName;
                    existingShop.Location = shop.Location;
                    TempData["Success"] = "Shop details updated.";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MyShops");
        }

        public async Task<IActionResult> Employees()
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            var shop = await GetMyShop();
            if (shop == null) return RedirectToAction("ShopProfile");

            var employees = await _context.Users
                .Where(u => u.ShopId == shop.ShopId && u.Role == UserRole.Employee)
                .ToListAsync();

            return View(employees);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessEmployee(int id, string action)
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            
            var employee = await _context.Users.FindAsync(id);
            var shop = await GetMyShop();

            if (employee != null && employee.ShopId == shop?.ShopId)
            {
                if (action == "Remove")
                {
                    _context.Users.Remove(employee);
                }
                else
                {
                    employee.Status = action == "Approve" ? UserStatus.Approved : UserStatus.Rejected;
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Employees");
        }

        // --- Categories ---
        public async Task<IActionResult> Categories()
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            var shop = await GetMyShop();
            if (shop == null) return RedirectToAction("ShopProfile");

            var categories = await _context.Categories.Include(c => c.Products)
                                 .Where(c => c.ShopId == shop.ShopId && !c.IsDeleted)
                                 .ToListAsync();
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(string name)
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            var shop = await GetMyShop();
            if (shop != null)
            {
                if (_context.Categories.Any(c => c.ShopId == shop.ShopId && c.Name == name))
                {
                    TempData["Error"] = "Category already exists!";
                    return RedirectToAction("Categories");
                }
                _context.Categories.Add(new Category { Name = name, ShopId = shop.ShopId });
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category added successfully!";
            }
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id)
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            var shop = await GetMyShop();
            if (shop == null) return RedirectToAction("ShopProfile");

            var category = await _context.Categories.Include(c => c.Products)
                                 .FirstOrDefaultAsync(c => c.Id == id && c.ShopId == shop.ShopId);

            if (category != null)
            {
                // To fully delete, we remove associated products first
                if (category.Products != null && category.Products.Any())
                {
                    foreach(var prod in category.Products)
                    {
                        prod.IsDeleted = true;
                    }
                }
                category.IsDeleted = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category and its products deleted permanently!";
            }
            else
            {
                TempData["Error"] = "Category not found or access denied.";
            }
            return RedirectToAction("Categories");
        }

        // --- Products ---
        public async Task<IActionResult> Products()
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            var shop = await GetMyShop();
            if (shop == null) return RedirectToAction("ShopProfile");

            ViewBag.Categories = await _context.Categories.Where(c => c.ShopId == shop.ShopId && !c.IsDeleted).ToListAsync();
            var products = await _context.Products.Include(p => p.Category)
                                 .Where(p => p.ShopId == shop.ShopId && !p.IsDeleted)
                                 .ToListAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            var shop = await GetMyShop();
            if (shop != null)
            {
                product.ShopId = shop.ShopId;
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Products");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            var shop = await GetMyShop();
            if (shop == null) return RedirectToAction("ShopProfile");

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.ShopId == shop.ShopId);
            
            if (product != null)
            {
                product.IsDeleted = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted permanently!";
            }
            else
            {
                TempData["Error"] = "Product not found or access denied.";
            }
            return RedirectToAction("Products");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProductStock(int id, int newQuantity)
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            var shop = await GetMyShop();
            if (shop == null) return RedirectToAction("ShopProfile");

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.ShopId == shop.ShopId);
            
            if (product != null)
            {
                product.Quantity = newQuantity;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product stock updated successfully!";
            }
            else
            {
                TempData["Error"] = "Product not found or access denied.";
            }
            return RedirectToAction("Products");
        }

        // --- Sales ---
        public async Task<IActionResult> Sales()
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            var shop = await GetMyShop();
            if (shop == null) return RedirectToAction("ShopProfile");

            ViewBag.Products = await _context.Products.Where(p => p.ShopId == shop.ShopId && p.Quantity > 0 && !p.IsDeleted).ToListAsync();
            var sales = await _context.Sales.Include(s => s.Product).Where(s => s.ShopId == shop.ShopId).OrderByDescending(s => s.Date).ToListAsync();
            return View(sales);
        }

        [HttpPost]
        public async Task<IActionResult> RecordSale(int productId, int quantity)
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");

            var product = await _context.Products.FindAsync(productId);
            var shop = await GetMyShop();

            if (product != null && shop != null && product.ShopId == shop.ShopId)
            {
                if (product.Quantity < quantity)
                {
                    TempData["Error"] = "Insufficient stock!";
                    return RedirectToAction("Sales");
                }

                decimal totalAmount = product.SellingPrice * quantity;
                decimal totalPurchase = product.PurchasePrice * quantity;
                decimal profit = totalAmount - totalPurchase;

                Sale sale = new Sale
                {
                    ProductId = productId,
                    Quantity = quantity,
                    TotalAmount = totalAmount,
                    Profit = profit,
                    ShopId = shop.ShopId,
                    Date = DateTime.Now
                };

                product.Quantity -= quantity; // Update stock
                _context.Sales.Add(sale);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Sale recorded successfully!";
            }

            return RedirectToAction("Sales");
        }

        public async Task<IActionResult> SalesHistory()
        {
            if (!IsOwner()) return RedirectToAction("Login", "Account");
            var shop = await GetMyShop();
            if (shop == null) return RedirectToAction("ShopProfile");

            var sales = await _context.Sales
                .Include(s => s.Product)
                .Where(s => s.ShopId == shop.ShopId)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            return View(sales);
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string filter = "monthly")
        {
            if (!IsOwner()) return Unauthorized();
            var shop = await GetMyShop();
            if (shop == null) return NotFound();

            var salesQuery = _context.Sales.Where(s => s.ShopId == shop.ShopId);
            
            if (filter == "monthly")
            {
                // X-axis: Days of the current month (or last 30 days)
                var thirtyDaysAgo = DateTime.Now.Date.AddDays(-30);
                
                // Fetch first to avoid SQLite EF translation errors with Date grouping
                var recentSales = await salesQuery
                    .Where(s => s.Date >= thirtyDaysAgo)
                    .ToListAsync();

                var dailyData = recentSales
                    .GroupBy(s => s.Date.Date)
                    .Select(g => new { Date = g.Key, Profit = g.Sum(s => s.Profit) })
                    .OrderBy(d => d.Date)
                    .ToList();
                
                var labels = dailyData.Select(d => d.Date.ToString("dd MMM")).ToList();
                var values = dailyData.Select(d => d.Profit).ToList();
                
                return Json(new { labels, values });
            }
            else
            {
                // X-axis: Months
                // Fetch first to avoid SQLite EF translation errors
                var allSales = await salesQuery.ToListAsync();

                var monthlyData = allSales
                    .GroupBy(s => new { s.Date.Year, s.Date.Month })
                    .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Profit = g.Sum(s => s.Profit) })
                    .OrderBy(m => m.Year).ThenBy(m => m.Month)
                    .ToList();

                // Format labels nicely
                var labels = monthlyData.Select(m => new DateTime(m.Year, m.Month, 1).ToString("MMM yyyy")).ToList();
                var values = monthlyData.Select(m => m.Profit).ToList();

                return Json(new { labels, values });
            }
        }
    }
}
