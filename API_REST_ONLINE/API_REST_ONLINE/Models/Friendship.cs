using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_REST_ONLINE.Models
{
    [Table("friendship")]
    public class Friendship
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int friendshipid { get; set; }

        [Required]
        public Guid userid1 { get; set; }

        [Required]
        public Guid userid2 { get; set; }

    }
}
