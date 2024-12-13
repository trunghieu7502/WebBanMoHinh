using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAnCoSo_WebBanMoHinh.Models
{
    public class Coupons
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CouponId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Required]
        public int PointsRequired { get; set; }

        public DateTime ExpirationDate { get; set; }

        public string? RedeemedBy { get; set; } 

        public ApplicationUser? User { get; set; } 

        public int? OrderId { get; set; } 

        public Order? Order { get; set; }
    }
}