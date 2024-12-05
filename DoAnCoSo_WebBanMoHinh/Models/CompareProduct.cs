namespace DoAnCoSo_WebBanMoHinh.Models
{
    public class CompareProduct
    {
        public int Id { get; set; }
        public ApplicationUser User { get; set; }
        public string UserID { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
