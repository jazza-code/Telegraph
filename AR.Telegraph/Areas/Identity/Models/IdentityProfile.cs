using AR.Telegraph.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace AR.Telegraph.Areas.Identity.Models
{
    public class IdentityProfile
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
        [DataType(DataType.Upload, ErrorMessage = "خطأ في تحميل الصورة")]
        [MaxFileSize(5 * 1024 * 1024)]
        [AllowedExtensions(new string[] { ".png", ".jpg" })]
        public IFormFile Picture { get; set; }
        public string ViewPicture { get; set; }
        [Phone(ErrorMessage = "{0} غير صحيح")]
        [Display(Name = "رقم الهاتف")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "رقم الهاتف غير صحيح.")]
        public string PhoneNumber { get; set; }
    }
}
