using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AR.Telegraph.Areas.Identity.Pages.Admin.Roles
{
    public class DeleteModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public DeleteModel(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        [Required(ErrorMessage = "المجموعة مطلوبة"), BindProperty(SupportsGet = true)]
        public string Role { get; set; }
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
                if(role == "Administrator")
                {
                    return LocalRedirect(Url.Page("/Admin/Roles/Index"));
                }
                Role = role;
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
            else if(existRole != null && existRole.Name == "Administrator")
            {
                StatusMessage = "خطأ , لا يمكن حذف مجموعة المسؤول";
                return RedirectToPage();
            }
            else
            {
                var result = await _roleManager.DeleteAsync(existRole).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    return LocalRedirect(Url.Page("/Admin/Roles/Index"));
                }
                else
                {
                    StatusMessage = "خطأ , لا يمكن حذف هذه المجموعة حاول مرة أخرى";
                    return Page();
                }
            }
        }
    }
}
