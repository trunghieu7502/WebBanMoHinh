using Microsoft.AspNetCore.Identity;

namespace DoAnCoSo_WebBanMoHinh.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; }
        public string Notes { get; set; }
        public ApplicationUser User { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
