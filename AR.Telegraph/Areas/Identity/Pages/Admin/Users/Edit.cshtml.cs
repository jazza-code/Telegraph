using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AR.Telegraph.Areas.Identity.Data;
using AR.Telegraph.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AR.Telegraph.Areas.Identity.Pages.Admin.Users
{
    public class EditModel : PageModel
    {
        private readonly UserManager<UserData> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EditModel(
            UserManager<UserData> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [TempData]
        public string StatusMessage { get; set; }
        [BindProperty(SupportsGet = true)]
        public string UserName { get; set; }
        [BindProperty]
        public IEnumerable<string> Roles { get; set; }
        public IdentityManageAccounts Input { get; set; }
        private async Task LoadUserByNameAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username).ConfigureAwait(true);
            if(user != null)
            {
                Input = new IdentityManageAccounts
                {
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    BirthDate = user.BirthDate,
                    Gender = user.Gender,
                    ViewPicture = user.GetPicture() != null && user.GetPicture().Length > 0 && !string.IsNullOrEmpty(user.PictureType) ? string.Format(CultureInfo.InvariantCulture, "data:{0};base64,{1}", user.PictureType, Convert.ToBase64String(user.GetPicture())) : Url.Content("~/images/user.jpg"),
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd,
                    AccessFailedCount = user.AccessFailedCount
                };
            }
        }
        public async Task<MultiSelectList> GetUserRolesAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username).ConfigureAwait(true);
            var userRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(true);
            var _roles = await _roleManager.Roles.Select(x => new IdentityManageRoles
            {
                Name = x.Name
            }).ToListAsync().ConfigureAwait(true);
            return new MultiSelectList(_roles, "Name", "Name", userRoles);
        }
        public async Task<IActionResult> OnGet(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return LocalRedirect(Url.Page("/Admin/Users/Index"));
            }
            UserName = username;
            await LoadUserByNameAsync(username).ConfigureAwait(true);
            if(Input == null)
            {
                return LocalRedirect(Url.Page("/Admin/Users/Index"));
            }
            return Page();
        }
        public async Task<IActionResult> OnPost(string username)
        {
            var userInAdmin = await _userManager.GetUsersInRoleAsync("Administrator").ConfigureAwait(true);
            var user = await _userManager.FindByNameAsync(username).ConfigureAwait(true);
            var roles = Roles;
            if (userInAdmin.Count <= 1)
            {
                if (userInAdmin.Contains(user) && !roles.Contains("Administrator"))
                {
                    StatusMessage = "خطأ , يجب أن تحتوي مجوعة المسؤول على مستخدم على الأقل لا يمكنك تغير المجموعة";
                    return RedirectToPage();
                }
            }
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(true);
                await _userManager.RemoveFromRolesAsync(user, userRoles).ConfigureAwait(true);
                if (roles != null && roles.Any())
                {
                    foreach (var r in roles)
                    {
                        var role = await _roleManager.FindByNameAsync(r).ConfigureAwait(true);
                        if (role != null && !string.IsNullOrEmpty(role.Name))
                        {
                            await _userManager.AddToRoleAsync(user, role.Name).ConfigureAwait(true);
                            StatusMessage = "تم إضافة المجموعة/المجموعات بنجاح";
                        }
                    }
                }
            }
            return RedirectToPage();
        }
    }
}
