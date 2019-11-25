using System.ComponentModel.DataAnnotations;

namespace AR.Telegraph.Areas.Identity.Models
{
    public class IdentityLoginWithRecoveryCode
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "رمز الاسترداد")]
        public string RecoveryCode { get; set; }
    }
}
