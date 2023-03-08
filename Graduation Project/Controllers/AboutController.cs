using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    public class AboutController : Controller
    {
        private readonly ISubmissionRepository<Submission> _submissions;
        public AboutController(ISubmissionRepository<Submission> submissions)
        {
            _submissions = submissions; 
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
        public ActionResult GetVerdict(int submissionId, string memory, string time, string verdict)
        {
            // check some thing 
            var current = _submissions.Find(submissionId);
            if (current == null) return View("ErrorLink", "Thank You");
            current.Verdict = verdict;
            current.MemoryConsumeBytes = memory;
            current.TimeConsumeMillis = time;
            _submissions.Update(current);
            return View("ErrorLink", "Thank You");
        }
    }
}
