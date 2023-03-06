using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraduationProject.Data.Models
{
    public class BlogTag
    {
        [Key, Column(Order = 0)]
        public int BlogId { get; set; }
        [Key, Column(Order = 1)]
        public int TagId { get; set; }
        public Blog Blog { get; set; }
        public Tag Tag { get; set; }
    }
}
