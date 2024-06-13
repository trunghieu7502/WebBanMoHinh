using DoAnCoSo_WebBanMoHinh.Extentions;
using DoAnCoSo_WebBanMoHinh.Models;
using DoAnCoSo_WebBanMoHinh.Models.Services;
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
        private readonly IVNPayService _vnPayService;

        public ShoppingCartController(ApplicationDbContext context, IProductRepository productRepository, ICategoryRepository categoryRepository, ICompanyRepository companyRepository, UserManager<ApplicationUser> userManager, IVNPayService vnPayService)
        {
            _context = context;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _companyRepository = companyRepository;
            _userManager = userManager;
            _vnPayService = vnPayService;
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
        public async Task<IActionResult> Checkout(Order order, CheckoutVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if(model.PaymentMethod == "VNPAY")
            {
                var vnPayModel = new VNPayRequestModel
                {
                    Amount = (double)cart.Items.Sum(p => p.Price * p.Quantity),
                    CreatedDate = DateTime.Now,
                    Description = "Đơn hàng thành công",
                    FullName = user.UserName,
                    OrderId = new Random().Next(100, 1000)
                };
                if (cart == null || !cart.Items.Any())
                {
                    return RedirectToAction("Index");
                }
                order.UserId = user.Id;
                order.UserName = user.UserName;
                order.OrderDate = DateTime.UtcNow;
                order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
                order.IsDone = false;
                order.ShippingAddress = model.Address;
                order.City = model.City;
                order.District = model.District;
                order.Ward = model.Ward;
                order.PaymentMethod = model.PaymentMethod;
                order.Notes = model.Notes;
                order.OrderDetails = cart.Items.Select(i => new OrderDetail
                {
                    ProductId = i.Id,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList();
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                HttpContext.Session.Remove("Cart");
                return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
            }
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }
            order.UserId = user.Id;
            order.UserName = user.UserName;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.IsDone = false;
            order.ShippingAddress = model.Address;
            order.City = model.City;
            order.District = model.District;
            order.Ward = model.Ward;
            order.PaymentMethod = model.PaymentMethod;
            order.Notes = model.Notes;
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.Id,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart"); // Xóa giỏ hàng khỏi session
            return View("OrderCompleted", order); // Trang xác nhận hoàn thành đơn hàng
        }

        public IActionResult PaymentSuccess()
        {
            return View("OrderCompleted");
        }

        [Authorize]
        public IActionResult PaymentFail()
        {
            return View();
        }

        [Authorize]
        public IActionResult PaymentCallBack(Order order)
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }


            // Lưu đơn hàng vô database
            TempData["Message"] = $"Thanh toán VNPay thành công";
            return View("OrderCompleted", order);
        }
    }
}
