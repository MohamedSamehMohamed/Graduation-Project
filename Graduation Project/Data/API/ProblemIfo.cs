using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Data.API
{
    public class ProblemIfo
    {
        public string Title { get; set; }
        public string Source { get; set; }
        public string ProblemId { get; set; }
        public int Rate { get; set; }
        public IList<string> Tags { get; set; }
        public string Problem { get; set; }
    }
}
