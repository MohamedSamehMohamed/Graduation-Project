using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.ViewModels.ProblemViewsModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace GraduationProject.Controllers
{
    public class InterviewController : Controller
    {
        private readonly IProblemRepository<Problem> _problemRepository;
        private readonly ISubmissionRepository<Submission> _submissionRepository;
        private readonly User _user;
        private readonly bool _login;
        private readonly IEnumerable<Submission> _listMySubmission;
        private readonly IEnumerable<ProblemUser> _listMyFavorite;
        private const int _pageSize = 25;
        public InterviewController(ISubmissionRepository<Submission> submissionRepository, 
            IUserRepository<User> userRepository, 
            IProblemRepository<Problem> problemRepository, 
            IHttpContextAccessor httpContextAccessor)
        {
            _submissionRepository = submissionRepository;
            _problemRepository = problemRepository;
            _login = false;
            try
            {
                var isLogin = httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
                if (!isLogin) return;
                _login = true;
                var userId = httpContextAccessor.HttpContext.User?.
                    FindFirst(ClaimTypes.NameIdentifier)
                    ?.Value;
                _user = userRepository.Find(userId);
                _listMySubmission = _user.Submissions;
                _listMyFavorite = _user.UserProblems;
            }
            catch
            {
                // ignored
            }
        }
        public ActionResult Index(int? page)
        {
            ViewBag.function = "Index";
            var pageNumber = page ?? 1;
            var listProblems = _problemRepository.Search(1, new List<string> { "2" });
            var model = GetAllModel(listProblems);

            var modelCount = model.Count;
            ViewBag.TotalPageProblem = (modelCount / _pageSize) + (modelCount % _pageSize == 0 ? 0 : 1);
            if (pageNumber < 0 || pageNumber > ViewBag.TotalPageProblem) pageNumber = 1;
            ViewBag.Pagenum = pageNumber;
            var list = model.ToPagedList(pageNumber, _pageSize);
            return View(list);
        }
        public bool CanSeeSubmission(int submissionId)
        {
            var submission = _submissionRepository.Find(submissionId);
            return submission.Visible || (_login && submission.UserId == _user.UserId);
        }
        public ActionResult Status(int? page)
        {
            var pageNumber = page ?? 1;
            ViewBag.function = "Status";
            var submissions = _submissionRepository.
                GetSpecificSubmission(
                    2, 
                    "", 
                    "", 
                    "", 
                    "", 
                    "", 
                    null).
                OrderByDescending(s => s.SubmissionId);
            var list = GetAllStatus(submissions);
            ViewBag.TotalPageProblem = (list.Count / _pageSize) + (list.Count % _pageSize == 0 ? 0 : 1);
            if (pageNumber < 0 || pageNumber > ViewBag.TotalPageProblem) pageNumber = 1;
            ViewBag.Pagenum = pageNumber;
            var newList = list.ToPagedList(pageNumber, _pageSize);
            return View(newList);
        }
        [HttpPost]
        public ActionResult GetTextSubmission(int submissionId)
        {
            var result = _submissionRepository.Find(submissionId).SubmissionText;
            return Content(result, "text/plain");
        }
        public ActionResult Filter(int? page, string problemId, string problemName, string problemSource)
        {
            var pageNumber = page ?? 1;
            ViewBag.problemid = problemId;
            ViewBag.problemname = problemName;
            ViewBag.Problemsource = problemSource;
            ViewBag.function = "Filter";
            var listProblems = _problemRepository.Search(
                2, 
                new List<string> { "2", problemId, problemName, problemSource });
            var model = GetAllModel(listProblems);
            ViewBag.TotalPageProblem = (model.Count / _pageSize) + (model.Count % _pageSize == 0 ? 0 : 1);
            if (pageNumber < 0 || pageNumber > ViewBag.TotalPageProblem) pageNumber = 1;
            ViewBag.Pagenum = pageNumber;
            var list = model.ToPagedList(pageNumber, _pageSize);
            return View("Index", list);
        }
        public ActionResult FilterStatus(int? page, 
            string userName, string problemName, string problemSource, 
            string problemResult, string problemLang)
        {
            var pageNumber = page ?? 1;
            ViewBag.function = "FilterStatus";
            userName ??= "";
            problemName ??= "";
            problemSource = ((problemSource == null || problemSource == "All") ? "" : problemSource);
            problemResult = ((problemResult == null || problemResult == "All") ? "" : problemResult);
            problemLang = ((problemLang == null || problemLang == "All") ? "" : problemLang);
            ViewBag.username = userName;
            ViewBag.problemName = problemName;
            ViewBag.problemSource = problemSource;
            ViewBag.problemResult = problemResult;
            ViewBag.problemLang = problemLang;

            var submissions = _submissionRepository.
                GetSpecificSubmission(2, userName, problemName, 
                    problemSource, problemResult, problemLang, 
                    null).OrderByDescending(s => s.SubmissionId);
            IEnumerable<ViewStatusModel> list = GetAllStatus(submissions);
            ViewBag.TotalPageProblem = (list.Count() / _pageSize) + (list.Count() % _pageSize == 0 ? 0 : 1);
            if (pageNumber < 0 || pageNumber > ViewBag.TotalPageProblem) pageNumber = 1;
            ViewBag.Pagenum = pageNumber;
            var model = list.ToPagedList(pageNumber, _pageSize);
            return View("Status", model);
        }

        public ActionResult FlipFavourite(int id)
        {
            var p = _problemRepository.Find(id);
            if (p == null)
            {
                return View("ErrorLink");
            }
            _flipFavorite(p);
            return RedirectToAction(nameof(Index));
        }

        public List<ViewProblemModel> GetAllModel(IList<Problem> listProblems)
        {
            var model = new List<ViewProblemModel>();
            foreach (var p in listProblems)
            {
                var item = new ViewProblemModel()
                {
                    ProblemId = p.ProblemId,
                    OnlineJudge = p.ProblemSource,
                    ProblemSourceId = p.ProblemSourceId,
                    Title = p.ProblemTitle,
                    rating = p.Rating,
                    UrlSource = p.UrlSource
                };
                if (_login)
                {
                    var acSubmission = _listMySubmission.
                        FirstOrDefault(s => s.ProblemId == p.ProblemId
                                            && s.Verdict == "Accepted");
                    if (acSubmission != null)
                    {
                        item.Status = "Solved";
                    }
                    else
                    {
                        var wrSubmission = _listMySubmission.
                            FirstOrDefault(s => s.ProblemId == p.ProblemId 
                                                && s.Verdict == "Wrong");
                        item.Status = wrSubmission != null ? "Attempted" : "";
                    }
                    var isFavorite = _listMyFavorite.
                        FirstOrDefault(f => f.IsFavourite && f.ProblemId == p.ProblemId);
                    item.Favorite = isFavorite != null;
                }
                else
                {
                    item.Favorite = false;
                    item.Status = "";
                }
                model.Add(item);
            }
            return model;
        }
        public IList<ViewStatusModel> GetAllStatus(IEnumerable<Submission> submissions)
        {
            IList<ViewStatusModel> list = new List<ViewStatusModel>();
            foreach (var item in submissions)
            {
                var temp = new ViewStatusModel
                {
                    RunID = item.SubmissionId,
                    UserId = item.User.UserId,
                    UserName = item.User.FirstName,
                    ProblemId = item.Problem.ProblemId,
                    OnlineJudge = item.Problem.ProblemSource,
                    ProblemSourcesId = item.Problem.ProblemSourceId,
                    Verdict = item.Verdict,
                    TimeConsumed = item.TimeConsumeMillis,
                    MemoryConsumed = item.MemoryConsumeBytes,
                    Language = item.ProgrammingLanguage,
                    SubmitTime = item.CreationTime,
                    contestId = item.ContestId
                };
                if (item.Visible || (_login && item.User.UserId == _user.UserId)) 
                    temp.Visiable = true;
                else 
                    item.Visible = false;
                list.Add(temp);
            }
            return list;
        }
        public ActionResult FlipFavouriteDetails(int id)
        {
            if (!_login) return View("ErrorLink");
            var problem = _problemRepository.Find(id);
            if (problem == null)
            {
                return View("ErrorLink");
            }
            _flipFavorite(problem);
            return RedirectToAction("Details", new { id });
        }
        public ActionResult Details(int id)
        {
            var problem = _problemRepository.Find(id);
            if (problem == null)
            {
                return View("~/Views/Shared/ErrorLink.cshtml");
            }
            var model = GetDetailProblem(problem);
            return View(model);
        }
        private ViewProblemDetails GetDetailProblem(Problem problem)
        {

            var model = new ViewProblemDetails()
            {
                problemId = problem.ProblemId,
                problemSource = problem.ProblemSource,
                problemsourceId = problem.ProblemSourceId,
                urlSource = problem.UrlSource,
                problemtitle = problem.ProblemTitle,
                Problemhtml = problem.ProblemInHtmlForm,
                Rating = problem.Rating,
                NumberAc = problem.Submissions.Count(p => p.Verdict == "Accepted"),
                Numbersubmission = problem.Submissions.Count()
            };
            if (_login)
            {
                var isFavorite = _listMyFavorite.
                    FirstOrDefault(f => f.IsFavourite && f.ProblemId == problem.ProblemId);
                model.IsFavorite = isFavorite != null;
            }
            var tags = new List<string>();

            foreach (var item in problem.ProblemTag)
            {
                tags.Add(item.Tag.TagName);
            }
            model.problemTag = tags;
            return model;
        }
        private void _flipFavorite(Problem newProblem)
        {
            var pu = new ProblemUser();
            var problemUser = newProblem.ProblemUsers.
                FirstOrDefault(u => u.UserId == _user.UserId);
            if (problemUser == null)
            {
                pu.ProblemId = newProblem.ProblemId;
                pu.UserId = _user.UserId;
                pu.IsFavourite = true;
                newProblem.ProblemUsers.Add(pu);
            }
            else
            {
                problemUser.IsFavourite ^= true;
            }
            _problemRepository.Update(newProblem);
        }
    }
}
