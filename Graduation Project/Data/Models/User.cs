using System;
using System.Collections.Generic;

namespace GraduationProject.Data.Models
{
    public class User
    {
        public string UserIdentityId { get; set; }
        public int UserId { get; set; }
        public int BirthDateYear { get; set; }  
        public string Country { get; set; }
        public string UserName { get; set; }
        public string PhotoUrl { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Gender { get; set; }
        public DateTime DateOfJoin { get; set; } = DateTime.Now;
        public virtual ICollection<UserContest> UserContest { get; set; } = new HashSet<UserContest>();
        public virtual ICollection<UserGroup> UserGroup { get; set; } = new HashSet<UserGroup>();
        public Handle Handle { get; set; }
        public UserStatistics UserStatistics { get; set; } = new();
        public CodeforcesStatistics CodeforcesStatistics { get; set; } = new();
        public AtcoderStatistics AtCoderStatistics { get; set; } = new();
        public virtual ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();
        public virtual ICollection<UserBlog> UserBlogs { get; set; } = new HashSet<UserBlog>();
        public virtual ICollection<CommentVote> UserComments { get; set; } = new HashSet<CommentVote>();
        public virtual ICollection<ProblemUser> UserProblems { get; set; } = new HashSet<ProblemUser>();

        public int TotalNumberOfSolvedProblems => UserStatistics.SolvedCount
                                                  + CodeforcesStatistics.SolvedCount
                                                  + AtCoderStatistics.SolvedCount;
    }
}
