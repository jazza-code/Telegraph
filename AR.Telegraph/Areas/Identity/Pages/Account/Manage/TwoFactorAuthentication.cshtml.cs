using System.Threading.Tasks;
using AR.Telegraph.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AR.Telegraph.Areas.Identity.Pages.Account.Manage
{
    public class TwoFactorAuthenticationModel : PageModel
    {
        private readonly UserManager<UserData> _userManager;
        private readonly SignInManager<UserData> _signInManager;
        private readonly ILogger<TwoFactorAuthenticationModel> _logger;

        public TwoFactorAuthenticationModel(
            UserManager<UserData> userManager,
            SignInManager<UserData> signInManager,
            ILogger<TwoFactorAuthenticationModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
        public bool HasAuthenticator { get; set; }

        public int RecoveryCodesLeft { get; set; }

        [BindProperty]
        public bool Is2faEnabled { get; set; }

        public bool IsMachineRemembered { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }

            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user).ConfigureAwait(true) != null;
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user).ConfigureAwait(true);
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user).ConfigureAwait(true);
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user).ConfigureAwait(true);

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }

            await _signInManager.ForgetTwoFactorClientAsync().ConfigureAwait(true);
            StatusMessage = "لقد تم نسيان المتصفح الحالي. عند تسجيل الدخول مرة أخرى من هذا المتصفح ، ستتم مطالبتك برمز 2fa الخاص بك.";
            return RedirectToPage();
        }
    }
}