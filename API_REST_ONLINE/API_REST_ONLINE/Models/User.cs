using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace API_REST_ONLINE.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid id { get; set; }

        [Required]
        [StringLength(100)]
        public string username { get; set; }

        [Required]
        [StringLength(100)]
        public string email { get; set; }

        [Required]
        [StringLength(100)]
        public string password { get; set; }

        [Required]
        [StringLength(100)]
        public string salt { get; set; }

        [ForeignKey("RoleId")]
        public int roleid { get; set; }

        [ForeignKey("PendingInvite")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pendinginviteid { get; set; }

        public double killdeathratio { get; set; }

        public string? lastmapplayed { get; set; }

        [ForeignKey("RankId")]
        public int rankid { get; set; }

        public List<Success> achievements { get; set; }
    }
}
