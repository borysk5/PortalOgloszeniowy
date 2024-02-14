using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PortalOgloszeniowy.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        [DisplayName("Tytuł")]
        public string Title { get; set; }
        [DisplayName("Tekst")]
        public string Text { get; set; }
        [Required]
        [DisplayName("Łapki w górę")]
        public int UpVotes { get; set; }
        [Required]
        [DisplayName("Data utworzenia")]
        public DateTime Date { get; set; }
        [DisplayName("Tagi")]
        public virtual IEnumerable<Tag> Tags { get; set; }
        [DisplayName("Komentarze")]
        public virtual IEnumerable<Comment> Comments { get; set; }
    }
}