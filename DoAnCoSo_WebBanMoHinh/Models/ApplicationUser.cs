using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DoAnCoSo_WebBanMoHinh.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string? Address { get; set; }
        public int Points { get; set; } = 0;
        public ICollection<Coupons> RedeemedCoupons { get; set; }
    }
}
