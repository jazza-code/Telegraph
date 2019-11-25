using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Threading.Tasks;
using AR.Telegraph.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Identity;
using System;

namespace AR.Telegraph.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<UserData> _userManager;
        private readonly IEmailSender _sender;

        public RegisterConfirmationModel(UserManager<UserData> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        public string Email { get; set; }

        public bool DisplayConfirmAccountLink { get; set; }


        public Uri EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;
            // Once you add a real email sender, you should remove this code that lets you confirm the account
            DisplayConfirmAccountLink = true;
            if (DisplayConfirmAccountLink)
            {
                var userId = await _userManager.GetUserIdAsync(user).ConfigureAwait(true);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var uriBuild = new UriBuilder()
                {
                    Path = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId, code },
                    protocol: Request.Scheme),
                    Query = null
                };
                EmailConfirmationUrl = uriBuild.Uri;
            }

            return Page();
        }
    }
}
