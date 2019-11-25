using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Linq;
using System.Threading.Tasks;
using AR.Telegraph.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System;
using System.Collections.Generic;
using AR.Telegraph.Areas.Identity.Models;

namespace AR.Telegraph.Areas.Identity.Pages.Account.Manage
{
    public class EnableAuthenticatorModel : PageModel
    {
        private readonly UserManager<UserData> _userManager;
        private readonly ILogger<EnableAuthenticatorModel> _logger;
        private readonly UrlEncoder _urlEncoder;

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public EnableAuthenticatorModel(
            UserManager<UserData> userManager,
            ILogger<EnableAuthenticatorModel> logger,
            UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _logger = logger;
            _urlEncoder = urlEncoder;
        }

        public string SharedKey { get; set; }
        public Uri AuthenticatorUri { get; private set; }
        public IList<string> RecoveryCodes { get; private set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public IdentityEnableAuthenticator Input { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }

            await LoadSharedKeyAndQrCodeUriAsync(user).ConfigureAwait(true);

            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user).ConfigureAwait(true);
                return Page();
            }

            // Strip spaces and hypens
            var verificationCode = Input.Code.Replace(" ", string.Empty, StringComparison.CurrentCultureIgnoreCase).Replace("-", string.Empty, StringComparison.CurrentCultureIgnoreCase);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode).ConfigureAwait(true);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Input.Code", "رمز التحقق غير صالح.");
                await LoadSharedKeyAndQrCodeUriAsync(user).ConfigureAwait(true);
                return Page();
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true).ConfigureAwait(true);
            var userId = await _userManager.GetUserIdAsync(user).ConfigureAwait(true);
            _logger.LogInformation($"قام المستخدم ذي المعرف '{userId}' بتمكين 2FA من خلال تطبيق مصادقة.", userId);

            StatusMessage = "تم التحقق من تطبيق الموثق.";

            if (await _userManager.CountRecoveryCodesAsync(user).ConfigureAwait(true) == 0)
            {
                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10).ConfigureAwait(true);
                RecoveryCodes = recoveryCodes.ToArray();
                return RedirectToPage("./ShowRecoveryCodes");
            }
            else
            {
                return RedirectToPage("./TwoFactorAuthentication");
            }
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(UserData user)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user).ConfigureAwait(true);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user).ConfigureAwait(true);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user).ConfigureAwait(true);
            }

            SharedKey = FormatKey(unformattedKey);

            var email = await _userManager.GetEmailAsync(user).ConfigureAwait(true);
            var uriBuild = new UriBuilder()
            {
                Path = GenerateQrCodeUri(email, unformattedKey)
            };
            AuthenticatorUri = uriBuild.Uri;
        }
        private static string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToUpperInvariant();
        }
        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                AuthenticatorUriFormat,
                _urlEncoder.Encode("AR.Telegraph"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
    }
}
