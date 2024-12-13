using DoAnCoSo_WebBanMoHinh.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo_WebBanMoHinh.Repositories
{
    public class EFCouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCouponRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Coupons>> GetAllAsync()
        {
            return await _context.Coupons.ToListAsync();
        }

        public async Task<Coupons> GetByIdAsync(int id)
        {
            return await _context.Coupons.SingleOrDefaultAsync(c => c.CouponId == id);
        }

        // Thêm coupon mới
        public async Task AddAsync(Coupons coupon)
        {
            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Coupons coupon)
        {
            _context.Coupons.Update(coupon);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
            }
        }
    }
}
