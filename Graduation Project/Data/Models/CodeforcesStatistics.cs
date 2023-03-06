using System;

namespace GraduationProject.Data.Models
{
    public class CodeforcesStatistics
    {
        public int CodeforcesStatisticsId { get; set; }
        public int SolvedCount { get; set; }
        public DateTime LastCheckSubmission { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
