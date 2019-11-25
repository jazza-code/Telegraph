using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AR.Telegraph.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using AR.Telegraph.Areas.Identity.Models;

namespace AR.Telegraph.Areas.Identity.Pages.Account.Manage
{
    public partial class EmailModel : PageModel
    {
        private readonly UserManager<UserData> _userManager;
        private readonly SignInManager<UserData> _signInManager;
        private readonly IEmailSender _emailSender;
        public EmailModel(
            UserManager<UserData> userManager,
            SignInManager<UserData> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }
        public string Username { get; set; }
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public IdentityEmail Input { get; set; }
        private async Task LoadAsync(UserData user)
        {
            var email = await _userManager.GetEmailAsync(user).ConfigureAwait(true);
            Email = email;

            Input = new IdentityEmail
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user).ConfigureAwait(true);
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم بمعرف '{_userManager.GetUserId(User)}'.");
            }
            await LoadAsync(user).ConfigureAwait(true);
            return Page();
        }
        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }
            if (!ModelState.IsValid)
            {
                await LoadAsync(user).ConfigureAwait(true);
                return Page();
            }
            var email = await _userManager.GetEmailAsync(user).ConfigureAwait(true);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user).ConfigureAwait(true);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail).ConfigureAwait(true);
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { userId, email = Input.NewEmail, code },
                    protocol: Request.Scheme);
                await _emailSender.SendEmailAsync(
                    Input.NewEmail,
                    "تأكيد بريدك الالكتروني",
                    $"يرجى تأكيد حسابك <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>تأكيد البريد الإلكتروني</a>.").ConfigureAwait(true);

                StatusMessage = "رابط التأكيد لتغيير البريد الإلكتروني المرسل. تفقد بريدك الالكتروني من فضلك.";
                return RedirectToPage();
            }
            StatusMessage = "لم يتم تغير البريد الإلكترني";
            await LoadAsync(user).ConfigureAwait(true);
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل المستخدم '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user).ConfigureAwait(true);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user).ConfigureAwait(true);
            var email = await _userManager.GetEmailAsync(user).ConfigureAwait(true);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(true);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId, code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "تأكيد بريدك الالكتروني",
                $"يرجى تأكيد حسابك <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>تأكيد البريد الإلكترني</a>.").ConfigureAwait(true);

            StatusMessage = "تم إرسال رسالة التحقق. تفقد بريدك الالكتروني من فضلك.";
            return RedirectToPage();
        }
    }
}
