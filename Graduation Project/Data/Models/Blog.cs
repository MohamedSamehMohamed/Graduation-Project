using System;
using System.Collections.Generic;

namespace GraduationProject.Data.Models
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string BlogTitle { get; set; }
        public string BlogContent { get; set; }
        public int BlogVote { get; set; }
        public DateTime CreationTime { get; } = DateTime.Now;
        public bool BlogVisibility { get; set; }
        public virtual ICollection<UserBlog> UserBlog { get; set; } = new HashSet<UserBlog>();
        public virtual ICollection<BlogTag> BlogTag { get; set; } = new HashSet<BlogTag>();
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
        public int? GroupId { get; set; } 
        public Group Group { get; set; }
    }
}