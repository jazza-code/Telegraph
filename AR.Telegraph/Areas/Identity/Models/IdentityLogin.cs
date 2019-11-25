using System.ComponentModel.DataAnnotations;

namespace AR.Telegraph.Areas.Identity.Models
{
    public class IdentityLogin
    {
        [Required(ErrorMessage = "{0} مطلوب")]
        [EmailAddress(ErrorMessage = "أدخل بريد إلكتروني صحيح.")]
        [Display(Name = "البريد الإلكترني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "{0} مطلوبة")]
        [DataType(DataType.Password, ErrorMessage = "أدخل بريد إلكتروني صحيح.")]
        [Display(Name = "كلمة المرر")]
        public string Password { get; set; }

        [Display(Name = "تذكرني ؟")]
        public bool RememberMe { get; set; }
    }
}
