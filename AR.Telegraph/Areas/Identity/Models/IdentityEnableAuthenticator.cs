using System.ComponentModel.DataAnnotations;

namespace AR.Telegraph.Areas.Identity.Models
{
    public class IdentityEnableAuthenticator
    {
        [Required(ErrorMessage = "{0} مطلوب")]
        [StringLength(7, ErrorMessage = "يجب أن يكون {0} على الأقل {2} وبحد أقصى {1} حرفًا.", MinimumLength = 6)]
        [DataType(DataType.Text, ErrorMessage = "الرمز غير صحيح")]
        [Display(Name = "رمز التحقق")]
        public string Code { get; set; }
    }
}
