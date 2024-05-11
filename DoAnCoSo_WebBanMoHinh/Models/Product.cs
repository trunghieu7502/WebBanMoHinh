using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DoAnCoSo_WebBanMoHinh.Models
{
    public class Product
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên Sản Phẩm không được để trống"), StringLength(200, ErrorMessage = "Tên Sản Phẩm không được quá 200 ký tự")]
        [DisplayName("Tên Sản Phẩm")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(1000.00, 10000000.00, ErrorMessage = "Giá phải từ {1} đến {2} VNĐ")]
        [DisplayName("Giá")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Mô Tả không được để trống")]
        [DisplayName("Mô Tả")]
        public string Description { get; set; }

        [DisplayName("Tình trạng tồn sản phẩm")]
        public bool Status { get; set; }

        [DisplayName("Ảnh đại diện")]
        public string? ImageUrl { get; set; }

        [DisplayName("Các Ảnh Khác")]
        public List<ProductImage>? Images { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
