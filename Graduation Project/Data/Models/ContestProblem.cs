namespace GraduationProject.Data.Models
{
    public class ContestProblem
    {
        public int ContestId { get; set; }
        public int ProblemId { get; set; }
        // order the problem in contest
        public int Order { get; set; } 
        // the problem source 
        public string PlatForm { get; set; } = "";
        public string Alias { get; set; } = "";
        public string ProblemSourceId { get; set; } = ""; 
        public Contest Contest { get; set; }
        public Problem  Problem { get; set; }
    }
}
