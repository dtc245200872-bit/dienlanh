using dienlanh.Data;
using dienlanh.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace dienlanh.Controllers
{
    public class DeviceController : Controller
    {
        private readonly AppDbContext _context;

        public DeviceController(AppDbContext context)
        {
            _context = context;
        }

        private bool HasManagementRole()
        {
            var role = HttpContext.Session.GetString("role");
            return role == "admin" || role == "technician";
        }

        public IActionResult Index()
        {
            if (!HasManagementRole())
                return RedirectToAction("Login", "Account");

            var devices = _context.Devices.OrderBy(d => d.Name).ToList();
            return View(devices);
        }

        public IActionResult Create()
        {
            if (!HasManagementRole())
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public IActionResult Create(Device device)
        {
            if (!HasManagementRole())
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(device.Name))
                return View(device);

            var exists = _context.Devices.Any(d => d.Name.ToLower() == device.Name.Trim().ToLower());
            if (!exists)
            {
                device.Name = device.Name.Trim();
                _context.Devices.Add(device);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!HasManagementRole())
                return RedirectToAction("Login", "Account");

            var device = _context.Devices.Find(id);
            if (device != null)
            {
                _context.Devices.Remove(device);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
