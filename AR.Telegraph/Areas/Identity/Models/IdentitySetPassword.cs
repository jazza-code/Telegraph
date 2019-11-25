using System.ComponentModel.DataAnnotations;

namespace AR.Telegraph.Areas.Identity.Models
{
    public class IdentitySetPassword
    {
        [Required(ErrorMessage = "{0} مطلوبة")]
        [StringLength(100, ErrorMessage = "يجب أن يكون {0} على الأقل {2} وبحد أقصى {1} حرفًا.", MinimumLength = 6)]
        [DataType(DataType.Password, ErrorMessage = "كلمة المرر غير صحيحة")]
        [Display(Name = "كلمة المرر الجديدة")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password, ErrorMessage = "كلمة المرر غير صحيحة")]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور الجديدة وكلمة التأكيد غير متطابقة.")]
        public string ConfirmPassword { get; set; }
    }
}
