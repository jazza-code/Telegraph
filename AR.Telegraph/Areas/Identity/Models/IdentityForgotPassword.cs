using System.ComponentModel.DataAnnotations;

namespace AR.Telegraph.Areas.Identity.Models
{
    public class IdentityForgotPassword
    {
        [Required(ErrorMessage = "{0} مطلوب")]
        [EmailAddress(ErrorMessage = "أدخل بريد إلكتروني صحيح.")]
        [Display(Name = "البريد الإلكترني")]
        public string Email { get; set; }
    }
}
