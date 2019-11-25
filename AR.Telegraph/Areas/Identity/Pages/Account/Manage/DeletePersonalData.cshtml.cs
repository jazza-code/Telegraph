using System;
using System.Threading.Tasks;
using AR.Telegraph.Areas.Identity.Data;
using AR.Telegraph.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AR.Telegraph.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<UserData> _userManager;
        private readonly SignInManager<UserData> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        public DeletePersonalDataModel(
            UserManager<UserData> userManager,
            SignInManager<UserData> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public IdentityDeletePersonalData Input { get; set; }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user).ConfigureAwait(true);
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user).ConfigureAwait(true);
            if (RequirePassword)
            {
                if (!await _userManager.CheckPasswordAsync(user, Input.Password).ConfigureAwait(true))
                {
                    ModelState.AddModelError(string.Empty, "كلمة المرور غير صحيحة.");
                    return Page();
                }
            }
            var userInAdmin = await _userManager.GetUsersInRoleAsync("Administrator").ConfigureAwait(true);
            if (userInAdmin.Count <= 1)
            {
                if (userInAdmin.Contains(user))
                {
                    ModelState.AddModelError(string.Empty,"خطأ , لا يمكن حذف المستخدم قبل إنشاء حساب مسؤول بديل");
                    return Page();
                }
            }
            var result = await _userManager.DeleteAsync(user).ConfigureAwait(true);
            var userId = await _userManager.GetUserIdAsync(user).ConfigureAwait(true);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"حدث خطأ غير متوقع في حذف المستخدم '{userId}'.");
            }

            await _signInManager.SignOutAsync().ConfigureAwait(true);

            _logger.LogInformation($"حذف المستخدم ذو الكود '{userId}' نفسه.");

            return Redirect("~/");
        }
    }
}
