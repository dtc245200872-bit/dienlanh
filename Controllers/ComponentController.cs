using dienlanh.Data;
using dienlanh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace dienlanh.Controllers
{
    public class ComponentController : Controller
    {
        private readonly AppDbContext _context;

        public ComponentController(AppDbContext context)
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

            var components = _context.Components
                .Include(c => c.Device)
                .OrderBy(c => c.Name)
                .ToList();
            return View(components);
        }

        public IActionResult Create()
        {
            if (!HasManagementRole())
                return RedirectToAction("Login", "Account");

            ViewBag.Devices = _context.Devices.OrderBy(d => d.Name).ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Component component)
        {
            if (!HasManagementRole())
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(component.Name))
            {
                ViewBag.Devices = _context.Devices.OrderBy(d => d.Name).ToList();
                return View(component);
            }

            component.Name = component.Name.Trim();
            _context.Components.Add(component);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!HasManagementRole())
                return RedirectToAction("Login", "Account");

            var component = _context.Components.Find(id);
            if (component != null)
            {
                _context.Components.Remove(component);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
