using DoAnCoSo_WebBanMoHinh.Models;

namespace DoAnCoSo_WebBanMoHinh.Areas.Admin.Models
{
    public class UserRole
    {
        public ApplicationUser User { get; set; }
        public string Role { get; set; }
    }
}
