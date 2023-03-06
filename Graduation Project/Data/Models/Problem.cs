using System.Collections.Generic;

namespace GraduationProject.Data.Models
{
    public class Problem
    {
        public int ProblemId { get; set; }
        public string ProblemSource { get; set; }
        public string ProblemSourceId { get; set; }
        public string UrlSource { get; set; }
        public string ProblemTitle { get; set; }
        //interview ==1 or contest==2
        public int ProblemType { get; set; }
        public string ProblemInHtmlForm { get; set; }
        public int? Rating { get; set; }
        public virtual ICollection<ProblemTag> ProblemTag { get; set; } = new HashSet<ProblemTag>();
        public virtual ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();
        public virtual ICollection<ProblemUser> ProblemUsers { get; set; } = new HashSet<ProblemUser>();
    }
}