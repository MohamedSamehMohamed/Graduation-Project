using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.ViewModels.ProblemViewsModel;
using GraduationProject.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using X.PagedList;

namespace GraduationProject.Controllers
{

    public class UserController : Controller
    {
        private readonly IUserRepository<User> _users;
        private readonly IBlogRepository<Blog> _blogs;
        private readonly IGroupRepository<Group> _groups;
        private readonly  IContestRepository<Contest> _contests;
        private readonly IEnumerable<Submission> _listMySubmission;
        private readonly ISubmissionRepository<Submission> _submissionRepository;
        private readonly bool _login;
        private readonly IProblemRepository<Problem> _problemRepository;
        private readonly User _user;
        public UserController(IBlogRepository<Blog> blogs,
            IGroupRepository<Group> groups, 
            IContestRepository<Contest> contests, 
            IProblemRepository<Problem> problemRepository, 
            ISubmissionRepository<Submission> submissionRepository, 
            IUserRepository<User> users, 
            IHttpContextAccessor httpContextAccessor)
        {
            _problemRepository = problemRepository;
            _contests = contests;
            _groups = groups;
            _users = users;
            _blogs = blogs;
            var isAuthenticated = httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated;
            if (isAuthenticated == true)
            {
                _submissionRepository = submissionRepository;
                _login = true;
                var userId = httpContextAccessor.HttpContext?.User.
                    FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _user = users.Find(userId);
                users.Find(_user.UserId);
                _listMySubmission = _user.Submissions;
            }
            else
            {
                _login = false;
            }

        }

        // GET: UserController
        public ActionResult Index()
        {
            return View();
        }

        [Route("User/Details/{id}")]
        public ActionResult Details(int id)
        {
            var currentUser = _users.Find(id);
            if (_login)
                TempData["userIdentity"] = _user.UserIdentityId;
            return View(currentUser);
        }
        [Authorize]
        [Route("User/MyProfile")]
        public ActionResult Details()
        {
            var currentUser = _users.Find(_user.UserId);
            TempData["userIdentity"] = _user.UserIdentityId;
            return View(currentUser);
        }
        public ActionResult OpenContest(int id)
        {
            var currentUser = _users.Find(id);
            TempData["PrepeardBy"] = currentUser.UserName;
            return RedirectToAction("filter", "Contest");
        }
        public ActionResult OpenBlog()
        {
            TempData["BlogsByUser"] = "UserBlogs";
            return RedirectToAction("Index", "Blog");
        }
        [Authorize]
        public ActionResult MySubmission(int id, int? page)
        {
            if (_user.UserId != id) return View("ErrorLink");
            var pageNumber = page ?? 1;
            ViewBag.USER = _user;
            var list = GetAllSubmission(id).
                OrderByDescending(s => s.RunID).ToList();
            const int pageSize = 25;
            ViewBag.TotalPageProblem = (list.Count / pageSize) + (list.Count % pageSize == 0 ? 0 : 1);
            ViewBag.Pagenum = pageNumber;
            var newList = list.ToPagedList(pageNumber, pageSize);
            return View(newList);
        }

        [Authorize]
        public ActionResult Favorite(int id)
        {
            if (_user.UserId != id) return View("ErrorLink");
            ViewBag.USER = _user;
            var favouriteContest = _user.UserContest.
                Where(c => c.IsFavourite).ToList();
            var favouriteGroup = _user.UserGroup.
                Where(g => g.IsFavourite).ToList();
            var favouriteBlog = _user.UserBlogs.
                Where(b => b.IsFavourite).ToList();

            var listProblemUser = _user.UserProblems.
                Where(pu => pu.IsFavourite == true);
            var favouriteProblem = GetAllModel(listProblemUser);
            var model = new FavoriteViewModel()
            {
                pu = favouriteProblem,
                uc = favouriteContest,
                ug = favouriteGroup,
                ub = favouriteBlog
            };
            return View(model);
        }
        public void FlipSubmissionVisibilityStatus(int submissionId)
        {
            var submission = _submissionRepository.Find(submissionId);
            if (submission == null || !_login || _user.UserId != submission.UserId) return;
            submission.Visible ^= true;
            _submissionRepository.Update(submission);
        }
        private IList<ViewStatusModel> GetAllSubmission(int id)
        {
            var allSubmission = _submissionRepository.FindUserSubmissions(id);
            var list = new List<ViewStatusModel>();
            foreach (var item in allSubmission)
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
                    SubmitTime = item.CreationTime
                };
                if (item.Visible) 
                    temp.Visiable = true;
                else 
                    item.Visible = false;
                list.Add(temp);
            }
            return list;
        }
        public IEnumerable<ViewProblemModel> GetAllModel(IEnumerable<ProblemUser> list)
        {
            var model = new List<ViewProblemModel>();
            foreach (var p in list)
            {
                var item = new ViewProblemModel()
                {
                    ProblemId = p.ProblemId,
                    OnlineJudge = p.Problem.ProblemSource,
                    ProblemSourceId = p.Problem.ProblemSourceId,
                    Title = p.Problem.ProblemTitle,
                    rating = p.Problem.Rating,
                    UrlSource = p.Problem.UrlSource,
                    Favorite = p.IsFavourite
                };
                var acsubmission = _listMySubmission.FirstOrDefault(s => s.ProblemId == p.ProblemId && s.Verdict == "Accepted");
                if (acsubmission != null)
                {
                    item.Status = "Solved";
                }
                else
                {
                    var wrsubmission = _listMySubmission.FirstOrDefault(s => s.ProblemId == p.ProblemId && s.Verdict == "Wrong");
                    if (wrsubmission != null)
                        item.Status = "Attempted";
                    else
                        item.Status = "";
                }
                model.Add(item);
            }
            return model;
        }
        [Authorize]
        public ActionResult FlipFavouriteProblem(int id, int uid)
        {
            if (uid != _user.UserId)
                return View("~/Views/Shared/ErrorLink.cshtml");
            var p = _problemRepository.Find(id);
            if (p == null)
            {
                return View("~/Views/Shared/ErrorLink.cshtml");
            }
            var problemuser = p.ProblemUsers.FirstOrDefault(u => u.UserId == _user.UserId);
            problemuser.IsFavourite ^= true;
            _problemRepository.Update(p);
            return RedirectToAction(nameof(Favorite), new { id = _user.UserId });
        }
        [Authorize]
        public ActionResult FlipFavouritecontest(int id, int uid)
        {
            if (uid != _user.UserId)
                return View("~/Views/Shared/ErrorLink.cshtml");
            var c = _contests.Find(id);
            if (c == null)
            {
                return View("~/Views/Shared/ErrorLink.cshtml");
            }
            _contests.FlipFavourite(id, uid);
            return RedirectToAction(nameof(Favorite), new { id = _user.UserId });
        }
        [Authorize]
        public ActionResult FlipFavouritegroup(int id, int uid)
        {
            if (uid != _user.UserId)
                return View("~/Views/Shared/ErrorLink.cshtml");
            var g = _groups.Find(id);
            if (g == null)
            {
                return View("~/Views/Shared/ErrorLink.cshtml");
            }
            _groups.FlipFavourite(id, uid);
            return RedirectToAction(nameof(Favorite), new { id = _user.UserId });
        }
        [Authorize]
        public ActionResult FlipFavouriteblog(int id, int uid)
        {
            if (uid != _user.UserId)
                return View("~/Views/Shared/ErrorLink.cshtml");
            var b = _blogs.Find(id);
            if (b == null)
            {
                return View("~/Views/Shared/ErrorLink.cshtml");
            }
            _blogs.UpdateFavourite(id, uid);
            return RedirectToAction(nameof(Favorite), new { id = _user.UserId });
        }
    }
}
