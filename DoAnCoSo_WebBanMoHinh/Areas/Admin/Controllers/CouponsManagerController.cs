using DoAnCoSo_WebBanMoHinh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo_WebBanMoHinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebRoles.Role_Admin)]
    public class CouponsManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponsManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var coupons = await _context.Coupons
                .Select(c => new
                {
                    c.CouponId,
                    c.Code,
                    c.DiscountAmount,
                    c.PointsRequired,
                    c.ExpirationDate,
                    RedeemedBy = c.User != null ? c.User.FullName : "Chưa Đổi",
                    RedeemedById = c.RedeemedBy ?? "N/A",
                    OrderId = c.OrderId.HasValue ? $"#{c.OrderId}" : "Chưa Áp Dụng"
                })
                .ToListAsync();

            return View(coupons);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupons coupon)
        {
            if (ModelState.IsValid)
            {
                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Coupon đã được tạo thành công!";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Vui lòng kiểm tra lại thông tin!";
            return View(coupon);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.CouponId == id);
            if (coupon == null)
            {
                TempData["ErrorMessage"] = "Coupon không tồn tại!";
                return RedirectToAction("Index");
            }

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Coupon đã được xóa!";
            return RedirectToAction("Index");
        }
    }
}