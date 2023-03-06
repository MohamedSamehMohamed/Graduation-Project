using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Data.Models
{
    public class UserContest
    {
        [Key, Column(Order = 0)]
        public int UserId { get; set; }
        [Key, Column(Order = 1)]
        public int ContestId { get; set; }
        public bool IsFavourite { get; set; }
        public bool IsOwner { get; set; }
        public bool IsRegistered { get; set; }
        public virtual User User { get; set; }
        public virtual Contest Contest { get; set; }
    }
}
