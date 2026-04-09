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
            ViewBag.Devices = new List<string>
            {
                "Máy lạnh", "Tủ lạnh", "Máy giặt"
            };

            return View();
        }

        [HttpPost]
        public IActionResult Create(RepairRequest request, IFormFile imageFile)
        {
            var role = HttpContext.Session.GetString("role");

            if (string.IsNullOrEmpty(role) || role != "customer")
                return RedirectToAction("Login", "Account");

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