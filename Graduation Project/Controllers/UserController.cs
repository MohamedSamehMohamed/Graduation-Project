using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.Data.Repositories.IProblemRepository;
using GraduationProject.ViewModels.ContestViewsModel;
using GraduationProject.ViewModels.ProblemViewsModel;
using GraduationProject.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using X.PagedList;

namespace GraduationProject.Controllers
{

    public class UserController : Controller
    {
        private readonly IUserRepository<User> users;
        private readonly IBlogRepository<Data.Models.Blog> blogs;
        private readonly IGroupRepository<GraduationProject.Data.Models.Group> groups;
        readonly private IContestRepository<GraduationProject.Data.Models.Contest> contests;
        private readonly IEnumerable<Submission> ListMysubmission;
        private readonly IEnumerable<ProblemUser> ListMyfavorite;
        private readonly ISubmissionRepository<Submission> SubmissionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Boolean login;
        private readonly IProblemRepository<Problem> problemRepository;
        private User user;
        public UserController(IBlogRepository<GraduationProject.Data.Models.Blog> blogs,IGroupRepository<GraduationProject.Data.Models.Group> groups, IContestRepository<GraduationProject.Data.Models.Contest> contests, IProblemRepository<Problem> problemRepository, ISubmissionRepository<Submission> _SubmissionRepository, IUserRepository<User> users, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            this.problemRepository = problemRepository;
            this.contests = contests;
            this.groups = groups;
            this.users = users;
            this.blogs = blogs;
            var flag = _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
            if (flag == true)
            {
                SubmissionRepository = _SubmissionRepository;
                login = true;
                _httpContextAccessor = httpContextAccessor;
                var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                user = users.Find(userId);
                users.Find(user.UserId);
                ListMysubmission = user.Submissions;
                ListMyfavorite = user.UserProblems;
            }
            else
            {
                login = false;
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
            var currentuser = users.Find(id);
            if (login == true)
                TempData["userIdentity"] = user.UserIdentityId;
            return View(currentuser);
        }
        [Authorize]
        [Route("User/MyProfile")]
        public ActionResult Details()
        {
            var currentuser = users.Find(user.UserId);
            TempData["userIdentity"] = user.UserIdentityId;
            return View(currentuser);
        }
        public ActionResult OpenContest(int id)
        {
            var currentuser = users.Find(id);
            TempData["PrepeardBy"] = currentuser.UserName;
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
            if (user.UserId == id)
            {
                int pagenumber = page ?? 1;
                ViewBag.USER = user;
                var list = GetAllSubmission(id).OrderByDescending(s => s.RunID);
                ViewBag.TotalPageProblem = (list.Count() / 25) + (list.Count() % 25 == 0 ? 0 : 1);
                ViewBag.Pagenum = pagenumber;
                var newlist = list.ToPagedList(pagenumber, 25);
                return View(newlist);
            }
            else
            {
                return View("~/Views/Shared/ErrorLink.cshtml");
            }
        }

        [Authorize]
        public ActionResult Favorite(int id)
        {
            if (user.UserId == id)
            {
                ViewBag.USER = user;
                var Favouritecontest = user.UserContest.Where(c => c.IsFavourite == true).ToList();
                var Favouritegroup = user.UserGroup.Where(g => g.IsFavourite == true).ToList();
                var Favouriteblog = user.UserBlogs.Where(b => b.IsFavourite == true).ToList();

                var listproblemuser = user.UserProblems.Where(pu => pu.IsFavourite == true);
                var FavouriteProblem = getAllmodel(listproblemuser);
                FavoriteViewModel model = new FavoriteViewModel()
                {
                    pu = FavouriteProblem,
                    uc = Favouritecontest,
                    ug = Favouritegroup,
                    ub = Favouriteblog

                };


                return View(model);
            }
            else
            {
                return View("~/Views/Shared/ErrorLink.cshtml");
            }
        }
        public void FlibShare(int SubmisionId)
        {
            Submission submission = SubmissionRepository.Find(SubmisionId);
            if (submission != null && login && user.UserId == submission.UserId)
            {
                submission.Visible ^= true;
                SubmissionRepository.Update(submission);
            }
        }
        private IList<ViewStatusModel> GetAllSubmission(int id)
        {
            var allSubmission = SubmissionRepository.FindSubmissionUser(id);
            IList<ViewStatusModel> list = new List<ViewStatusModel>();
            foreach (var item in allSubmission)
            {
                var tmp = new ViewStatusModel
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
                if (item.Visible == true) tmp.Visiable = true;
                else item.Visible = false;
                list.Add(tmp);
            }
            return list;
        }
        // GET: UserController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UserController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UserController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public IEnumerable<ViewProblemModel> getAllmodel(IEnumerable<ProblemUser> List)
        {
            List<ViewProblemModel> model = new List<ViewProblemModel>();
            foreach (var p in List)
            {
                ViewProblemModel item = new ViewProblemModel()
                {
                    ProblemId = p.ProblemId,
                    OnlineJudge = p.Problem.ProblemSource,
                    ProblemSourceId = p.Problem.ProblemSourceId,
                    Title = p.Problem.ProblemTitle,
                    rating = p.Problem.Rating,
                    UrlSource = p.Problem.UrlSource,
                    Favorite = p.IsFavourite
                };
                var acsubmission = ListMysubmission.FirstOrDefault(s => s.ProblemId == p.ProblemId && s.Verdict == "Accepted");
                if (acsubmission != null)
                {
                    item.Status = "Solved";
                }
                else
                {
                    var wrsubmission = ListMysubmission.FirstOrDefault(s => s.ProblemId == p.ProblemId && s.Verdict == "Wrong");
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
            if (uid != user.UserId)
                return View("~/Views/Shared/ErrorLink.cshtml");
            var p = problemRepository.Find(id);
            if (p == null)
            {
                return View("~/Views/Shared/ErrorLink.cshtml");
            }
            var problemuser = p.ProblemUsers.FirstOrDefault(u => u.UserId == user.UserId);
            problemuser.IsFavourite ^= true;
            problemRepository.Update(p);
            return RedirectToAction(nameof(Favorite), new { id = user.UserId });
        }
        [Authorize]
        public ActionResult FlipFavouritecontest(int id, int uid)
        {
            if (uid != user.UserId)
                return View("~/Views/Shared/ErrorLink.cshtml");
            var c = contests.Find(id);
            if (c == null)
            {
                return View("~/Views/Shared/ErrorLink.cshtml");
            }
            contests.FlipFavourite(id, uid);
            return RedirectToAction(nameof(Favorite), new { id = user.UserId });
        }
        [Authorize]
        public ActionResult FlipFavouritegroup(int id, int uid)
        {
            if (uid != user.UserId)
                return View("~/Views/Shared/ErrorLink.cshtml");
            var g = groups.Find(id);
            if (g == null)
            {
                return View("~/Views/Shared/ErrorLink.cshtml");
            }
            groups.FlipFavourite(id, uid);
            return RedirectToAction(nameof(Favorite), new { id = user.UserId });
        }
        [Authorize]
        public ActionResult FlipFavouriteblog(int id, int uid)
        {
            if (uid != user.UserId)
                return View("~/Views/Shared/ErrorLink.cshtml");
            var b = blogs.Find(id);
            if (b == null)
            {
                return View("~/Views/Shared/ErrorLink.cshtml");
            }
            blogs.UpdateFavourite(id, uid);
            return RedirectToAction(nameof(Favorite), new { id = user.UserId });
        }
    }
}
