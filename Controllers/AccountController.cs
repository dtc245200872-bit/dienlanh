using Microsoft.AspNetCore.Mvc;
using dienlanh.Data;
using dienlanh.Models;
using System.Linq;

namespace dienlanh.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        public IActionResult Register(User user)
        {
            user.Role = (user.Role ?? string.Empty).Trim().ToLower();
            if (user.Role != "customer" && user.Role != "technician")
            {
                ViewBag.Error = "Vai trò không hợp lệ";
                return View(user);
            }

            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Login");
        }

        // GET: Login
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // 🔥 LUÔN dùng lowercase
                HttpContext.Session.SetString("role", user.Role.Trim().ToLower());
                HttpContext.Session.SetInt32("userId", user.Id);
                HttpContext.Session.SetString("email", user.Email);

                // 🔥 Phân quyền
                if (user.Role.ToLower() == "admin")
                    return RedirectToAction("Dashboard", "Admin");

                if (user.Role.ToLower() == "technician")
                    return RedirectToAction("MyJobs", "Repair");

                if (user.Role.ToLower() == "customer")
                    return RedirectToAction("Home", "Customer");
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
    }
