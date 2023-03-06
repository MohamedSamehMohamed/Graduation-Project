using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Data.Models
{
    public class UserGroup
    {
        [Key, Column(Order = 0)]
        public int UserId { get; set; }
        [Key, Column(Order = 1)]
        public int GroupId { get; set; }
        //Creator, Manager, Participant
        public string UserRole { get; set; }
        public DateTime MemberSince { get; set; } = DateTime.Now;
        public bool IsFavourite { get; set; }
        public virtual User User { get; set; }
        public virtual Group Group { get; set; }
    }
}
