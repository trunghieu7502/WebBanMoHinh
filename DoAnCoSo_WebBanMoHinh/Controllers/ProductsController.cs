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
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();
            var companies = await _companyRepository.GetAllAsync();
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
                return RedirectToAction("Login", "Account");
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

        public async Task<IActionResult> AddToFavorites(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _productRepository.GetByIdAsync(id);
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Details", "Products", new { id = product.Id });
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
            return RedirectToAction("Details", "Products", new { id = product.Id });
        }

        public async Task<IActionResult> RemoveFromFavorites(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _productRepository.GetByIdAsync(id);
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Details", "Products", new { id = product.Id });
            }
            var existingFavorite = await _context.FavoriteProducts.FirstOrDefaultAsync(u => u.ProductId == product.Id && u.User.Id == userId);
            if (existingFavorite != null)
            {
                _context.FavoriteProducts.Remove(existingFavorite);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", "Products", new { id = product.Id });
        }

        public async Task<IActionResult> Comparation()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Account");
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
                return RedirectToAction("Login", "Account");
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
            return RedirectToAction("Comparation", "Products");
        }

        public async Task<IActionResult> RemoveFromCompares(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _productRepository.GetByIdAsync(id);
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Login", "Account");
            }
            var existingComparation = await _context.CompareProducts.FirstOrDefaultAsync(u => u.ProductId == product.Id && u.User.Id == userId);
            if (existingComparation != null)
            {
                _context.CompareProducts.Remove(existingComparation);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Comparation", "Products");
        }
    }
}