using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAnCoSo_WebBanMoHinh.Models;
using DoAnCoSo_WebBanMoHinh.Repositories;
using System.Security.Claims;

namespace DoAnCoSo_WebBanMoHinh.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICompanyRepository _companyRepository;

        public ProductsController(ApplicationDbContext context, IProductRepository productRepository, ICategoryRepository categoryRepository, ICompanyRepository companyRepository)
        {
            _context = context;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _companyRepository = companyRepository;
        }

        // GET: Products
        public async Task<ActionResult> Index()
        {
            var allProducts = await _context.Products.ToListAsync();
            var randomProducts = allProducts.OrderBy(p => Guid.NewGuid()).Take(8).ToList();
            var latestProducts = allProducts.OrderByDescending(p => p.Id).Take(4).ToList();
            var products = await _context.Products.Include(p => p.Category).Include(p => p.Company).ToListAsync();
            ViewBag.RandomProducts = randomProducts;
            ViewBag.LatestProducts = latestProducts;
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Companies = await _context.Companies.ToListAsync();
            return View(products);
        }

        // GET: Products/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            var categories = await _categoryRepository.GetByIdAsync(id);
            var companies = await _companyRepository.GetAllAsync();
            if (product == null)
                return NotFound();
            if (product.Images != null)
            {
                ViewBag.Images = product.Images;
            }
            var relatedProducts = await _context.Products
                .Where(p => (p.CompanyId == product.CompanyId || p.CategoryId == product.CategoryId) && p.Id != product.Id)
                .ToListAsync();
            var suggestedProducts = relatedProducts
                .OrderBy(p => Guid.NewGuid()) // Random
                .Take(4)
                .ToList();
            ViewBag.SuggestedProducts = suggestedProducts;
            ViewBag.FavoriteProducts = product.FavoriteProducts;
            ViewBag.CompareProducts = product.CompareProducts;
            return View(product);
        }

        public async Task<IActionResult> SearchProductsAsync(string query)
        {
            var products = await _productRepository.GetAllAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    query = "null";
                var result = products.Where(p => p.Name.Contains(query) ||
                                            (p.Description != null && p.Description.Contains(query))).ToList();
                return View("SearchResult", result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> ProductList(int pageNumber = 1)
        {
            int pageSize = 8;
            IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category).Include(x => x.Company);
            var paginatedProducts = await PaginatedList<Product>.CreateAsync((IQueryable<Product>)productsQuery, pageNumber, pageSize);
            return View(paginatedProducts);
        }

        public async Task<IActionResult> CategoryList(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }
            var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            var products = await _context.Products.Include(p => p.Category).Include(x => x.Company).ToListAsync();
            return View(category);
        }

        public async Task<IActionResult> CompanyList(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }
            var company = await _context.Companies.FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }
            var products = await _context.Products.Include(p => p.Category).Include(x => x.Company).ToListAsync();
            return View(company);
        }

        public async Task<IActionResult> FavoriteList()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var favoriteProducts = await _context.FavoriteProducts
                .Include(fp => fp.Product)
                .ThenInclude(p => p.Category)
                .Include(fp => fp.Product.Company)
                .Where(fp => fp.UserID == userId)
                .Select(fp => fp.Product)
                .ToListAsync();
            return View(favoriteProducts);
        }

        public async Task<IActionResult> AddToFavorites(int id, string returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _productRepository.GetByIdAsync(id);
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? Request.Headers["Referer"].ToString() });
            }
            var existingFavorite = await _context.FavoriteProducts.FirstOrDefaultAsync(u => u.ProductId == product.Id && u.User.Id == userId);
            if (existingFavorite == null)
            {
                var favoriteProduct = new FavoriteProduct
                {
                    ProductId = product.Id,
                    User = await _context.Users.FindAsync(userId),
                    UserID = userId
                };
                _context.FavoriteProducts.Add(favoriteProduct);
                await _context.SaveChangesAsync();
            }
            return Redirect(returnUrl ?? Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> RemoveFromFavorites(int id, string returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _productRepository.GetByIdAsync(id);
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? Request.Headers["Referer"].ToString() });
            }
            var existingFavorite = await _context.FavoriteProducts.FirstOrDefaultAsync(u => u.ProductId == product.Id && u.User.Id == userId);
            if (existingFavorite != null)
            {
                _context.FavoriteProducts.Remove(existingFavorite);
                await _context.SaveChangesAsync();
            }
            return Redirect(returnUrl ?? Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Comparation()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var compareProducts = await _context.CompareProducts
                .Include(fp => fp.Product)
                .ThenInclude(p => p.Category)
                .Include(fp => fp.Product.Company)
                .Where(fp => fp.UserID == userId)
                .Select(fp => fp.Product)
                .ToListAsync();

            return View(compareProducts);
        }

        public async Task<IActionResult> AddToCompare(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _productRepository.GetByIdAsync(id);
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            if (product == null)
            {
                return NotFound();
            }
            var existingComparation = await _context.CompareProducts.FirstOrDefaultAsync(u => u.ProductId == product.Id && u.User.Id == userId);
            if (existingComparation == null)
            {
                var compareProduct = new CompareProduct
                {
                    ProductId = product.Id,
                    User = await _context.Users.FindAsync(userId),
                    UserID = userId
                };
                _context.CompareProducts.Add(compareProduct);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Comparation");
        }

        public async Task<IActionResult> RemoveFromCompares(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _productRepository.GetByIdAsync(id);
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            var existingComparation = await _context.CompareProducts.FirstOrDefaultAsync(u => u.ProductId == product.Id && u.User.Id == userId);
            if (existingComparation != null)
            {
                _context.CompareProducts.Remove(existingComparation);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Comparation", "Products");
        }

        public async Task<IActionResult> GetAvailableProducts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var comparedProductIds = await _context.CompareProducts
                .Where(cp => cp.UserID == userId)
                .Select(cp => cp.ProductId)
                .ToListAsync();
            var products = await _context.Products
                .Include(p => p.Company)
                .Include(p => p.Category)
                .Where(p => !comparedProductIds.Contains(p.Id))
                .ToListAsync();
            var htmlString = "";
            foreach (var product in products)
            {
                htmlString += $"<tr>" +
                              $"<td><img src='{product.ImageUrl}' class='img-thumbnail' alt='{product.Name}' style='height: 50px;'></td>" +
                              $"<td>{product.Name}</td>" +
                              $"<td>{product.Price.ToString("#,##0")} VNĐ</td>" +
                              $"<td>{product.Company?.CompName}</td>" +
                              $"<td>{product.Category?.CateName}</td>" +
                              $"<td><button class='btn btn-success btn-sm' onclick='addToComparison({product.Id})'>Thêm</button></td>" +
                              $"</tr>";
            }
            return Content(htmlString);
        }

        public async Task<IActionResult> GetCategoriesAndCompanies()
        {
            var categories = await _context.Categories.ToListAsync();
            var companies = await _context.Companies.ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.Companies = companies;

            return PartialView("_DropdownPartial");
        }
    }
}