namespace DoAnCoSo_WebBanMoHinh.Models.Services
{
    public class CouponService
    {
        private readonly ApplicationDbContext _context;

        public CouponService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Coupons> GetAvailableCoupons()
        {
            return _context.Coupons.Where(c => c.ExpirationDate >= DateTime.Now).ToList();
        }

        public IEnumerable<dynamic> GetRedeemedCoupons(string userName)
        {
            return _context.RedeemedCoupons
                .Where(rc => rc.UserName == userName)
                .Select(rc => new
                {
                    rc.CouponId,
                    rc.Code,
                    rc.DiscountAmount,
                    rc.RedeemedDate
                })
                .ToList();
        }

        public Coupons GetCouponById(int couponId)
        {
            return _context.Coupons.FirstOrDefault(c => c.CouponId == couponId);
        }

        public bool RedeemCoupon(string userName, int couponId)
        {
            var coupon = GetCouponById(couponId);
            if (coupon == null) return false;

            var user = _context.Users.FirstOrDefault(u => u.UserName == userName);
            if (user == null || user.Points < coupon.PointsRequired) return false;

            user.Points -= coupon.PointsRequired;

            _context.RedeemedCoupons.Add(new RedeemedCoupon
            {
                UserName = userName,
                CouponId = couponId,
                Code = coupon.Code,
                DiscountAmount = coupon.DiscountAmount,
                RedeemedDate = DateTime.Now
            });

            _context.SaveChanges();
            return true;
        }
    }

}