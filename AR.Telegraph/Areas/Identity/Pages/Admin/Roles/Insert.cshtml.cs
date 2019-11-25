using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace AR.Telegraph.Areas.Identity.Pages.Admin.Roles
{
    public class InsertModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public InsertModel(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        [TempData]
        public string StatusMessage { get; set; }
        [BindProperty, Required(ErrorMessage = "المجموعة مطلوبة")]
        public string Role { get; set; }
        public async Task<IActionResult> OnPostAsync()
        {
            var existRole = await _roleManager.FindByNameAsync(Role).ConfigureAwait(true);
            if (existRole == null)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(Role)).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    StatusMessage = "تم إضافة المجموعة بنجاح";
                    return LocalRedirect(Url.Page("/Admin/Roles/Index"));
                }
                else
                {
                    StatusMessage = "خطأ , لايمكن إضافة المجموعة";
                }
            }
            else
            {
                StatusMessage = "خطأ , المجموعة موجودة بالفعل";
            }
            return Page();
        }
    }
}
