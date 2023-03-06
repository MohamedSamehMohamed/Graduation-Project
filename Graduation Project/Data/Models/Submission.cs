using System;

namespace GraduationProject.Data.Models
{
    public class Submission
    {
        public int SubmissionId { get; set; }
        public string MemoryConsumeBytes { get; set; }
        public string TimeConsumeMillis { get; set; }
        public bool Visible { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public string Verdict { get; set; }
        public string ProgrammingLanguage { get; set; }
        public string SubmissionText { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int? ContestId { get; set; }
        public Contest Contest { get; set; }
        public int ProblemId { get; set; }
        public Problem Problem { get; set; }
    }
}
