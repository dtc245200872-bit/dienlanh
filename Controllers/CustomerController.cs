using Microsoft.AspNetCore.Mvc;
using dienlanh.Data;
using dienlanh.Models;
using System.Linq;

namespace dienlanh.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext _context; // 🔥 THÊM DÒNG NÀY

        public CustomerController(AppDbContext context)
        {
            _context = context; // 🔥 THÊM DÒNG NÀY
        }

        public IActionResult Home()
        {
            var role = HttpContext.Session.GetString("role");

            if (string.IsNullOrEmpty(role) || role != "Customer")
                return RedirectToAction("Login", "Account");

            return View();
        }

        // 🔥 Xem yêu cầu của khách
        public IActionResult MyRequests()
        {
            var role = HttpContext.Session.GetString("role");
            var email = HttpContext.Session.GetString("email");

            if (role != "Customer")
                return RedirectToAction("Login", "Account");

            var list = _context.RepairRequests
                .Where(r => r.Email == email)
                .ToList();

            return View(list);
        }

        public IActionResult MyRequests()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "Customer")
                return RedirectToAction("Login", "Account");

            var list = _context.RepairRequests
                .Where(r => r.CustomerId == userId)
                .ToList();

            return View(list);
        }
    }
}