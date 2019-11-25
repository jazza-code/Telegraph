using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AR.Telegraph.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using System.Net;
using System.IO;
using AR.Telegraph.Areas.Identity.Models;
using System.Collections.Generic;

namespace AR.Telegraph.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<UserData> _signInManager;
        private readonly UserManager<UserData> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<UserData> signInManager,
            UserManager<UserData> userManager,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public IdentityExternalLogin Input { get; set; }

        public string LoginProvider { get; set; }


        public Uri ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, Uri returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }
        public async Task<IActionResult> OnGetCallbackAsync(Uri returnUrl = null, string remoteError = null)
        {
            var uriBuild = new UriBuilder()
            {
                Path = Url.Content("~/"),
                Query = null
            };
            returnUrl ??= uriBuild.Uri;
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync().ConfigureAwait(true);
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor : true).ConfigureAwait(true);
            if (result.Succeeded)
            {
                _logger.LogInformation(string.Format(CultureInfo.CurrentCulture, "{0} logged in with {1} provider.", info.Principal.Identity.Name, info.LoginProvider));
                return LocalRedirect(returnUrl.LocalPath);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                LoginProvider = info.LoginProvider;
                Input = new IdentityExternalLogin();
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.GivenName))
                {
                    Input.FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                }
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Surname))
                {
                    Input.LastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
                }
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Gender))
                {
                    Input.Gender = info.Principal.FindFirstValue(ClaimTypes.Gender);
                }
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.DateOfBirth))
                {
                    Input.BirthDate = Convert.ToDateTime(info.Principal.FindFirstValue(ClaimTypes.DateOfBirth), CultureInfo.CurrentCulture);
                }
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input.Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                }
                if (info.LoginProvider.ToLower(CultureInfo.CurrentCulture) == "google" && info.Principal.HasClaim(c => c.Type == "urn:google:picture"))
                {
                    Input.ViewPicture = info.Principal.FindFirst("urn:google:picture").ToString().Replace("urn:google:picture:", "",StringComparison.CurrentCultureIgnoreCase);
                }
                else if (info.LoginProvider.ToLower(CultureInfo.CurrentCulture) == "facebook")
                {
                    var claim = info.Principal.Claims.Where(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").FirstOrDefault();
                    var url = string.Format(CultureInfo.CurrentCulture,"http://graph.facebook.com/{0}/picture?type=large", claim.Value);
                    Input.ViewPicture = url;
                }
                return Page();
            }
        }
        public async Task<IActionResult> OnPostConfirmationAsync(Uri returnUrl = null)
        {
            var uriBuild = new UriBuilder()
            {
                Path = Url.Content("~/"),
                Query = null
            };
            returnUrl ??= uriBuild.Uri;
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync().ConfigureAwait(true);
            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new UserData();
                if (Input.Picture != null && Input.Picture.ContentType.ToLower(CultureInfo.CurrentCulture).StartsWith("image/",StringComparison.CurrentCultureIgnoreCase))
                {
                    using MemoryStream ms = new MemoryStream();
                    Input.Picture.OpenReadStream().CopyTo(ms);
                    byte[] msArray = ms.ToArray();
                    if (msArray != user.GetPicture())
                    {
                        user = new UserData
                        {
                            FirstName = Input.FirstName,
                            MiddleName = Input.MiddleName,
                            LastName = Input.LastName,
                            Gender = Input.Gender,
                            BirthDate = Input.BirthDate,
                            UserName = Input.Email,
                            Email = Input.Email,
                            PhoneNumber = Input.PhoneNumber,
                            PictureType = Input.Picture.ContentType
                        };
                        user.SetPicture(msArray);
                    }
                }
                else if (!string.IsNullOrEmpty(Input.ViewPicture))
                {
                    byte[] picture = Array.Empty<byte>();
                    string pictureType = "";

                    using (WebClient webclient = new WebClient())
                    {
                        picture = webclient.DownloadData(Input.ViewPicture);
                        pictureType = webclient.ResponseHeaders["content-type"];
                    }
                    if (picture != null && picture.Length > 0 && !string.IsNullOrEmpty(pictureType))
                    {
                        user = new UserData
                        {
                            FirstName = Input.FirstName,
                            MiddleName = Input.MiddleName,
                            LastName = Input.LastName,
                            Gender = Input.Gender,
                            BirthDate = Input.BirthDate,
                            UserName = Input.Email,
                            Email = Input.Email,
                            PhoneNumber = Input.PhoneNumber,
                            PictureType = pictureType
                        };
                        user.SetPicture(picture);
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
                            Email = Input.Email,
                            PhoneNumber = Input.PhoneNumber
                        };
                    }
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
                        Email = Input.Email,
                        PhoneNumber = Input.PhoneNumber
                    };
                }

                var result = await _userManager.CreateAsync(user).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info).ConfigureAwait(true);
                    if (result.Succeeded)
                    {
                        var props = new AuthenticationProperties();
                        props.StoreTokens(info.AuthenticationTokens);
                        props.IsPersistent = true;
                        await _signInManager.SignInAsync(user, props).ConfigureAwait(true);
                        _logger.LogInformation(string.Format(CultureInfo.CurrentCulture, "User created an account using {0} provider.", info.LoginProvider));

                        var userId = await _userManager.GetUserIdAsync(user).ConfigureAwait(true);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId, code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.").ConfigureAwait(true);

                        return LocalRedirect(returnUrl.LocalPath);
                    }
                }
                else
                {
                    ErrorMessage = result.Errors.FirstOrDefault().Description;
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            LoginProvider = info.LoginProvider;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
