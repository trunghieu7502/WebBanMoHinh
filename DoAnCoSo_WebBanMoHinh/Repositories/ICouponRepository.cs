using DoAnCoSo_WebBanMoHinh.Models;

namespace DoAnCoSo_WebBanMoHinh.Repositories
{
    public interface ICouponRepository
    {
        Task<IEnumerable<Coupons>> GetAllAsync();
        Task<Coupons> GetByIdAsync(int id);
        Task AddAsync(Coupons coupon); 
        Task UpdateAsync(Coupons coupon);
        Task DeleteAsync(int id);

    }
}
