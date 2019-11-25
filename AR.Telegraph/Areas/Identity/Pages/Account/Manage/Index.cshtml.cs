using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using AR.Telegraph.Areas.Identity.Data;
using AR.Telegraph.Areas.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AR.Telegraph.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<UserData> _userManager;
        private readonly SignInManager<UserData> _signInManager;

        public IndexModel(
            UserManager<UserData> userManager,
            SignInManager<UserData> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public IdentityProfile Input { get; set; }

        private void Load(UserData user)
        {
            Username = user.UserName;
            if (user.GetPicture() != null && user.PictureType != null && user.GetPicture().Length > 0 && user.PictureType.Length > 0)
            {
                Input = new IdentityProfile
                {
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    BirthDate = user.BirthDate,
                    ViewPicture = string.Format(CultureInfo.InvariantCulture,"data:{0};base64,{1}", user.PictureType, Convert.ToBase64String(user.GetPicture())),
                    PhoneNumber = user.PhoneNumber
                };
            }
            else
            {
                Input = new IdentityProfile
                {
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    BirthDate = user.BirthDate,
                    PhoneNumber = user.PhoneNumber
                };
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"غير قادر على تحميل بيانات المستخدم '{_userManager.GetUserId(User)}'.");
            }

            Load(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            if (!ModelState.IsValid)
            {
                Load(user);
                return Page();
            }
            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
                StatusMessage = "تم تحديث البيانات الشخصية .";
            }
            if (Input.MiddleName != user.MiddleName)
            {
                user.MiddleName = Input.MiddleName;
                StatusMessage = "تم تحديث البيانات الشخصية .";
            }
            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
                StatusMessage = "تم تحديث البيانات الشخصية .";
            }
            if (Input.Gender != user.Gender)
            {
                user.Gender = Input.Gender;
                StatusMessage = "تم تحديث البيانات الشخصية .";
            }
            if (Input.BirthDate != user.BirthDate)
            {
                user.BirthDate = Input.BirthDate;
                StatusMessage = "تم تحديث البيانات الشخصية .";
            }
            if (Input.Picture != null && Input.Picture.ContentType.ToLower(CultureInfo.CurrentCulture).StartsWith("image/", StringComparison.InvariantCultureIgnoreCase))
            {
                using MemoryStream ms = new MemoryStream();
                Input.Picture.OpenReadStream().CopyTo(ms);
                byte[] msArray = ms.ToArray();
                if (msArray != user.GetPicture())
                {
                    user.SetPicture(msArray);
                    user.PictureType = Input.Picture.ContentType;
                    StatusMessage = "تم تحديث البيانات الشخصية .";
                }
            }
            if (Input.PhoneNumber != user.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber).ConfigureAwait(true);
                if (!setPhoneResult.Succeeded)
                {
                    var userId = await _userManager.GetUserIdAsync(user).ConfigureAwait(true);
                    StatusMessage = $"حدث خطأ غير متوقع في إعداد رقم الهاتف للمستخدم ذي المعرف '{userId}'.";
                }
                StatusMessage = "تم تحديث البيانات الشخصية .";
            }
            StatusMessage = string.IsNullOrEmpty(StatusMessage) ? "البيانات الشخصية لم تتغير." : StatusMessage;
            await _userManager.UpdateAsync(user).ConfigureAwait(true);
            await _signInManager.RefreshSignInAsync(user).ConfigureAwait(true);
            return RedirectToPage();
        }
    }
}
