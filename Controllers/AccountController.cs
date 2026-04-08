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
                // 🔥 Lưu session (KHÔNG toLower)
                HttpContext.Session.SetString("role", user.Role.Trim());
                HttpContext.Session.SetInt32("userId", user.Id);
                HttpContext.Session.SetString("email", user.Email);

                // 🔥 Phân quyền
                if (user.Role == "Admin")
                    return RedirectToAction("Dashboard", "Admin");

                if (user.Role == "Technician")
                    return RedirectToAction("MyJobs", "Repair");

                if (user.Role == "Customer")
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
