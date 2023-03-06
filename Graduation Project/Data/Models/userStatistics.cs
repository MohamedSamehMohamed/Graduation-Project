using System;

namespace GraduationProject.Data.Models
{
    public class UserStatistics
    {
        public int UserStatisticsId { get; set; }
        public int SolvedCount { get; set; }
        // used to not check for user submission a lot of time [may be check every 1 hour]
        public DateTime LastCheckSubmission { get; set; } = new();
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
