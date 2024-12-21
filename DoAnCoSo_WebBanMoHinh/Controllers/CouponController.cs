using DoAnCoSo_WebBanMoHinh.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo_WebBanMoHinh.Controllers
{
    [Authorize]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> ShopCoupons()
        {
            var coupons = _context.Coupons
                .Where(c => c.RedeemedBy == null && c.ExpirationDate > DateTime.Now)
                .Select(c => new
                {
                    c.CouponId,
                    c.DiscountAmount,
                    c.PointsRequired,
                    c.ExpirationDate
                })
                .ToList();

            if (!coupons.Any())
            {
                ViewData["NoCoupons"] = "Hiện tại không có coupon nào trong shop. Vui lòng quay lại sau!";
            }

            var userName = User.Identity.Name;
            var redeemedCoupons = _context.RedeemedCoupons
                .Where(rc => rc.UserName == userName)
                .Select(rc => new
                {
                    rc.CouponId,
                    rc.Code,
                    rc.DiscountAmount,
                    rc.RedeemedDate,
                    ExpirationDate = _context.Coupons
                        .Where(c => c.CouponId == rc.CouponId)
                        .Select(c => c.ExpirationDate)
                        .FirstOrDefault(),
                    PointsUsed = _context.Coupons
                        .Where(c => c.CouponId == rc.CouponId)
                        .Select(c => c.PointsRequired)
                        .FirstOrDefault()
                })
                .ToList();

            ViewData["RedeemedCoupons"] = redeemedCoupons;
            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            ViewData["UserPoints"] = user?.Points ?? 0;
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Companies = await _context.Companies.ToListAsync();
            return View(coupons);
        }

        [HttpPost]
        public IActionResult RedeemCoupon(int couponId)
        {
            var coupon = _context.Coupons.FirstOrDefault(c => c.CouponId == couponId);
            if (coupon == null || coupon.ExpirationDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "Coupon không khả dụng hoặc đã hết hạn.";
                return RedirectToAction("ShopCoupons");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user == null || user.Points < coupon.PointsRequired)
            {
                TempData["ErrorMessage"] = "Không đủ điểm để đổi coupon.";
                return RedirectToAction("ShopCoupons");
            }
            user.Points -= coupon.PointsRequired;
            coupon.RedeemedBy = user.Id;
            var redeemedCoupon = new RedeemedCoupon
            {
                CouponId = couponId,
                UserId = user.Id,
                UserName = user.UserName,
                Code = coupon.Code,
                DiscountAmount = coupon.DiscountAmount,
                RedeemedDate = DateTime.Now
            };

            try
            {
                _context.Coupons.Update(coupon);
                _context.RedeemedCoupons.Add(redeemedCoupon);
                _context.Users.Update(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Đổi coupon thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi đổi coupon: {ex.Message}";
            }

            return RedirectToAction("ShopCoupons");
        }
    }
}
