using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PortalOgloszeniowy.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Data")]
        public DateTime Date { get; set; }
        [DisplayName("Tekst")]
        public string Text { get; set; }
    }
}