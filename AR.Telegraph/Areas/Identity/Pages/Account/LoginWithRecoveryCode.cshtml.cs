using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
    public class LoginWithRecoveryCodeModel : PageModel
    {
        private readonly SignInManager<UserData> _signInManager;
        private readonly ILogger<LoginWithRecoveryCodeModel> _logger;

        public LoginWithRecoveryCodeModel(SignInManager<UserData> signInManager, ILogger<LoginWithRecoveryCodeModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public IdentityLoginWithRecoveryCode Input { get; set; }

        public Uri ReturnUrl { get; set; }
        public async Task<IActionResult> OnGetAsync(Uri returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync().ConfigureAwait(true);
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.".ToString(CultureInfo.CurrentCulture));
            }
            ReturnUrl = returnUrl;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(Uri returnUrl = null)
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

            var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty, StringComparison.CurrentCultureIgnoreCase);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode).ConfigureAwait(true);

            if (result.Succeeded)
            {
                _logger.LogInformation(string.Format(CultureInfo.CurrentCulture, "User with ID '{0}' logged in with a recovery code.", user.Id));
                return LocalRedirect(returnUrl.LocalPath);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(string.Format(CultureInfo.CurrentCulture, "User with ID '{0}' account locked out.", user.Id));
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning(string.Format(CultureInfo.CurrentCulture, "Invalid recovery code entered for user with ID '{0}' ", user.Id));
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return Page();
            }
        }
    }
}
