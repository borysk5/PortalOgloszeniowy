using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PortalOgloszeniowy.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Treść")]
        public string Text { get; set; }
        [Required]
        [DisplayName("Data utworzenia")]
        public DateTime Date { get; set; }
    }
}
