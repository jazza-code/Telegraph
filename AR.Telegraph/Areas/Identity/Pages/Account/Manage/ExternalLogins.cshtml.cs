using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AR.Telegraph.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AR.Telegraph.Areas.Identity.Pages.Account.Manage
{
    public class ExternalLoginsModel : PageModel
    {
        private readonly UserManager<UserData> _userManager;
        private readonly SignInManager<UserData> _signInManager;
        public ExternalLoginsModel(
            UserManager<UserData> userManager,
            SignInManager<UserData> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IList<UserLoginInfo> CurrentLogins { get; private set; }
        public IList<AuthenticationScheme> OtherLogins { get; private set; }
        public bool ShowRemoveButton { get; set; }
        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }

            CurrentLogins = await _userManager.GetLoginsAsync(user).ConfigureAwait(true);
            OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync().ConfigureAwait(true))
                .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
                .ToList();
            ShowRemoveButton = user.PasswordHash != null || CurrentLogins.Count > 1;
            return Page();
        }
        public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }

            var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey).ConfigureAwait(true);
            if (!result.Succeeded)
            {
                StatusMessage = "لم تتم إزالة تسجيل الدخول الخارجي.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user).ConfigureAwait(true);
            StatusMessage = "تمت إزالة تسجيل الدخول الخارجي.";
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme).ConfigureAwait(true);

            // Request a redirect to the external login provider to link a login for the current user
            var redirectUrl = Url.Page("./ExternalLogins", pageHandler: "LinkLoginCallback");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return new ChallengeResult(provider, properties);
        }
        public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user).ConfigureAwait(true)).ConfigureAwait(true);
            if (info == null)
            {
                throw new InvalidOperationException($"حدث خطأ غير متوقع أثناء تحميل معلومات تسجيل الدخول الخارجية للمستخدم ذي المعرف '{user.Id}'.");
            }

            var result = await _userManager.AddLoginAsync(user, info).ConfigureAwait(true);
            if (!result.Succeeded)
            {
                StatusMessage = "لم تتم إضافة تسجيل الدخول الخارجي. لا يمكن ربط تسجيلات الدخول الخارجية إلا بحساب واحد.";
                return RedirectToPage();
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme).ConfigureAwait(true);

            StatusMessage = "تمت إضافة تسجيل الدخول الخارجي.";
            return RedirectToPage();
        }
    }
}
