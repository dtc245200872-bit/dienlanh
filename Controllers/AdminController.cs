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

            if (role != "admin")
                return RedirectToAction("Login", "Account");

            var stats = BuildStatistics();
            return View(stats);
        }

        // 🔥 Danh sách yêu cầu
        public IActionResult Requests()
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "admin")
                return RedirectToAction("Login", "Account");

            var list = _context.RepairRequests
                .Include(r => r.Technician) // 🔥 FIX QUAN TRỌNG
                .ToList();

            return View(list);
        }

        public IActionResult Reports()
        {
            var role = HttpContext.Session.GetString("role");
            if (role != "admin")
                return RedirectToAction("Login", "Account");

            var reports = _context.RepairRequests
                .Where(r => r.CustomerReported)
                .OrderByDescending(r => r.ReportedAt)
                .ToList();

            return View(reports);
        }

        public IActionResult Statistics()
        {
            var role = HttpContext.Session.GetString("role");
            if (role != "admin")
                return RedirectToAction("Login", "Account");

            var stats = BuildStatistics();
            return View(stats);
        }

        [HttpPost]
        public IActionResult ResolveReport(int id, string resolutionNote)
        {
            var role = HttpContext.Session.GetString("role");
            if (role != "admin")
                return RedirectToAction("Login", "Account");

            var req = _context.RepairRequests.Find(id);
            if (req == null) return NotFound();
            if (!req.CustomerReported) return BadRequest("Yêu cầu chưa có báo lỗi.");

            req.ReportResolved = true;
            req.ReportResolutionNote = string.IsNullOrWhiteSpace(resolutionNote)
                ? "Đã xử lý theo quy trình."
                : resolutionNote.Trim();
            req.ResolvedAt = DateTime.Now;

            // Return to relevant workflow after resolving report.
            if (req.PartsConfirmedByCustomer && req.Status == "Đã báo lỗi")
                req.Status = "Chờ thanh toán";
            else if (!req.PartsConfirmedByCustomer && req.Status == "Đã báo lỗi")
                req.Status = "Chờ khách xác nhận linh kiện";

            _context.SaveChanges();
            return RedirectToAction("Reports");
        }

        // 🔥 Trang phân công
        public IActionResult Assign(int id)
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "admin")
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);

            if (request == null)
                return NotFound();

            var technicians = _context.Users
                .Where(u => u.Role != null && u.Role.ToLower() == "technician")
                .ToList();

            ViewBag.Techs = technicians;
            ViewBag.Request = request;
            ViewBag.RequestId = request.Id;

            return View();
        }

        [HttpPost]
        public IActionResult Review(int id, string decision)
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "admin")
                return RedirectToAction("Login", "Account");

            var req = _context.RepairRequests.Find(id);
            if (req == null)
                return NotFound();

            if (decision == "approve")
            {
                req.Status = "Đã duyệt";
            }
            else if (decision == "reject")
            {
                req.Status = "Từ chối";
            }
            else
            {
                return BadRequest("Quyết định không hợp lệ");
            }

            _context.SaveChanges();
            return RedirectToAction("Requests");
        }

        // 🔥 Xử lý phân công
        [HttpPost]
        public IActionResult Assign(int requestId, int technicianId)
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "admin")
                return RedirectToAction("Login", "Account");

            var req = _context.RepairRequests.Find(requestId);

            if (req == null)
                return NotFound();

            if (req.Status != "Đã duyệt")
                return BadRequest("Yêu cầu chưa được duyệt để phân công");

            req.TechnicianId = technicianId;
            req.Status = "Đã phân công"; // 🔥 chuẩn hơn

            _context.SaveChanges();

            return RedirectToAction("Requests");
        }

        private AdminStatisticsViewModel BuildStatistics()
        {
            var requestQuery = _context.RepairRequests.AsQueryable();
            var paidStatuses = new[] { "Đã thanh toán" };
            var completedStatuses = new[] { "Hoàn thành", "Đã hoàn thành", "Đã thanh toán" };

            return new AdminStatisticsViewModel
            {
                TotalRequests = requestQuery.Count(),
                PendingRequests = requestQuery.Count(r => r.Status == "Chờ xử lý"),
                CompletedRequests = requestQuery.Count(r => r.Status != null && completedStatuses.Contains(r.Status)),
                PaidRequests = requestQuery.Count(r => r.Status != null && paidStatuses.Contains(r.Status)),
                TotalCustomers = _context.Users.Count(u => u.Role != null && u.Role.ToLower() == "customer"),
                TotalTechnicians = _context.Users.Count(u => u.Role != null && u.Role.ToLower() == "technician"),
                TotalRevenue = requestQuery
                    .Where(r => r.Status != null && paidStatuses.Contains(r.Status))
                    .Sum(r => r.FinalAmount ?? 0m),
                DeviceBreakdown = requestQuery
                    .GroupBy(r => r.DeviceType ?? "Chưa rõ")
                    .Select(g => new DeviceStatsItem
                    {
                        DeviceType = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                StatusBreakdown = requestQuery
                    .GroupBy(r => r.Status ?? "Chưa cập nhật")
                    .Select(g => new StatusStatsItem
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList()
            };
        }
    }
}