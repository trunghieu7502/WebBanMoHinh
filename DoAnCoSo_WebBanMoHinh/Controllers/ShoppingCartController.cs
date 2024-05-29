using DoAnCoSo_WebBanMoHinh.Extentions;
using DoAnCoSo_WebBanMoHinh.Models;
using DoAnCoSo_WebBanMoHinh.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DoAnCoSo_WebBanMoHinh.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(ApplicationDbContext context, IProductRepository productRepository, ICategoryRepository categoryRepository, ICompanyRepository companyRepository, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _companyRepository = companyRepository;
            _userManager = userManager;
        }
        private async Task<Product> GetProductFromDatabase(int Id)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
            var product = await _productRepository.GetByIdAsync(Id);
            return product;
        }

        public async Task<IActionResult> AddToCart(int Id, int quantity)
        {
            var product = await GetProductFromDatabase(Id);
            var cartItem = new CartItem
            {
                Id = product.Id,
                Name = product.Name,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                Quantity = quantity
            };
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        // GET: ShoppingCartController
        public ActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        public IActionResult RemoveFromCart(int Id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is not null)
            {
                cart.RemoveItem(Id);
                // Lưu lại giỏ hàng vào Session sau khi đã xóa mục
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        public ActionResult UpdateCart(int id, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var findCartItem = cart.Items.FirstOrDefault(p => p.Id == id);
            if (findCartItem != null)
            {
                findCartItem.Quantity = quantity;
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Index", "Products");
            }
            var model = new CheckoutVM
            {
                User = user,
                Cart = cart
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutVM model)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }
            var user = await _userManager.GetUserAsync(User);
            var order = new Order
            {
                UserId = user.Id,
                UserName = user.UserName,
                OrderDate = DateTime.UtcNow,
                TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity),
                IsDone = false,
                ShippingAddress = model.Address,
                City = model.City,
                District = model.District,
                Ward = model.Ward,
                DeliveryMethod = model.DeliveryMethod,
                PaymentMethod = model.PaymentMethod,
                OrderDetails = cart.Items.Select(i => new OrderDetail
                {
                    ProductId = i.Id,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart"); // Xóa giỏ hàng khỏi session
            return View("OrderCompleted", order); // Trang xác nhận hoàn thành đơn hàng
        }
    }
}
