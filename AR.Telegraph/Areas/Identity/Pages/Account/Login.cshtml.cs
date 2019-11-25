using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AR.Telegraph.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Globalization;
using AR.Telegraph.Areas.Identity.Models;
using System;

namespace AR.Telegraph.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<UserData> _userManager;
        private readonly SignInManager<UserData> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IEmailSender _emailSender;

        public LoginModel(SignInManager<UserData> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<UserData> userManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public IdentityLogin Input { get; set; }

        public List<AuthenticationScheme> ExternalLogins { get; private set; }

        public Uri ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(Uri returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
            var uriBuild = new UriBuilder()
            {
                Path = returnUrl == null ? Url.Content("~/") : Url.Content(returnUrl.OriginalString),
                Query = null
            };
            ReturnUrl = uriBuild.Uri;
            if (User.Identity.IsAuthenticated)
            {
                return LocalRedirect(ReturnUrl.LocalPath);
            }
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme).ConfigureAwait(true);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync().ConfigureAwait(true)).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Uri returnUrl = null)
        {
            var uriBuild = new UriBuilder() {
                Path = returnUrl == null ? Url.Content("~/") : Url.Content(returnUrl.OriginalString),
                Query = null
            };
            ReturnUrl = uriBuild.Uri;

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.".ToString(CultureInfo.CurrentCulture));
                    return LocalRedirect(ReturnUrl.LocalPath);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.".ToString(CultureInfo.CurrentCulture));
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync().ConfigureAwait(true)).ToList();
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync().ConfigureAwait(true)).ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email).ConfigureAwait(true);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            }

            var userId = await _userManager.GetUserIdAsync(user).ConfigureAwait(true);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId, code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                Input.Email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.").ConfigureAwait(true);

            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return Page();
        }
    }
}
