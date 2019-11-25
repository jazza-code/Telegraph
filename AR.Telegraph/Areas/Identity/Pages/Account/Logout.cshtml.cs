using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AR.Telegraph.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace AR.Telegraph.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<UserData> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<UserData> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }
        public async Task<IActionResult> OnPost(Uri returnUrl = null)
        {
            await _signInManager.SignOutAsync().ConfigureAwait(true);
            _logger.LogInformation("User logged out.".ToString(CultureInfo.CurrentCulture));
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl.LocalPath);
            }
            else
            {
                return RedirectToPage();
            }
        }
    }
}
