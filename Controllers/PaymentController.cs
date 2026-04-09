using Microsoft.AspNetCore.Mvc;
using dienlanh.Data;
using System.Linq;

namespace dienlanh.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Pay(int id)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "customer" || userId == null)
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.FirstOrDefault(r => r.Id == id);

            if (request == null)
                return NotFound();

            // Customers can only pay their own requests.
            if (request.CustomerId != userId)
                return Forbid();

            if (request.Status != "Chờ thanh toán" || !request.PartsConfirmedByCustomer)
                return BadRequest("Bạn cần xác nhận linh kiện trước khi thanh toán.");
            if (request.CustomerReported && !request.ReportResolved)
                return BadRequest("Yêu cầu đang được hệ thống xử lý báo lỗi.");

            return View(request);
        }

        [HttpPost]
        public IActionResult Confirm(int id)
        {
            var role = HttpContext.Session.GetString("role");
            var userId = HttpContext.Session.GetInt32("userId");

            if (role != "customer" || userId == null)
                return RedirectToAction("Login", "Account");

            var request = _context.RepairRequests.Find(id);

            if (request == null)
                return NotFound();

            if (request.CustomerId != userId)
                return Forbid();

            if (request.Status != "Chờ thanh toán" || !request.PartsConfirmedByCustomer)
                return BadRequest("Bạn cần xác nhận linh kiện trước khi thanh toán.");
            if (request.CustomerReported && !request.ReportResolved)
                return BadRequest("Yêu cầu đang được hệ thống xử lý báo lỗi.");

            request.Status = "Đã thanh toán";

            _context.SaveChanges();

            return RedirectToAction("MyRequests", "Customer");
        }
    }
}