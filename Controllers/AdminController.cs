using dienlanh.Data;
using dienlanh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dienlanh.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // 🔥 Dashboard
        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            return View();
        }

        // 🔥 Danh sách yêu cầu
        public IActionResult Requests()
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            var list = _context.RepairRequests
                .Include(r => r.Technician) // 🔥 FIX QUAN TRỌNG
                .ToList();

            return View(list);
        }

        // 🔥 Trang phân công
        public IActionResult Assign(int id)
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);

            if (request == null)
                return NotFound();

            var technicians = _context.Users
                .Where(u => u.Role == "Technician")
                .ToList();

            ViewBag.Techs = technicians;
            ViewBag.Request = request;

            return View();
        }

        // 🔥 Xử lý phân công
        [HttpPost]
        public IActionResult Assign(int requestId, int technicianId)
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            var req = _context.RepairRequests.Find(requestId);

            if (req == null)
                return NotFound();

            req.TechnicianId = technicianId;
            req.Status = "Đã phân công"; // 🔥 chuẩn hơn

            _context.SaveChanges();

            return RedirectToAction("Requests");
        }
    }
}