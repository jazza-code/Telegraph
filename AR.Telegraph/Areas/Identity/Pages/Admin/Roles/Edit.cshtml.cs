using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace AR.Telegraph.Areas.Identity.Pages.Admin.Roles
{
    public class EditModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public EditModel(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        [BindProperty(SupportsGet = true)]
        public string Role { get; set; }
        [BindProperty, Required(ErrorMessage = "المجموعة مطلوبة")]
        public string NewRole { get; set; }
        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string role)
        {
            var existRole = await _roleManager.FindByNameAsync(role).ConfigureAwait(true);
            if (existRole == null)
            {
                return LocalRedirect(Url.Page("/Admin/Roles/Index"));
            }
            else
            {
                if (role == "Administrator")
                {
                    return LocalRedirect(Url.Page("/Admin/Roles/Index"));
                }
                Role = role;
                NewRole = role;
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(string role)
        {
            var existRole = await _roleManager.FindByNameAsync(role).ConfigureAwait(true);
            if (existRole == null)
            {
                return LocalRedirect(Url.Page("/Admin/Roles/Index"));
            }
            else if (existRole != null && existRole.Name == "Administrator")
            {
                StatusMessage = "خطأ , لا يمكن تعديل مجموعة المسؤول";
                return RedirectToPage();
            }
            else
            {
                var IsExistRole = await _roleManager.RoleExistsAsync(NewRole).ConfigureAwait(true);
                if (IsExistRole)
                {
                    StatusMessage = "خطأ , المجموعة مجودة بالفعل";
                    return RedirectToPage();
                }
                else
                {
                    existRole.Name = NewRole;
                    var result = await _roleManager.UpdateAsync(existRole).ConfigureAwait(true);
                    if (result.Succeeded)
                    {
                        StatusMessage = "تم تعديل الإسم بنجاح";
                        return LocalRedirect(Url.Page("/Admin/Roles/Edit", new { Role = NewRole }));
                    }
                    else
                    {
                        StatusMessage = "خطأ , لا يمكن تعديل المجموعة حاول لاحقا";
                        return Page();
                    }
                }
            }
        }
    }
}
