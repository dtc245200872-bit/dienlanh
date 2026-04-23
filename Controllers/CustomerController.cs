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

        [HttpPost]
        public IActionResult SubmitTechnicianRating(int id, int rating, string comment)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "customer" || userId == null)
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);
            if (request == null) return NotFound();
            if (request.CustomerId != userId) return Forbid();

            if (request.Status != "Đã thanh toán")
                return BadRequest("Bạn chỉ có thể đánh giá sau khi xác nhận thanh toán.");
            if (request.TechnicianId == null)
                return BadRequest("Yêu cầu chưa có kỹ thuật viên để đánh giá.");
            if (request.TechnicianRating.HasValue)
                return BadRequest("Bạn đã đánh giá kỹ thuật viên cho yêu cầu này.");
            if (rating < 1 || rating > 5)
                return BadRequest("Điểm đánh giá phải từ 1 đến 5 sao.");
            if (string.IsNullOrWhiteSpace(comment))
                return BadRequest("Vui lòng nhập nhận xét về kỹ thuật viên.");

            request.TechnicianRating = rating;
            request.TechnicianRatingComment = comment.Trim();
            request.TechnicianRatedAt = DateTime.Now;
            _context.SaveChanges();

            return RedirectToAction("MyRequests");
        }

        // Cancel Request - if admin hasn't confirmed and assigned
        public IActionResult CancelRequest()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (string.IsNullOrEmpty(role) || role != "customer")
                return RedirectToAction("Login", "Account");

            var list = _context.RepairRequests
                .Where(r => r.CustomerId == userId && 
                       (r.Status == "Chờ xử lý" || r.Status == "Đã phân công"))
                .ToList();

            return View(list);
        }

        [HttpPost]
        public IActionResult CancelRequestConfirm(int id)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "customer" || userId == null)
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);
            if (request == null) return NotFound();
            if (request.CustomerId != userId) return Forbid();

            // Only allow cancellation if not yet confirmed/assigned
            if (request.Status != "Chờ xử lý" && request.Status != "Đã phân công")
                return BadRequest("Không thể hủy yêu cầu ở trạng thái này.");

            request.Status = "Đã hủy";
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Yêu cầu đã được hủy thành công.";
            return RedirectToAction("MyRequests");
        }

        // Update Status from Technician
        public IActionResult UpdateStatus()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (string.IsNullOrEmpty(role) || role != "customer")
                return RedirectToAction("Login", "Account");

            var list = _context.RepairRequests
                .Where(r => r.CustomerId == userId)
                .OrderByDescending(r => r.PreferredVisitAt)
                .ToList();

            return View(list);
        }

        // View Reviews (separated from MyRequests)
        public IActionResult Reviews()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (string.IsNullOrEmpty(role) || role != "customer")
                return RedirectToAction("Login", "Account");

            var reviewedRequests = _context.RepairRequests
                .Where(r => r.CustomerId == userId && r.TechnicianRating.HasValue)
                .OrderByDescending(r => r.TechnicianRatedAt)
                .ToList();

            return View(reviewedRequests);
        }

        // Report History
        public IActionResult ReportHistory()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (string.IsNullOrEmpty(role) || role != "customer")
                return RedirectToAction("Login", "Account");

            var reportedRequests = _context.RepairRequests
                .Where(r => r.CustomerId == userId && r.CustomerReported)
                .OrderByDescending(r => r.ReportedAt)
                .ToList();

            return View(reportedRequests);
        }

        // Customer Support Page
        public IActionResult Support()
        {
            var role = HttpContext.Session.GetString("role");

            if (string.IsNullOrEmpty(role) || role != "customer")
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}