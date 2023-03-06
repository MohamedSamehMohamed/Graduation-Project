using System;

namespace GraduationProject.Data.Models
{
    public class AtcoderStatistics
    {
        public int AtcoderStatisticsId { get; set; }
        public int SolvedCount { get; set; }
        public DateTime LastCheckSubmission { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
