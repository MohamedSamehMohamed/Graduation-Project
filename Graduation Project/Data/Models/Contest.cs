using System;
using System.Collections.Generic;

namespace GraduationProject.Data.Models
{
    public class Contest
    {
        public int ContestId { get; set; }
        public string ContestTitle { get; set; }
        public DateTime ContestStartTime { get; set; }
        public int ContestDuration { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public string ContestVisibility { get; set; }
        public virtual ICollection<UserContest> UserContest { get; set; } = new HashSet<UserContest>();
        public virtual ICollection<Submission> Submissions { get; set; } = new HashSet<Submission>();
        public virtual ICollection<ContestProblem> ContestProblems { get; set; } = new HashSet<ContestProblem>();
        public int? GroupId { get; set; }

        /*
         * boolean variable to determine if this contest in a group or not
         * if @GroupId is not null that means this contest in a group 
         */
        public bool InGroup => GroupId != null;

        public string Password { get; set; } = "";
        public int ContestStatus
        {
            get
            {
                // -1 UpComing 
                // 0 Running 
                // 1 Ended 
                if (ContestStartTime > DateTime.Now)
                    return -1;
                return ContestStartTime.AddMinutes(ContestDuration) < DateTime.Now ? 1 : 0;
            }
        }
    }
}
