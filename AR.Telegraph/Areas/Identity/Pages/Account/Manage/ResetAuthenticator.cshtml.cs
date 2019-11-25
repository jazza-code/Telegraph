using System.Threading.Tasks;
using AR.Telegraph.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AR.Telegraph.Areas.Identity.Pages.Account.Manage
{
    public class ResetAuthenticatorModel : PageModel
    {
        UserManager<UserData> _userManager;
        private readonly SignInManager<UserData> _signInManager;
        ILogger<ResetAuthenticatorModel> _logger;

        public ResetAuthenticatorModel(
            UserManager<UserData> userManager,
            SignInManager<UserData> signInManager,
            ILogger<ResetAuthenticatorModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false).ConfigureAwait(true);
            await _userManager.ResetAuthenticatorKeyAsync(user).ConfigureAwait(true);
            _logger.LogInformation($"قام المستخدم ذي المعرف '{user.Id}' بإعادة تعيين مفتاح تطبيق المصادقة الخاص بهم.");

            await _signInManager.RefreshSignInAsync(user).ConfigureAwait(true);
            StatusMessage = "تمت إعادة تعيين مفتاح تطبيق الموثق ، ستحتاج إلى تكوين تطبيق الموثق باستخدام المفتاح الجديد.";

            return RedirectToPage("./EnableAuthenticator");
        }
    }
}