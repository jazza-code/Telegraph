using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using AR.Telegraph.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Globalization;
using AR.Telegraph.Areas.Identity.Models;

namespace AR.Telegraph.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<UserData> _signInManager;
        private readonly UserManager<UserData> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        public RegisterModel(
            UserManager<UserData> userManager,
            SignInManager<UserData> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }
        [BindProperty]
        public IdentityRegister Input { get; set; }
        public Uri ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; private set; }
        public async Task<IActionResult> OnGetAsync(Uri returnUrl = null)
        {
            var uriBuild = new UriBuilder()
            {
                Path = Url.Content("~/"),
                Query = null
            };
            returnUrl ??= uriBuild.Uri;
            if (User.Identity.IsAuthenticated)
            {
                return LocalRedirect(returnUrl.LocalPath);
            }
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync().ConfigureAwait(true)).ToList();
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(Uri returnUrl = null)
        {
            var uriBuild = new UriBuilder()
            {
                Path = Url.Content("~/"),
                Query = null
            };
            returnUrl ??= uriBuild.Uri;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync().ConfigureAwait(true)).ToList();
            if (ModelState.IsValid)
            {
                var user = new UserData();
                if (Input.Picture != null &&
                    Input.Picture.Length > 0 &&
                    !string.IsNullOrEmpty(Input.Picture.ContentType) &&
                    Input.Picture.ContentType.ToLower(CultureInfo.CurrentCulture).StartsWith("image/",StringComparison.CurrentCulture))
                {
                    using MemoryStream ms = new MemoryStream();
                    Input.Picture.OpenReadStream().CopyTo(ms);
                    user = new UserData
                    {
                        FirstName = Input.FirstName,
                        MiddleName = Input.MiddleName,
                        LastName = Input.LastName,
                        Gender = Input.Gender,
                        BirthDate = Input.BirthDate,
                        PictureType = Input.Picture.ContentType,
                        UserName = Input.Email,
                        Email = Input.Email
                    };
                    user.SetPicture(ms.ToArray());
                }
                else
                {
                    user = new UserData
                    {
                        FirstName = Input.FirstName,
                        MiddleName = Input.MiddleName,
                        LastName = Input.LastName,
                        Gender = Input.Gender,
                        BirthDate = Input.BirthDate,
                        UserName = Input.Email,
                        Email = Input.Email
                    };
                }
                var result = await _userManager.CreateAsync(user, Input.Password).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.".ToString(CultureInfo.CurrentCulture));

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.").ConfigureAwait(true);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false).ConfigureAwait(true);
                        return LocalRedirect(returnUrl.LocalPath);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
