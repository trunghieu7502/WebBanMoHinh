using System.ComponentModel.DataAnnotations;

namespace DoAnCoSo_WebBanMoHinh.Models
{
    public class RedeemedCoupon
    {
        [Key]
        public int Id { get; set; }
        public int CouponId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Code { get; set; } 
        public decimal DiscountAmount { get; set; } 
        public DateTime RedeemedDate { get; set; }
        public ApplicationUser User { get; set; }
    }
}
