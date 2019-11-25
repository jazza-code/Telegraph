using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AR.Telegraph.Areas.Identity.Pages.Account.Manage
{
    public static class ManageNavPages
    {
        public static string Index => "الملف الشخصي";

        public static string Email => "البريد الإلكترني";

        public static string ChangePassword => "تغير كلمة المرر";

        public static string ExternalLogins => "تسجيلات الدخول الخارجية";

        public static string PersonalData => "البيانات شخصية";

        public static string TwoFactorAuthentication => "توثيق ذو عاملين";

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        public static string EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, Email);

        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

        public static string ExternalLoginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExternalLogins);

        public static string PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);

        public static string TwoFactorAuthenticationNavClass(ViewContext viewContext) => PageNavClass(viewContext, TwoFactorAuthentication);

        private static string PageNavClass(ViewContext viewContext, string page)
        {
            if (viewContext != null && !string.IsNullOrEmpty(page))
            {
                var activePage = viewContext.ViewData["ActivePage"] as string
                    ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
                return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
            }
            else
            {
                throw new ArgumentNullException(nameof(viewContext));
            }
        }
    }
}
