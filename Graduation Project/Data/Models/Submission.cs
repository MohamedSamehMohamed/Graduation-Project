﻿using GraduationProject.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Data.Models
{
    public class Submission
    {
        public int SubmissionId { get; set; }
        public float MemoryConsumeBytes { get; set; }
        public float TimeConsumeMillis { get; set; }
        public Boolean Visable { get; set; }
        public DateTime CreationTime { get; set; }
        public string Verdict { get; set; }
        public string ProgrammingLanguage { get; set; }
        public string SubmissionText { get; set; }
        public int userId { get; set; }
        public User user { get; set; }
        public int? contestId { get; set; }
        public Contest? contest { get; set; }
        public Boolean InContest { get; set; } // True -> submission in contest
        public int ProblemId { get; set; }
        public Problem problem { get; set; }

    }
}
