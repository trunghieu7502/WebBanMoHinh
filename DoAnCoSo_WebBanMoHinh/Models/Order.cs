using Microsoft.AspNetCore.Identity;

namespace DoAnCoSo_WebBanMoHinh.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string DeliveryMethod { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
        public bool IsDone {  get; set; }
        public ApplicationUser User { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
