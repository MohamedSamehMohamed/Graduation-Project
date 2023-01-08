using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class AboutController : Controller
    {
        private readonly ISubmissionRepository<Submission> Submissions;
        public AboutController(ISubmissionRepository<Submission> Submissions)
        {
            this.Submissions = Submissions; 
        }
        // GET: HomeController
        public ActionResult Index()
        {
            return View();
        }
        
        /*
         * The Script that get the verdict of the submission will access this method ,
         * with get request sending the verdict information, 
         */
        public ActionResult GetVerdict(int SubmissionId, string Memory, string Time, string Verdict)
        {
            // check some thing 
            var current = Submissions.Find(SubmissionId);
            if (current != null)
            {
                current.Verdict = Verdict;
                current.MemoryConsumeBytes = Memory;
                current.TimeConsumeMillis = Time;
                Submissions.Update(current);
            }
            return View("ErrorLink", "Thank You");
        }
    }
}
