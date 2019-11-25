using AR.Telegraph.Areas.Identity.Data;
using AR.Telegraph.Areas.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AR.Telegraph.Areas.Identity.Pages.Admin.Users
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<UserData> _userManager;

        public IndexModel(
            UserManager<UserData> userManager)
        {
            _userManager = userManager;
        }
        public IList<IdentityManageAccounts> Users { get; private set; }
        private async Task LoadUsers()
        {
            Users = await _userManager.Users.Select(x => new IdentityManageAccounts
            {
                UserName = x.UserName,
                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                LastName = x.LastName,
                BirthDate = x.BirthDate,
                Gender = x.Gender,
                ViewPicture = x.GetPicture() != null && x.GetPicture().Length > 0 && !string.IsNullOrEmpty(x.PictureType) ? string.Format(CultureInfo.InvariantCulture, "data:{0};base64,{1}", x.PictureType, Convert.ToBase64String(x.GetPicture())) : Url.Content("~/images/user.jpg"),
                Email = x.Email,
                EmailConfirmed = x.EmailConfirmed,
                PhoneNumber = x.PhoneNumber,
                PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                TwoFactorEnabled = x.TwoFactorEnabled,
                LockoutEnabled = x.LockoutEnabled,
                LockoutEnd = x.LockoutEnd,
                AccessFailedCount = x.AccessFailedCount
            }).ToListAsync().ConfigureAwait(true);
        }
        public async Task OnGetAsync()
        {
            await LoadUsers().ConfigureAwait(true);
        }
    }
}
