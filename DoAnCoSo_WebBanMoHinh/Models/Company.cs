using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DoAnCoSo_WebBanMoHinh.Models
{
    public class Company
    {
        [Required]
        public int Id { get; set; }
        [Required(ErrorMessage = "Tên Hãng không được để trống"), StringLength(50)]
        [DisplayName("Tên Hãng")]
        public string CompName { get; set; }
        public List<Product>? Products { get; set; }
    }
}