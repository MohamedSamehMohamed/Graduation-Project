using System.Collections.Generic;

namespace GraduationProject.Data.Models
{
    public class Tag
    {
        public int TagId { get; set; }
        public string TagName { get; set; }
        public virtual ICollection<ProblemTag> ProblemTag { get; set; } = new HashSet<ProblemTag>();
        public virtual ICollection<BlogTag> BlogTag { get; set; } = new HashSet<BlogTag>();
    }   
}
