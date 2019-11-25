using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AR.Telegraph.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the UserData class
    public class UserData : IdentityUser
    {
        [PersonalData]
        public string FirstName { get; set; }
        [PersonalData]
        public string MiddleName { get; set; }
        [PersonalData]
        public string LastName { get; set; }
        [PersonalData]
        public string Gender { get; set; }
        [PersonalData]
        public DateTime? BirthDate { get; set; }
        [PersonalData]
        [Column("Picture")]
        private byte[] Picture;
        public byte[] GetPicture()
        {
            return Picture;
        }
        public void SetPicture(byte[] pic)
        {
            Picture = pic;
        }
        public string PictureType { get; set; }
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }
}
