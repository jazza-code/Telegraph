using AR.Telegraph.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace AR.Telegraph.Areas.Identity.Models
{
    public class IdentityRegister
    {
        [Required(ErrorMessage = "{0} مطلوب")]
        [Display(Name = "الإسم الأول")]
        public string FirstName { get; set; }
        [Display(Name = "الإسم الأوسط")]
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "{0} مطلوب")]
        [Display(Name = "الإسم الأخير")]
        public string LastName { get; set; }
        [Display(Name = "الجنس")]
        public string Gender { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "تاريخ الميلاد")]
        public DateTime? BirthDate { get; set; }
        [Required(ErrorMessage = "{0} مطلوب")]
        [EmailAddress(ErrorMessage = "أدخل بريد إلكتروني صحيح.")]
        [Display(Name = "البريد الإلكترني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "{0} مطلوبة")]
        [StringLength(100, ErrorMessage = "يجب أن يكون {0} على الأقل {2} وبحد أقصى {1} حرفًا.", MinimumLength = 6)]
        [DataType(DataType.Password, ErrorMessage = "أدخل بريد إلكتروني صحيح.")]
        [Display(Name = "كلمة المرر")]
        public string Password { get; set; }
        [DataType(DataType.Upload, ErrorMessage = "خطأ في تحميل الصورة")]
        [MaxFileSize(5 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".png", ".jpg" })]
        public IFormFile Picture { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمة المرور وكلمة المرور للتأكيد غير متطابقتين.")]
        public string ConfirmPassword { get; set; }
    }
}
