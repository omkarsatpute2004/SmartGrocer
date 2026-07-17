using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartGrocer.Data;
using SmartGrocer.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SmartGrocer.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectBasedOnRole(HttpContext.Session.GetString("Role"));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid Email or Password";
                return View();
            }

            if (user.Status == UserStatus.Pending)
            {
                ViewBag.Error = "Your account is still pending approval.";
                return View();
            }

            if (user.Status == UserStatus.Rejected)
            {
                ViewBag.Error = "Your account registration has been rejected.";
                return View();
            }

            if (user.Status == UserStatus.Blocked)
            {
                ViewBag.Error = "Your account has been blocked by the Administrator.";
                return View();
            }

            // Approved - Set Session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("Role", user.Role.ToString());
            if (user.ShopId.HasValue)
            {
                HttpContext.Session.SetString("ShopId", user.ShopId.Value.ToString());
            }

            return RedirectBasedOnRole(user.Role.ToString());
        }

        [HttpGet]
        public IActionResult RegisterOwner() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterOwner(User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ViewBag.Error = "Email already exists.";
                return View(user);
            }

            user.Role = UserRole.Owner;
            user.Status = UserStatus.Pending;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Registration successful! Waiting for Admin approval.";
            return View("Login");
        }

        [HttpGet]
        public IActionResult RegisterEmployee() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterEmployee(User user, int shopId)
        {
            var shop = await _context.Shops.FindAsync(shopId);
            if (shop == null)
            {
                ViewBag.Error = "Invalid Shop ID.";
                return View(user);
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ViewBag.Error = "Email already exists.";
                return View(user);
            }

            user.Role = UserRole.Employee;
            user.Status = UserStatus.Pending;
            user.ShopId = shopId;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Registration successful! Waiting for Shop Owner approval.";
            return View("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private IActionResult RedirectBasedOnRole(string role)
        {
            return role switch
            {
                "Admin" => RedirectToAction("Dashboard", "Admin"),
                "Owner" => RedirectToAction("Dashboard", "Owner"),
                "Employee" => RedirectToAction("Dashboard", "Employee"),
                _ => RedirectToAction("Login")
            };
        }
    }
}
