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

            if (string.IsNullOrEmpty(role) || role.ToLower() != "customer")
                return RedirectToAction("Login", "Account");

            return View();
        }

        // 🔥 Xem yêu cầu của khách


        public IActionResult MyRequests()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (string.IsNullOrEmpty(role) || role != "customer")
                return RedirectToAction("Login", "Account");

            var list = _context.RepairRequests
                .Where(r => r.CustomerId == userId)
                .ToList();

            return View(list); // 🔥 QUAN TRỌNG
        }

        [HttpPost]
        public IActionResult ConfirmParts(int id)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "customer" || userId == null)
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);
            if (request == null) return NotFound();
            if (request.CustomerId != userId) return Forbid();
            if (request.Status != "Chờ khách xác nhận linh kiện")
                return BadRequest("Yêu cầu chưa đến bước xác nhận linh kiện.");
            if (request.CustomerReported && !request.ReportResolved)
                return BadRequest("Yêu cầu đang được hệ thống xử lý báo lỗi.");

            request.PartsConfirmedByCustomer = true;
            request.Status = "Chờ thanh toán";
            _context.SaveChanges();

            return RedirectToAction("MyRequests");
        }

        [HttpPost]
        public IActionResult ReportIssue(int id, string note)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "customer" || userId == null)
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);
            if (request == null) return NotFound();
            if (request.CustomerId != userId) return Forbid();

            if (request.Status != "Chờ khách xác nhận linh kiện" &&
                request.Status != "Chờ thanh toán" &&
                request.Status != "Đã thanh toán")
            {
                return BadRequest("Chưa thể báo lỗi ở trạng thái này.");
            }

            if (string.IsNullOrWhiteSpace(note))
                return BadRequest("Vui lòng nhập nội dung báo lỗi.");

            request.CustomerReported = true;
            request.ReportResolved = false;
            request.CustomerReportNote = note.Trim();
            request.ReportedAt = DateTime.Now;
            request.ReportResolutionNote = null;
            request.ResolvedAt = null;
            request.Status = "Đã báo lỗi";
            _context.SaveChanges();

            return RedirectToAction("MyRequests");
        }
    }
}