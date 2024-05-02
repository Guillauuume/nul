using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_REST_ONLINE.Models
{
    [Table("pending_invites")]
    public class PendingInvite
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [ForeignKey("Inviter")]
        public Guid inviterid { get; set; }

        [ForeignKey("Invitee")]
        public Guid inviteeid { get; set; }

        public string invitername { get; set; }
    }
}
