using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AR.Telegraph.Areas.Identity.Pages.Admin.Roles
{
    public class IndexModel : PageModel
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public IndexModel(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        [TempData]
        public string StatusMessage { get; set; }
        public IList<string> Roles { get; private set; }
        public async Task OnGetAsync()
        {
            Roles = await _roleManager.Roles.Select(x => x.Name).ToListAsync().ConfigureAwait(true);
        }
    }
}
