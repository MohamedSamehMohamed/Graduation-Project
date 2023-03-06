using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GraduationProject.Data.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        [MaxLength(50)]
        public string Content { get; set; }
        public int Upvote { get; set; }
        public int DownVote { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public int BlogId { get; set; }
        public Blog Blog { get; set; }
        public virtual ICollection<CommentVote> CommentVotes { get; set; } = new HashSet<CommentVote>();
    }
}
