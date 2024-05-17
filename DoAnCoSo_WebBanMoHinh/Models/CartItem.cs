namespace DoAnCoSo_WebBanMoHinh.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
