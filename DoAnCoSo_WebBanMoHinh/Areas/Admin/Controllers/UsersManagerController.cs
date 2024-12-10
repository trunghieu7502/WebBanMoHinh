using DoAnCoSo_WebBanMoHinh.Areas.Admin.Models;
using DoAnCoSo_WebBanMoHinh.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo_WebBanMoHinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = WebRoles.Role_Admin)]
    public class UsersManagerController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public UsersManagerController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var usersWithRoles = new List<UserRole>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault();

                usersWithRoles.Add(new UserRole
                {
                    User = user,
                    Role = userRole
                });
            }

            return View(usersWithRoles);
        }

        [HttpPost]
        public async Task<IActionResult> RoleChange(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
                return BadRequest("Mã người dùng hoặc chức vụ không được trống");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"Không tìm được người dùng với mã {userId}");

            var currentRoles = await _userManager.GetRolesAsync(user);
            foreach (var currentRole in currentRoles)
            {
                await _userManager.RemoveFromRoleAsync(user, currentRole);
            }
            var roleExist = await _roleManager.RoleExistsAsync(role);
            if (!roleExist)
                return BadRequest($"Chức vụ {role} không tồn tại");

            await _userManager.AddToRoleAsync(user, role);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Lockout(string userId, TimeSpan lockoutDuration)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Mã người dùng không được trống");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"Không tìm được người dùng với mã {userId}");

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.Now.Add(lockoutDuration);
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Index");
        }
    }
}
