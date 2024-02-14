using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PortalOgloszeniowy.Models
{
    public class RegisterUser
    {
        public RegisterUser(){}

        public RegisterUser(User user)
        {
            Id = user.Id;
            UserName = user.UserName;
            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
            Password = "";
            PasswordRequired = "";
        }

        [DisplayName("Id")]
        public string Id { get; set; }
        [Required]
        [DisplayName("Login")]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Hasło")]
        public string Password { get; set; }
        [Required]
        [EmailAddress]
        [DisplayName("E-mail")]
        public string Email { get; set; }
        [Phone]
        [DisplayName("Numer telefonu")]
        public string PhoneNumber { get; set; }
        [DataType(DataType.Password)]
        [DisplayName("Aby edytować dane podaj hasło")]
        public string PasswordRequired { get; set; }
    }
}
