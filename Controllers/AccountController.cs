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

        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: Edit Profile
        public IActionResult Edit()
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // POST: Update Profile
        [HttpPost]
        public IActionResult Edit(User updatedUser)
        {
            var userId = HttpContext.Session.GetInt32("userId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            // Update allowed fields
            user.Name = updatedUser.Name ?? user.Name;
            user.Email = updatedUser.Email ?? user.Email;
            user.Phone = updatedUser.Phone ?? user.Phone;
            user.Address = updatedUser.Address ?? user.Address;

            // Update password only if provided
            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
            {
                user.Password = updatedUser.Password;
            }

            _context.Users.Update(user);
            _context.SaveChanges();

            TempData["Message"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
