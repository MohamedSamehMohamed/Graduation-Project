namespace GraduationProject.Data.Models
{
    public class Handle
    {
        public int HandleId { get; set; }
        public string CodeforcesHandle { get; set; }
        public string AtCoderHandle { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
