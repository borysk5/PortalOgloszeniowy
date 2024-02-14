using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PortalOgloszeniowy.Models
{
    public class User : IdentityUser
    {
        [Key]
        public override string Id { get; set; }
        [Required]
        [EmailAddress]
        [DisplayName("E-mail")]
        public override string Email { get; set; }
        [Required]
        [DisplayName("Imię i nazwisko")]
        public override string UserName { get; set; }
        [Phone]
        [DisplayName("Numer telefonu")]
        public override string PhoneNumber { get; set; }
        [Required]
        public bool Ban { get; set; }
        [DisplayName("Ogłoszenia")]
        public virtual IEnumerable<Post> Posts { get; set; }
        [DisplayName("Komentarze")]
        public virtual IEnumerable<Comment> Comments { get; set; }
        [DisplayName("Powiadomienia")]
        public virtual IEnumerable<Notification> Notifications { get; set; }
    }
}
