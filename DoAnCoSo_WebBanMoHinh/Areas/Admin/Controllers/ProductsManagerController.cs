using DoAnCoSo_WebBanMoHinh.Models;
using DoAnCoSo_WebBanMoHinh.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo_WebBanMoHinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebRoles.Role_Admin)]
    public class ProductsManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICompanyRepository _companyRepository;

        public ProductsManagerController(ApplicationDbContext context, IProductRepository productRepository, ICategoryRepository categoryRepository, ICompanyRepository companyRepository)
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

        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName); // Thay đổi đường dẫn theo cấu hình của bạn
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối
        }

        // GET: Products/Create
        public async Task<ActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var companies = await _companyRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "CateName");
            ViewBag.Companies = new SelectList(companies, "Id", "CompName");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageUrl, List<IFormFile> images)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                if (images != null)
                {
                    product.Images = new List<ProductImage>();
                    foreach (var item in images)
                    {
                        ProductImage image = new ProductImage
                        {
                            ProductId = product.Id,
                            Url = await SaveImage(item)
                        };
                        product.Images.Add(image);
                    }
                }
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            var companies = await _companyRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "CateName");
            ViewBag.Companies = new SelectList(companies, "Id", "CompName");
            return View(product);
        }


        // GET: Products/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            var companies = await _companyRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "CateName", product.CategoryId);
            ViewBag.Companies = new SelectList(companies, "Id", "CompName", product.CompanyId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile imageUrl, List<IFormFile> images)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                if (images != null)
                {
                    product.Images = new List<ProductImage>();
                    foreach (var item in images)
                    {
                        ProductImage image = new ProductImage
                        {
                            ProductId = product.Id,
                            Url = await SaveImage(item)
                        };
                        foreach (var control in product.Images)
                        {
                            if (image.Url == control.Url)
                                continue;
                            else
                                product.Images.Add(image);
                        }
                    }
                }
                await _productRepository.UpdateAsync(product);
                if (product.Stock == 0)
                {
                    product.Status = false;
                    await _productRepository.UpdateAsync(product);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            var categories = await _categoryRepository.GetByIdAsync(id);
            var companies = await _companyRepository.GetAllAsync();
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SearchProductsAsync(string query)
        {
            var products = await _productRepository.GetAllAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    query = "null";
                var result = products.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                            (p.Description != null && p.Description.Contains(query, StringComparison.OrdinalIgnoreCase))).ToList();
                return View("SearchResault", result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
