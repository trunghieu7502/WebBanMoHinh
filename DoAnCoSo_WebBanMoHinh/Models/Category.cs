using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DoAnCoSo_WebBanMoHinh.Models
{
    public class Category
    {
        [Required]
        public int Id { get; set; }
        [Required(ErrorMessage = "Tên Loại Hàng không được để trống"), StringLength(50)]
        [DisplayName("Tên Loại Hàng")]
        public string CateName { get; set; }
        public List<Product>? Products { get; set; }
    }
}