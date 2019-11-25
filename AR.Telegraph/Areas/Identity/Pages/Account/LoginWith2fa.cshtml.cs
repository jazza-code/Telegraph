using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AR.Telegraph.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using AR.Telegraph.Areas.Identity.Models;
using System.Globalization;

namespace AR.Telegraph.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<UserData> _signInManager;
        private readonly ILogger<LoginWith2faModel> _logger;

        public LoginWith2faModel(SignInManager<UserData> signInManager, ILogger<LoginWith2faModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public IdentityLoginWith2fa Input { get; set; }
        public bool RememberMe { get; set; }
        public Uri ReturnUrl { get; set; }
        public async Task<IActionResult> OnGetAsync(bool rememberMe, Uri returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync().ConfigureAwait(true);

            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.".ToString(CultureInfo.CurrentCulture));
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }
        public async Task<IActionResult> OnPostAsync(bool rememberMe, Uri returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var uriBuild = new UriBuilder()
            {
                Path = Url.Content("~/"),
                Query = null
            };
            returnUrl ??= uriBuild.Uri;

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync().ConfigureAwait(true);
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.".ToString(CultureInfo.CurrentCulture));
            }

            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty,StringComparison.CurrentCultureIgnoreCase).Replace("-", string.Empty, StringComparison.CurrentCultureIgnoreCase);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine).ConfigureAwait(true);

            if (result.Succeeded)
            {
                _logger.LogInformation(string.Format(CultureInfo.CurrentCulture, "User with ID '{0}' logged in with 2fa.", user.Id));
                return LocalRedirect(returnUrl.LocalPath);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning(string.Format(CultureInfo.CurrentCulture, "User with ID '{0}' account locked out.", user.Id));
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning(string.Format(CultureInfo.CurrentCulture, "Invalid authenticator code entered for user with ID '{0}'.", user.Id));
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return Page();
            }
        }
    }
}
