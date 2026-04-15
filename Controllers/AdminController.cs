using dienlanh.Data;
using dienlanh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace dienlanh.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("role");
            return role == "admin";
        }

        // 🔥 Dashboard
        public IActionResult Dashboard()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var stats = BuildStatistics();
            return View(stats);
        }

        // 🔥 Danh sách yêu cầu
        public IActionResult Requests()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var list = _context.RepairRequests
                .Include(r => r.Technician) // 🔥 FIX QUAN TRỌNG
                .ToList();

            return View(list);
        }

        public IActionResult Reports()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var reports = _context.RepairRequests
                .Where(r => r.CustomerReported)
                .OrderByDescending(r => r.ReportedAt)
                .ToList();

            return View(reports);
        }

        public IActionResult Statistics()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var stats = BuildStatistics();
            return View(stats);
        }

        [HttpPost]
        public IActionResult ResolveReport(int id, string resolutionNote)
        {
            if (!IsAdmin())
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
            if (!IsAdmin())
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
            if (!IsAdmin())
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
            if (!IsAdmin())
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

        // Staff management
        public IActionResult Staff()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var staff = _context.Users
                .Where(u => u.Role != null && (u.Role.ToLower() == "technician" || u.Role.ToLower() == "admin"))
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Email)
                .ToList();

            return View(staff);
        }

        public IActionResult CreateStaff()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            return View(new StaffFormViewModel { Role = "technician" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateStaff(StaffFormViewModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            model.Role = (model.Role ?? string.Empty).Trim().ToLower();
            if (model.Role != "technician" && model.Role != "admin")
                ModelState.AddModelError(nameof(model.Role), "Vai trò phải là technician hoặc admin.");

            if (string.IsNullOrWhiteSpace(model.Password))
                ModelState.AddModelError(nameof(model.Password), "Vui lòng nhập mật khẩu.");

            if (_context.Users.Any(u => u.Email == model.Email))
                ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại.");

            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                Name = model.Name?.Trim(),
                Phone = model.Phone?.Trim(),
                Specializations = model.Specializations?.Trim(),
                Email = model.Email.Trim(),
                Password = model.Password,
                Role = model.Role
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction(nameof(Staff));
        }

        public IActionResult EditStaff(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            var model = new StaffFormViewModel
            {
                Id = user.Id,
                Name = user.Name ?? string.Empty,
                Phone = user.Phone ?? string.Empty,
                Specializations = user.Specializations ?? string.Empty,
                Email = user.Email,
                Role = user.Role ?? "technician"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditStaff(StaffFormViewModel model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.Id == model.Id);
            if (user == null)
                return NotFound();

            model.Role = (model.Role ?? string.Empty).Trim().ToLower();
            if (model.Role != "technician" && model.Role != "admin")
                ModelState.AddModelError(nameof(model.Role), "Vai trò phải là technician hoặc admin.");

            if (_context.Users.Any(u => u.Email == model.Email && u.Id != model.Id))
                ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại.");

            if (!string.IsNullOrWhiteSpace(model.Password) && model.Password.Length < 6)
                ModelState.AddModelError(nameof(model.Password), "Mật khẩu mới phải có ít nhất 6 ký tự.");

            if (!ModelState.IsValid)
                return View(model);

            user.Name = model.Name?.Trim();
            user.Phone = model.Phone?.Trim();
            user.Specializations = model.Specializations?.Trim();
            user.Email = model.Email.Trim();
            user.Role = model.Role;

            // Keep existing password if admin leaves the password input empty.
            if (!string.IsNullOrWhiteSpace(model.Password))
                user.Password = model.Password;

            _context.SaveChanges();

            return RedirectToAction(nameof(Staff));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStaff(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var currentUserId = HttpContext.Session.GetInt32("userId");
            if (currentUserId == id)
            {
                TempData["StaffError"] = "Không thể tự xóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Staff));
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            var role = (user.Role ?? string.Empty).ToLower();
            if (role != "technician" && role != "admin")
            {
                TempData["StaffError"] = "Chỉ được xóa tài khoản nhân viên.";
                return RedirectToAction(nameof(Staff));
            }

            // If staff is assigned to repair requests, unassign first to satisfy FK constraints.
            var assignedRequests = _context.RepairRequests
                .Where(r => r.TechnicianId == id)
                .ToList();

            foreach (var request in assignedRequests)
            {
                request.TechnicianId = null;
                if (request.Status == "Đã phân công")
                    request.Status = "Đã duyệt";
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            TempData["StaffSuccess"] = "Đã xóa nhân viên thành công.";
            return RedirectToAction(nameof(Staff));
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

    public class StaffFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập chuyên môn sửa chữa.")]
        public string Specializations { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        public string Role { get; set; } = "technician";
    }
}