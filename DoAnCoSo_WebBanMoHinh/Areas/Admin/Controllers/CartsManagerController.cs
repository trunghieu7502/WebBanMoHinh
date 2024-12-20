using DoAnCoSo_WebBanMoHinh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo_WebBanMoHinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebRoles.Role_Admin)]
    public class CartsManagerController : Controller
    {
        // GET: CartsManagerController
        private readonly ApplicationDbContext _context;

        public CartsManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/CartsManagerController
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
             .Include(o => o.Coupons)
             .ToListAsync();

            return View(orders);

        }

        // GET: CartsManagerController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            var detail = await _context.OrderDetails.Include(p => p.Product).Include(x => x.Order).ToListAsync();
            var products = await _context.Products.Include(p => p.Category).Include(x => x.Company).ToListAsync();
            return View(order);
        }

        private bool OrderExists(int id)
        {
            return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> MonthlyRevenue()
        {
            var now = DateTime.Now;
            var currentMonthRevenue = await _context.Orders
                .Where(o => o.OrderDate.Month == now.Month && o.OrderDate.Year == now.Year)
                .SumAsync(o => o.TotalPrice);
            var lastMonth = now.AddMonths(-1);
            var lastMonthRevenue = await _context.Orders
                .Where(o => o.OrderDate.Month == lastMonth.Month && o.OrderDate.Year == lastMonth.Year)
                .SumAsync(o => o.TotalPrice);
            decimal percentageChange = 0;
            if (lastMonthRevenue > 0)
            {
                percentageChange = ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100;
            }
            else if (currentMonthRevenue > 0)
            {
                percentageChange = 100;
            }
            var result = new
            {
                CurrentMonthRevenue = currentMonthRevenue,
                LastMonthRevenue = lastMonthRevenue,
                PercentageChange = percentageChange
            };
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateIsDone([FromBody] UpdateIsDoneModel model)
        {
            var order = await _context.Orders.FindAsync(model.Id);
            if (order == null)
            {
                return Json(new { success = false });
            }

            order.IsDone = model.IsDone;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
    public class UpdateIsDoneModel
    {
        public int Id { get; set; }
        public bool IsDone { get; set; }
    }
}
