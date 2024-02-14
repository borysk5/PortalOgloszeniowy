
using System.ComponentModel.DataAnnotations;

namespace PortalOgloszeniowy.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Text { get; set; }
    }
}
