using DoAnCoSo_WebBanMoHinh.Extentions;
using DoAnCoSo_WebBanMoHinh.Models;
using DoAnCoSo_WebBanMoHinh.Models.Services;
using DoAnCoSo_WebBanMoHinh.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
                Quantity = quantity,
                Stock = product.Stock,
            };
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Companies = await _context.Companies.ToListAsync();
            return RedirectToAction("Index");
        }

        // GET: ShoppingCartController
        public async Task<ActionResult> Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Companies = await _context.Companies.ToListAsync();
            if (!cart.Items.Any())
            {
                ViewData["DiscountAmount"] = 0;
                ViewData["CouponCode"] = null;
            }
            else
            {
                var discountAmount = HttpContext.Session.GetObjectFromJson<decimal?>("DiscountAmount") ?? 0;
                var couponCode = HttpContext.Session.GetObjectFromJson<string>("CouponCode");
                ViewData["DiscountAmount"] = Convert.ToDecimal(discountAmount);
                ViewData["CouponCode"] = couponCode;
            }

            return View(cart);
        }

        public IActionResult RemoveFromCart(int Id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is not null)
            {
                cart.RemoveItem(Id);
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
                if(quantity == 0)
                    findCartItem.Quantity = 1;
                else
                    findCartItem.Quantity = quantity;
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Companies = await _context.Companies.ToListAsync();
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (cart.Items.Count == 0)
            {
                return RedirectToAction("Index", "Products");
            }
            var discountAmount = HttpContext.Session.GetObjectFromJson<decimal?>("DiscountAmount") ?? 0;
            var couponCode = HttpContext.Session.GetObjectFromJson<string>("CouponCode");
            var model = new CheckoutVM
            {
                User = user,
                Cart = cart,
                DiscountAmount = discountAmount,
                CouponCode = couponCode
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order, CheckoutVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }
            foreach (var item in cart.Items)
            {
                if (item.Quantity <= 0)
                {
                    TempData["ErrorMessage"] = "Số lượng sản phẩm không hợp lệ. Vui lòng kiểm tra lại giỏ hàng.";
                    return RedirectToAction("Index");
                }
            }
            var discountAmount = HttpContext.Session.GetObjectFromJson<decimal?>("DiscountAmount") ?? 0;
            var couponCode = HttpContext.Session.GetObjectFromJson<string>("CouponCode");
            var totalPriceAfterDiscount = cart.Items.Sum(i => i.Price * i.Quantity) - discountAmount;
            if (totalPriceAfterDiscount < 0)
            {
                TempData["ErrorMessage"] = "Tổng tiền không hợp lệ sau khi áp dụng mã giảm giá.";
                return RedirectToAction("Index");
            }
            order.UserId = user.Id;
            order.UserName = user.UserName;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = totalPriceAfterDiscount;
            order.DiscountAmount = discountAmount;
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
            if (!string.IsNullOrEmpty(couponCode))
            {
                var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode);
                if (coupon != null)
                {
                    coupon.RedeemedBy = user.Id;
                    _context.Coupons.Update(coupon);
                    await _context.SaveChangesAsync();
                }
            }
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            foreach (var item in cart.Items)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.Id);
                if (product == null)
                {
                    continue;
                }
                if (product.Stock < item.Quantity)
                {
                    ModelState.AddModelError("", $"Không đủ số lượng mua sản phẩm!");
                    return View(model);
                }
                product.Stock -= item.Quantity;
                if (product.Stock == 0)
                {
                    product.Status = false;
                }
                _context.Update(product);
            }
            await _context.SaveChangesAsync();
            int rewardPoints = 0;
            if (totalPriceAfterDiscount >= 1000000 && totalPriceAfterDiscount < 3000000)
            {
                rewardPoints = 10;
            }
            else if (totalPriceAfterDiscount >= 3000000 && totalPriceAfterDiscount < 5000000)
            {
                rewardPoints = 40;
            }
            else if (totalPriceAfterDiscount >= 5000000 && totalPriceAfterDiscount < 10000000)
            {
                rewardPoints = 65;
            }
            else if (totalPriceAfterDiscount >= 10000000 && totalPriceAfterDiscount < 20000000)
            {
                rewardPoints = 140;
            }
            else if (totalPriceAfterDiscount >= 20000000 && totalPriceAfterDiscount < 50000000)
            {
                rewardPoints = 300;
            }
            else if (totalPriceAfterDiscount >= 50000000 && totalPriceAfterDiscount < 100000000)
            {
                rewardPoints = 900;
            }
            else if (totalPriceAfterDiscount >= 100000000)
            {
                rewardPoints = 2500 + (int)((totalPriceAfterDiscount - 100000000) / 1000000) * 25;
            }
            user.Points += rewardPoints;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            if (model.PaymentMethod == "VNPAY")
            {
                var vnPayModel = new VNPayRequestModel
                {
                    Amount = (double)totalPriceAfterDiscount,
                    CreatedDate = DateTime.Now,
                    Description = "Đơn hàng thành công",
                    FullName = user.UserName,
                    OrderId = new Random().Next(100, 1000)
                };

                HttpContext.Session.Remove("Cart");
                HttpContext.Session.Remove("DiscountAmount");
                HttpContext.Session.Remove("CouponCode");
                return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
            }
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("DiscountAmount");
            HttpContext.Session.Remove("CouponCode");
            TempData["SuccessMessage"] = $"Đơn hàng hoàn tất! Bạn đã được cộng {rewardPoints} điểm.";
            return View("OrderCompleted", order);
        }

        public async Task<IActionResult> PaymentSuccess()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Companies = await _context.Companies.ToListAsync();
            return View("OrderCompleted");
        }

        [Authorize]
        public async Task<IActionResult> PaymentFail()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Companies = await _context.Companies.ToListAsync();
            return View();
        }

        [Authorize]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }
            
            TempData["Message"] = $"Thanh toán VNPay thành công";

            //Biện pháp tạm thời, không thể áp dụng cho trường hợp 2 nguòi thanh toán cùng một lúc
            Order order = _context.Orders.OrderBy(i => i.Id).LastOrDefault();

            return View("OrderCompleted", order);
        }

        [HttpPost]
        public IActionResult ApplyCoupon(string CouponCode)
        {
            if (string.IsNullOrEmpty(CouponCode))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập mã giảm giá!";
                return RedirectToAction("Index");
            }
            var coupon = _context.Coupons.FirstOrDefault(c => c.Code == CouponCode && c.OrderId == null);
            if (coupon == null)
            {
                TempData["ErrorMessage"] = "Mã giảm giá không hợp lệ hoặc đã được sử dụng!";
                return RedirectToAction("Index");
            }
            if (coupon.ExpirationDate < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Mã giảm giá đã hết hạn!";
                return RedirectToAction("Index");
            }
            if (!string.IsNullOrEmpty(HttpContext.Session.GetObjectFromJson<string>("CouponCode")))
            {
                TempData["ErrorMessage"] = "Bạn đã áp dụng một mã giảm giá. Vui lòng hủy mã hiện tại trước khi áp dụng mã mới.";
                return RedirectToAction("Index");
            }
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var orderTotal = cart.Items.Sum(i => i.Quantity * i.Price);
            if (orderTotal < 500000)
            {
                TempData["ErrorMessage"] = "Đơn hàng của bạn phải trên 500.000 VNĐ để áp dụng mã giảm giá!";
                return RedirectToAction("Index");
            }
            HttpContext.Session.SetObjectAsJson("DiscountAmount", coupon.DiscountAmount);
            HttpContext.Session.SetObjectAsJson("CouponCode", coupon.Code);
            TempData["DiscountMessage"] = $"Mã giảm giá áp dụng thành công! Bạn được giảm {coupon.DiscountAmount:#,##0} VNĐ.";
            HttpContext.Session.SetObjectAsJson("DiscountAmount", coupon.DiscountAmount);
            HttpContext.Session.SetObjectAsJson("CouponCode", CouponCode);
            TempData["DiscountMessage"] = $"Đã áp dụng mã giảm giá thành công! Giảm {coupon.DiscountAmount:#,##0} VNĐ.";
            return RedirectToAction("Index");
        }

        public IActionResult CancelCheckout()
        {
            HttpContext.Session.Remove("DiscountAmount");
            HttpContext.Session.Remove("CouponCode");
            TempData["ErrorMessage"] = "Bạn đã hủy giao dịch. Mã giảm giá đã được khôi phục.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CancelCoupon()
        {
            HttpContext.Session.Remove("DiscountAmount");
            HttpContext.Session.Remove("CouponCode");
            TempData["SuccessMessage"] = "Mã giảm giá đã được hủy thành công!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            int cartCount = cart.Items.Sum(i => i.Quantity);
            return Json(new { count = cartCount });
        }
    }
}