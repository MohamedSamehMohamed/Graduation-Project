using System;
using System.Collections.Generic;

namespace GraduationProject.Data.Models
{
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupTitle { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public string GroupDescription { get; set; }
        public bool Visible { get; set; }
        public string Password { get; set; }
        public virtual ICollection<UserGroup> UserGroup { get; set; } = new HashSet<UserGroup>();
        public virtual ICollection<Blog> Blogs { get; set; } = new HashSet<Blog>();
        public virtual ICollection<Contest> Contests { get; set; } = new HashSet<Contest>();
    }
}
