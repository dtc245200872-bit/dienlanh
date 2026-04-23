using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dienlanh.Models;
using dienlanh.Data;

namespace dienlanh.Controllers
{
    public class RepairController : Controller
    {
        private readonly AppDbContext _context;
        private static readonly Dictionary<string, decimal> ComponentPrices = new()
        {
            { "Tụ điện", 120000m },
            { "Quạt dàn lạnh", 250000m },
            { "Bo mạch", 450000m },
            { "Cảm biến nhiệt", 180000m },
            { "Ống đồng", 300000m }
        };

        public RepairController(AppDbContext context)
        {
            _context = context;
        }

        // 🔥 Danh sách (Admin + Technician)
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "technician" && role != "admin")
                return RedirectToAction("Login", "Account");

            var list = _context.RepairRequests
                .Include(r => r.Technician)
                .ToList();

            return View(list);
        }

        // 🔥 Form tạo
        public IActionResult Create()
        {
            return View(new RepairRequest());
        }

        [HttpPost]
        public IActionResult Create(RepairRequest request, IFormFile imageFile)
        {
            var role = HttpContext.Session.GetString("role");

            if (string.IsNullOrEmpty(role) || role != "customer")
                return RedirectToAction("Login", "Account");

            if (!request.PreferredVisitAt.HasValue)
            {
                ViewBag.Error = "Vui lòng chọn ngày giờ mong muốn.";
                return View(request);
            }

            var preferredTime = request.PreferredVisitAt.Value;
            var isInWorkingWindow =
                preferredTime.TimeOfDay >= new TimeSpan(8, 0, 0) &&
                preferredTime.TimeOfDay <= new TimeSpan(19, 0, 0);

            if (!isInWorkingWindow)
            {
                ViewBag.Error = "Khung giờ hỗ trợ là từ 08:00 đến 19:00 mỗi ngày.";
                return View(request);
            }

            // 🔥 GÁN CUSTOMER
            request.CustomerId = HttpContext.Session.GetInt32("userId");

            // 🔥 Upload ảnh
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                request.ImagePath = "/images/" + fileName;
            }

            request.Status = "Chờ xử lý";

            _context.RepairRequests.Add(request);
            _context.SaveChanges();

            // 🔥 CHUYỂN ĐÚNG TRANG
            return RedirectToAction("MyRequests", "Customer");
        }
        // 🔥 Công việc kỹ thuật viên
        public IActionResult MyJobs()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "technician" || userId == null)
                return RedirectToAction("Login", "Account");

            // Pending jobs: assigned by admin, waiting technician acceptance.
            var jobs = _context.RepairRequests
                .Where(r => r.TechnicianId == userId && r.Status == "Đã phân công")
                .ToList();

            // History: jobs technician accepted/handled.
            var jobHistory = _context.RepairRequests
                .Where(r => r.TechnicianId == userId && r.Status != "Đã phân công")
                .ToList();

            ViewBag.JobHistory = jobHistory;
            return View(jobs);
        }

        // 🔥 Technician Dashboard
        public IActionResult TechnicianDashboard()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "technician" || userId == null)
                return RedirectToAction("Login", "Account");

            var technician = _context.Users.Find(userId);
            if (technician == null)
                return NotFound();

            var today = DateTime.Now.Date;

            // Assigned tasks (waiting to be accepted)
            var assignedTasks = _context.RepairRequests
                .Where(r => r.TechnicianId == userId && r.Status == "Đã phân công")
                .OrderBy(r => r.PreferredVisitAt)
                .ToList();

            // Work history (completed and other statuses)
            var workHistory = _context.RepairRequests
                .Where(r => r.TechnicianId == userId && r.Status != "Đã phân công")
                .OrderByDescending(r => r.PreferredVisitAt)
                .ToList();

            // Statistics
            int totalCompleted = _context.RepairRequests
                .Count(r => r.TechnicianId == userId && 
                       (r.Status == "Hoàn thành" || r.Status == "Đã thanh toán"));

            int completedToday = _context.RepairRequests
                .Count(r => r.TechnicianId == userId && 
                       (r.Status == "Hoàn thành" || r.Status == "Đã thanh toán") &&
                       r.PreferredVisitAt.HasValue && r.PreferredVisitAt.Value.Date == today);

            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            int completedThisMonth = _context.RepairRequests
                .Count(r => r.TechnicianId == userId && 
                       (r.Status == "Hoàn thành" || r.Status == "Đã thanh toán") &&
                       r.PreferredVisitAt.HasValue && r.PreferredVisitAt.Value >= monthStart);

            var model = new TechnicianDashboardViewModel
            {
                TechnicianProfile = technician,
                AssignedTasks = assignedTasks,
                WorkHistory = workHistory.Take(10).ToList(), // Latest 10 jobs
                TotalAssignedTasks = assignedTasks.Count,
                TotalCompletedToday = completedToday,
                TotalCompletedThisMonth = completedThisMonth,
                UnreadNotifications = 0, // Placeholder for future notifications feature
                Notifications = new(),
                Schedule = new()
                {
                    TechnicianId = userId.Value,
                    WeeklySchedule = GenerateDefaultSchedule()
                }
            };

            return View(model);
        }

        // 🔥 Assigned Tasks View
        public IActionResult AssignedTasks()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "technician" || userId == null)
                return RedirectToAction("Login", "Account");

            var assignedTasks = _context.RepairRequests
                .Where(r => r.TechnicianId == userId && r.Status == "Đã phân công")
                .OrderBy(r => r.PreferredVisitAt)
                .ToList();

            return View(assignedTasks);
        }

        // 🔥 Work History View
        public IActionResult WorkHistory()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "technician" || userId == null)
                return RedirectToAction("Login", "Account");

            var workHistory = _context.RepairRequests
                .Where(r => r.TechnicianId == userId && r.Status != "Đã phân công")
                .OrderByDescending(r => r.PreferredVisitAt)
                .ToList();

            return View(workHistory);
        }

        // 🔥 Technician Profile View
        public IActionResult Profile()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "technician" || userId == null)
                return RedirectToAction("Login", "Account");

            var technician = _context.Users.Find(userId);
            if (technician == null)
                return NotFound();

            var averageRating = (decimal?)_context.RepairRequests
                .Where(r => r.TechnicianId == userId && r.TechnicianRating.HasValue)
                .Average(r => (double?)r.TechnicianRating) ?? 0m;

            technician.AverageRating = averageRating;
            technician.TotalJobsCompleted = _context.RepairRequests
                .Count(r => r.TechnicianId == userId && 
                       (r.Status == "Hoàn thành" || r.Status == "Đã thanh toán"));

            return View(technician);
        }

        // 🔥 Update Technician Profile
        [HttpPost]
        public IActionResult UpdateProfile(User model)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "technician" || userId == null)
                return RedirectToAction("Login", "Account");

            var technician = _context.Users.Find(userId);
            if (technician == null)
                return NotFound();

            technician.Name = model.Name;
            technician.Phone = model.Phone;
            technician.Address = model.Address;
            technician.Specializations = model.Specializations;
            technician.WorkStartHour = model.WorkStartHour;
            technician.WorkEndHour = model.WorkEndHour;

            _context.SaveChanges();

            return RedirectToAction("Profile");
        }

        // Helper method to generate default schedule
        private List<ScheduleEntry> GenerateDefaultSchedule()
        {
            var schedule = new List<ScheduleEntry>();
            for (int i = 0; i < 7; i++)
            {
                schedule.Add(new ScheduleEntry
                {
                    DayOfWeek = (DayOfWeek)i,
                    StartTime = new TimeSpan(8, 0, 0),
                    EndTime = new TimeSpan(19, 0, 0),
                    IsWorkDay = i < 6 // Monday to Saturday are work days
                });
            }
            return schedule;
        }

        // 🔥 Assign (Admin)
        public IActionResult Assign(int id)
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "admin")
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);

            if (request == null) return NotFound();

            ViewBag.Technicians = _context.Users
                .Where(u => u.Role != null && u.Role.ToLower() == "technician")
                .ToList();

            return View(request);
        }

        [HttpPost]
        public IActionResult Assign(int id, int technicianId)
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "admin")
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);

            if (request == null) return NotFound();

            request.TechnicianId = technicianId;
            request.Status = "Đã phân công";

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // 🔥 Update status
        public IActionResult UpdateStatus(int id, string status)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "technician" || userId == null)
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);

            if (request == null) return NotFound();

            // Technician can only update jobs assigned to them.
            if (request.TechnicianId != userId)
                return Forbid();

            // Allow only valid workflow transitions.
            if (status == "Đang sửa" && request.Status != "Đã phân công")
                return BadRequest("Công việc chưa ở trạng thái sẵn sàng nhận.");

            if (status != "Đang sửa")
                return BadRequest("Trạng thái không hợp lệ.");

            request.Status = status;

            _context.SaveChanges();

            return RedirectToAction("MyJobs");
        }

        public IActionResult Complete(int id)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "technician" || userId == null)
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);
            if (request == null) return NotFound();
            if (request.TechnicianId != userId) return Forbid();
            if (request.Status != "Đang sửa") return BadRequest("Công việc chưa ở trạng thái đang sửa.");

            ViewBag.ComponentPrices = ComponentPrices;
            return View(request);
        }

        [HttpPost]
        public IActionResult Complete(int id, List<string>? replacedParts)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "technician" || userId == null)
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);
            if (request == null) return NotFound();
            if (request.TechnicianId != userId) return Forbid();
            if (request.Status != "Đang sửa") return BadRequest("Công việc chưa ở trạng thái đang sửa.");

            replacedParts ??= new List<string>();
            if (!replacedParts.Any())
                return BadRequest("Bạn cần chọn ít nhất 1 linh kiện đã thay.");

            decimal total = 200000m;
            foreach (var part in replacedParts)
            {
                if (ComponentPrices.TryGetValue(part, out var partPrice))
                    total += partPrice;
            }

            request.ReplacedParts = string.Join(", ", replacedParts.Distinct());
            request.FinalAmount = total;
            request.PartsConfirmedByCustomer = false;
            request.Status = "Chờ khách xác nhận linh kiện";

            _context.SaveChanges();
            return RedirectToAction("MyJobs");
        }

        // 🔥 Payment
        public IActionResult History()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "customer" || userId == null)
                return RedirectToAction("Login", "Account");

            var history = _context.RepairRequests
                .Where(r => r.CustomerId == userId)
                .OrderByDescending(r => r.Id)
                .ToList();

            return View(history);
        }

        public IActionResult Payment(int id)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "customer" || userId == null)
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);

            if (request == null) return NotFound();
            if (request.CustomerId != userId) return Forbid();

            return View(request);
        }

        [HttpPost]
        public IActionResult Payment(int id, string method)
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "customer")
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);

            if (request == null) return NotFound();

            if (request.Status != "Chờ xử lý" && request.Status != "Hoàn thành" && request.Status != "Đã hoàn thành")
                return BadRequest("Không thể thanh toán");

            request.Status = "Đã thanh toán";

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}