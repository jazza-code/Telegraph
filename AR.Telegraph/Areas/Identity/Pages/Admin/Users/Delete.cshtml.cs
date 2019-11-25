using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AR.Telegraph.Areas.Identity.Data;
using AR.Telegraph.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AR.Telegraph.Areas.Identity.Pages.Admin.Users
{
    public class DeleteModel : PageModel
    {
        private readonly UserManager<UserData> _userManager;
        private readonly SignInManager<UserData> _signInManager;

        public DeleteModel(
            UserManager<UserData> userManager,
            SignInManager<UserData> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
            if (user != null)
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
        public async Task<IList<string>> GetUserRolesAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username).ConfigureAwait(true);
            var userRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(true);
            return userRoles;
        }
        public async Task<IActionResult> OnGet(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return LocalRedirect(Url.Page("/Admin/Users/Index"));
            }
            UserName = username;
            await LoadUserByNameAsync(username).ConfigureAwait(true);
            if (Input == null)
            {
                return LocalRedirect(Url.Page("/Admin/Users/Index"));
            }
            return Page();
        }
        public async Task<IActionResult> OnPost(string username)
        {
            var user = await _userManager.FindByNameAsync(username).ConfigureAwait(true);
            var signInUser = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            var userInAdmin = await _userManager.GetUsersInRoleAsync("Administrator").ConfigureAwait(true);
            if (userInAdmin.Count <= 1)
            {
                if (userInAdmin.Contains(user))
                {
                    StatusMessage = "خطأ , لا يمكن حذف المستخدم قبل إنشاء حساب مسؤول بديل";
                    return RedirectToPage();
                }
            }
            if(user == signInUser)
            {
                var result = await _userManager.DeleteAsync(user).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    await _signInManager.SignOutAsync().ConfigureAwait(true);
                    return Redirect(Url.Page("/Index", new { area = "" }));
                }
                else
                {
                    StatusMessage = "خطأ , لا يمكن حذف الحساب حاول لاحقا";
                    return RedirectToPage();
                }
            }
            else
            {
                var result = await _userManager.DeleteAsync(user).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    return LocalRedirect(Url.Page("/Admin/Users/Index"));
                }
                else
                {
                    StatusMessage = "خطأ , لا يمكن حذف الحساب حاول لاحقا";
                    return RedirectToPage();
                }
            }
        }
    }
}
