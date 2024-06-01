using DoAnCoSo_WebBanMoHinh.Models;

public class CheckoutVM
{
    public ApplicationUser User { get; set; }
    public ShoppingCart Cart { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string District { get; set; }
    public string Ward { get; set; }
    public string PaymentMethod { get; set; }
    public string Notes { get; set; }
}