using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PortalOgloszeniowy.Models
{
    public class LoginUser
    {
        [Required]
        [DisplayName("Login")]
        public string UserName { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [DisplayName("Hasło")]
        public string Password { get; set; }
        [Required]
        [DisplayName("Zapamiętaj mnie")]
        public bool RememberMe { get; set; }
    }
}
