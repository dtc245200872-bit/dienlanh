using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dienlanh.Models;
using dienlanh.Data;

namespace dienlanh.Controllers
{
    public class RepairController : Controller
    {
        private readonly AppDbContext _context;

        public RepairController(AppDbContext context)
        {
            _context = context;
        }

        // 🔥 Danh sách (Admin + Technician)
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "Technician" && role != "Admin")
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

        // 🔥 Tạo yêu cầu + upload ảnh
        [HttpPost]
        public IActionResult Create(RepairRequest request, IFormFile imageFile)
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "Customer")
                return RedirectToAction("Login", "Account");

            // 👉 Upload ảnh
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

            return RedirectToAction("Payment", new { id = request.Id });
        }

        // 🔥 Công việc kỹ thuật viên
        public IActionResult MyJobs()
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "Technician")
                return RedirectToAction("Login", "Account");

            var jobs = _context.RepairRequests
                .Where(r => r.TechnicianId == userId)
                .ToList();

            return View(jobs);
        }

        // 🔥 Assign (Admin)
        public IActionResult Assign(int id)
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);

            if (request == null) return NotFound();

            ViewBag.Technicians = _context.Users
                .Where(u => u.Role == "Technician")
                .ToList();

            return View(request);
        }

        [HttpPost]
        public IActionResult Assign(int id, int technicianId)
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "Admin")
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

            if (role != "Technician")
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);

            if (request == null) return NotFound();

            request.Status = status;

            _context.SaveChanges();

            return RedirectToAction("MyJobs");
        }

        // 🔥 Payment
        public IActionResult Payment(int id)
        {
            var request = _context.RepairRequests.Find(id);

            if (request == null) return NotFound();

            return View(request);
        }

        [HttpPost]
        public IActionResult Payment(int id, string method)
        {
            var role = HttpContext.Session.GetString("role");

            if (role != "Customer")
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);

            if (request == null) return NotFound();

            if (request.Status != "Chờ xử lý" && request.Status != "Đã hoàn thành")
                return BadRequest("Không thể thanh toán");

            request.Status = "Đã thanh toán";

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}