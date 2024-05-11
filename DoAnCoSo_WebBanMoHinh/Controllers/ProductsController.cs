using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoAnCoSo_WebBanMoHinh.Models;
using DoAnCoSo_WebBanMoHinh.Repositories;

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
                return View("SearchResault", result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> ProductList(int pageNumber = 1)
        {

            int pageSize = 8;
            IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category);
            var paginatedProducts = await PaginatedList<Product>.CreateAsync((IQueryable<Product>)productsQuery, pageNumber, pageSize);
            return View(paginatedProducts);
        }
    }
}
