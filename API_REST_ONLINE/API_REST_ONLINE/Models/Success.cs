using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_REST_ONLINE.Models
{
    [Table("success")]
    public class Success
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(100)]
        public string name { get; set; }
        [Required]
        [StringLength(100)]
        public string description { get; set; }
        [Required]
        [StringLength(100)]
        public string imageurl { get; set; }
        public DateTime? timestamp { get; set; } // Nullable DateTime
    }
}
