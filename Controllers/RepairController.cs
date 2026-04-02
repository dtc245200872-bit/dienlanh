using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            List<RepairRequest> list = _context.RepairRequests.ToList();
            return View(list);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(RepairRequest request)
        {
            _context.RepairRequests.Add(request);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}