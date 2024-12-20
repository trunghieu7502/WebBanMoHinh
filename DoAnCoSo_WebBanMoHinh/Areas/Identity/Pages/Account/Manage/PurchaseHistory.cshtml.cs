using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DoAnCoSo_WebBanMoHinh.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DoAnCoSo_WebBanMoHinh.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class PurchaseHistoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PurchaseHistoryModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Order> Orders { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Orders = await _context.Orders
                    .Where(o => o.UserId == user.Id)
                    .Include(o => o.OrderDetails) // Load OrderDetails
                    .ThenInclude(od => od.Product) // Load Product t? OrderDetails
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
        }
    }
}
