using System.ComponentModel.DataAnnotations;

namespace AR.Telegraph.Areas.Identity.Models
{
    public class IdentityEmail
    {
        [Required(ErrorMessage = "{0} مطلوب")]
        [EmailAddress(ErrorMessage = "أدخل بريد إلكتروني صحيح.")]
        [Display(Name = "البريد الإلكترني")]
        public string NewEmail { get; set; }
    }
}
