using System.ComponentModel.DataAnnotations;

namespace AR.Telegraph.Areas.Identity.Models
{
    public class IdentityDeletePersonalData
    {
        [Required(ErrorMessage = "{0} مطلوبة")]
        [DataType(DataType.Password, ErrorMessage = "أدخل بريد إلكتروني صحيح.")]
        [Display(Name = "كلمة المرر")]
        public string Password { get; set; }
    }
}
