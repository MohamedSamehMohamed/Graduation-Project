using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GraduationProject.Data.API;
using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.ViewModels.ContestViewsModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GraduationProject.Controllers
{
    [Authorize]
    public class ContestController : Controller
    {
        private readonly IContestRepository<Contest> _contests;
        private readonly IProblemRepository<Problem> _problems;
        private readonly IGroupRepository<Data.Models.Group> _groups;
        private readonly ISubmissionRepository<Submission> _submissions;
        private readonly User _user;
        private const int NumberOfItemsForPage = 8;

        public ContestController(IContestRepository<Contest> contests
            , IUserRepository<User> userRepository
            , IHttpContextAccessor httpContextAccessor
            , IProblemRepository<Problem> problems
            , IGroupRepository<Data.Models.Group> groups
            , ISubmissionRepository<Submission> submissions
            )
        {
            var userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            _user = userRepository.Find(userId);
            _contests = contests;
            _problems = problems;
            _groups = groups;
            _submissions = submissions;
        }
        // GET: HomeController
        public ActionResult Index()
        {
            try
            {
                return View(_getPageItems(_getAllContests(), 1));
            }catch
            {
                return View("ErrorLink");
            }
        }
        private List<ViewContestModel> _getAllContests()
        {
            var list = new List<ViewContestModel>();
            foreach (var c in _contests.PublicContests())
            {
                if (CanAccessTheContest(c.ContestId, _user.UserId))
                    list.Add(_getContestViewModelFromContest(c));
            }
            return list; 
        }
        // GET: HomeController/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                if (!CanAccessTheContest(id, _user.UserId))
                    return RedirectToAction("Index");
                var contest = _contests.Find(id);
                var model = _getContestViewModelFromContest(contest);
                return View(model);
            }catch
            {
                return View("ErrorLink");
            }
        }

        // GET: HomeController/Create
        public ActionResult Create(int id)
        {
            try
            {
                var createContestView = CreateContestView();
                if (_groups.Find(id) != null)
                {
                    createContestView.CreateFromGroup = "1";
                    createContestView.groupId = id;
                }
                return View(createContestView);
            }catch
            {
                return View("Index");
            }
        }

        // POST: HomeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateContestModel model)
        {
            try
            {
                var contest = _createContestFromModel(model);
                _contests.CreateNewContest(_user.UserId, contest); 
                return !contest.InGroup ? RedirectToAction("Index") : 
                    RedirectToAction("Details", "Group", new { id = contest.GroupId });
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController/Edit/5
        public ActionResult Edit(int id)
        {
            try
            {
                if (!_contests.IsOwner(id, _user.UserId))
                    return RedirectToAction("Details", new { id });
                var contest = _contests.Find(id);
                return View(_getCreateContestModel(contest));
            }catch
            {
                return View("Index"); 
            }

        }

        // POST: HomeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CreateContestModel model)
        {
            if (!_contests.IsOwner(model.contestId, _user.UserId))
                return RedirectToAction("Index");
            var contest = _createContestFromModel(model);
            try
            {
                _contests.Update(contest);
                return contest.GroupId == null ? RedirectToAction("Index")
                        : RedirectToAction("Details", "Group", new { id = contest.GroupId });
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                if (!_contests.IsOwner(id, _user.UserId))
                    return RedirectToAction("Index");
                var contest = _contests.Find(id);
                return View(contest);
            }catch
            {
                return View("ErrorLink");
            }


        }

        // POST: HomeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(GraduationProject.Data.Models.Contest contest)
        {
            if (!_contests.IsOwner(contest.ContestId, _user.UserId))
                return RedirectToAction("Index");
            try
            {
                var groupId = _contests.Find(contest.ContestId).GroupId;
                _contests.Remove(contest.ContestId);
                return groupId == null ? RedirectToAction("Index"): 
                    RedirectToAction("Details", "Group", new { id = groupId }); 
            }
            catch
            {
                return View();
            }
        }
        public ActionResult RegisterInContest(int id)
        {
            try
            {
                _contests.RegisterInContest(_user.UserId, id);
                return RedirectToAction("Details", new { id });
            }catch
            {
                return View("ErrorLink");
            }
        }

        public ActionResult Standing(int id, int pageNumber)
        {
            try
            {
                if (!CanAccessTheContest(id, _user.UserId))
                    return RedirectToAction("Index");
                var model = CreateStandingView(id, pageNumber);
                return View(model);
            }catch
            {
                return View("ErrorLink");
            }
        }

        public ActionResult FlipFavourite(int id, ContestFilter filter)
        {
            return FlipFavourite(_user.UserId, id, filter);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FlipFavourite(int userId, int contestId, ContestFilter filter)
        {
            try 
            { 
                if (!CanAccessTheContest(contestId, _user.UserId))
                    return RedirectToAction("Filter", filter);
            
                _contests.FlipFavourite(contestId, userId);
                return RedirectToAction("Filter", filter);
            }
            catch
            {
                return RedirectToAction("Filter", filter);
            }
        }
        public ActionResult Filter()
        {
            try
            {
                var filter = new ContestFilter { PreparedBy = (string)TempData["PrepeardBy"] };
                return Filter(filter);
            }catch
            {
                return View("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Filter(ContestFilter model)
        {
            try
            {
                if (model.Reset == "Reset")
                    return RedirectToAction("Index");
                var list = new List<ViewContestModel>();
                model.UserId = _user.UserId;
                foreach (var c in _contests.Filter(model))
                    list.Add(_getContestViewModelFromContest(c));
                return View("Index", _getPageItems(list, 1));
            }catch
            {
                return View("Index"); 
            }
        }

        // This is For Problem 
        public ActionResult Status(int id, int pageNumber = 0)
        {
            try
            {
                if (!CanAccessTheContest(id, _user.UserId))
                    return RedirectToAction("Index");
                var contest = _contests.Find(id);
                var model = _getContestViewModelFromContest(contest);
                model.Submissions = _getPageItems(model.Submissions.
                    OrderByDescending(u => u.CreationTime).ToList(), pageNumber);
                return View("Details", model);
            }
            catch
            {
                return View("ErrorLink");
            }
        }
        
        public ActionResult MySubmission(int id, int pageNumber = 0)
        {
            try
            {
                if (!CanAccessTheContest(id, _user.UserId))
                    return RedirectToAction("Index");
                var contest = _contests.Find(id);
                var model = _getContestViewModelFromContest(contest);
                model.Submissions = _getPageItems(model.Submissions.
                    Where(u => u.UserId == _user.UserId).
                    OrderByDescending(u=>u.CreationTime).
                    ToList(), pageNumber); 
                return View("Details", model);
            }
            catch
            {
                return View("ErrorLink");
            }
        }
        
        public ActionResult DisplayProblem(int id, int problemId)
        {
            try
            {
                var contest = _contests.Find(id);
                var owner = contest.UserContest.FirstOrDefault(u => u.IsOwner);
                var ownerId = owner?.UserId;
                var alias = contest.ContestProblems.
                    FirstOrDefault(u => u.ProblemId == problemId)?.Alias;
                var problem = contest.ContestProblems.
                    FirstOrDefault(u => u.ProblemId == problemId)?.Problem;
                if (string.IsNullOrEmpty(alias)) 
                    alias = problem?.ProblemTitle;
                var model = _getDetailProblem(problem);
                model.contestDuration = contest.ContestDuration;
                model.contestId = contest.ContestId;
                model.contestStartTime = contest.ContestStartTime;
                model.groupId = contest.GroupId;
                model.IsCurrentUserOwner = (_user.UserId == ownerId);
                model.contestTitle = contest.ContestTitle;
                model.problemtitle = alias;
                return View("ProblemInContest", model);
            }
            catch 
            {
                return View("ErrorLink");
            }
        }
        // This Code Copied From Problem Controller vv
        private static ViewProblemDetailsInContest _getDetailProblem(Problem problem)
        {
            if (problem == null) return null;
            
            var model = new ViewProblemDetailsInContest()
            {
                problemId = problem.ProblemId,
                problemSource = problem.ProblemSource,
                problemsourceId = problem.ProblemSourceId,
                urlSource = problem.UrlSource,
                problemtitle = problem.ProblemTitle,
                Problemhtml = problem.ProblemInHtmlForm,
                Rating = problem.Rating,
                NumberAc = problem.Submissions.Count(p => p.Verdict == "Accepted"),
                Numbersubmission = problem.Submissions.Count
            };
            
            var tags = problem.ProblemTag.Select(item => item.Tag.TagName).ToList();
            model.problemTag = tags;
            
            return model;
        }
        private bool CanAccessTheContest(int contestId, int userId)
        {
            try
            {
                var c = _contests.Find(contestId);
                var rel = c.UserContest.FirstOrDefault(u => u.UserId == userId);
                return c.ContestVisibility == "Public" || rel != null;
            }catch
            {
                return false; 
            }
        }
        private ViewContestModel _getContestViewModelFromContest(Contest contest)
        {
            var contestStatus = contest.ContestStatus switch
            {
                -1 => "Upcoming",
                0 => "Running",
                1 => "Ended",
                _ => ""
            };
            var problems = new List<ProblemInfo>();
            foreach (var item in contest.ContestProblems.OrderBy(u=>u.Order).ToList())
            {
                var problemUrl = item.Problem.UrlSource; 
                var newProblem = new ProblemInfo
                {
                    ProblemId = item.ProblemId,
                    Origin = item.PlatForm,
                    OriginName = item.Problem.ProblemTitle,
                    PropblemTitle = string.IsNullOrEmpty(item.Alias)? item.Problem.ProblemTitle : item.Alias,
                    Solved = contest.Submissions.FirstOrDefault
                    (u => u.UserId == _user.UserId && 
                          u.ProblemId == item.ProblemId && 
                          u.Verdict == "Accepted") != null,
                    NumberOfAccepted = contest.Submissions.Count(u => u.UserId == _user.UserId &&
                                                                      u.ProblemId == item.ProblemId &&
                                                                      u.Verdict == "Accepted"),
                    NumberOfSubmissions = contest.Submissions.Count(u => u.ProblemId == item.ProblemId),
                    originUrl = problemUrl,
                    HasAttempt = contest.Submissions.FirstOrDefault
                        (u => u.UserId == _user.UserId && u.ProblemId == item.ProblemId) != null
                };
                problems.Add(newProblem); 
            }
            var owner = contest.UserContest.FirstOrDefault(u => u.IsOwner)?.User;
            var currentUser = contest.UserContest.FirstOrDefault(u => u.UserId == _user.UserId); 
            return new ViewContestModel
            {
                contestId = contest.ContestId,
                contestDuration = contest.ContestDuration,
                contestStartTime = contest.ContestStartTime,
                contestStatus = contestStatus,
                contestTitle = contest.ContestTitle,
                contestVisabilty = contest.ContestVisibility,
                UserContest = contest.UserContest,
                creationTime = contest.CreationTime,
                groupId = contest.GroupId,
                Problems = problems,
                Submissions = contest.Submissions.ToList(),
                IsFavourite = currentUser != null && currentUser.IsFavourite,
                PreparedBy = owner == null? "": owner.UserName, 
                PreparedById = owner?.UserId ?? -1,
                IsCurrentUserOwner = (owner != null && owner.UserId == _user.UserId),
                currentUserId = _user.UserId
            };
        }
        private StandingViewModel CreateStandingView(int contestId, int pageNumber)
        {
            var contest = _contests.Find(contestId);
            
            var contestOwnerId = contest.UserContest.FirstOrDefault(u => u.IsOwner)?.UserId;
            
            var numberOfUsers = contest.UserContest.
                Where(u => u.IsRegistered).
                Select(u => u.User).Count();
            
            var numberOfProblems = contest.ContestProblems.Count;
            
            var users = _buildStandingSubmissions(contest);
            users = _getPageItems(users, pageNumber); 
            
            var navInfo = new NavInfo {
                contestDuration = contest.ContestDuration,
                contestStartTime = contest.ContestStartTime,
                contestTitle = contest.ContestTitle,
                groupId = contest.GroupId,
                IsCurrentUserOwner = contestOwnerId == _user.UserId
            };

            return new StandingViewModel
            {
                contestId = contestId,
                NumberOfProblems = numberOfProblems,
                NumberOfUsers = numberOfUsers,
                users = users,
                NavInfo = navInfo    
            };
        }
        private static List<UserInStanding> _buildStandingSubmissions(Contest contest)
        {
            const int penaltyForWrongAnswer = 10;

            var users = _getUsersInStandingList(contest);

            // get submissions made in the contest when The contest is running 
            var submissions = contest.Submissions.ToList()
                .Where(u => u.CreationTime <= contest.ContestStartTime.AddMinutes(contest.ContestDuration))
                .OrderBy(u => u.CreationTime);

            var firstAcceptedInTheContestMade = false;
            foreach (var submission in submissions)
            {
                var currentUserId = submission.UserId;
                var currentProblemId = submission.ProblemId;
                var submissionVerdict = submission.Verdict;
                var currentUserIndex = users.IndexOf
                    (users.FirstOrDefault(u => u.userId == currentUserId));
                var problemInUserList =
                    users[currentUserIndex].UserPoblemsRaw.FirstOrDefault(u => u.problemId == currentProblemId);
                var currentProblemIndex = users[currentUserIndex].UserPoblemsRaw.IndexOf(problemInUserList);

                var countForProblemSubmissions =
                    users[currentUserIndex].UserPoblemsRaw[currentProblemIndex].NumberOfSubmissions;
                // if user solved this problem 
                if (users[currentUserIndex].UserPoblemsRaw[currentProblemIndex].Solved)
                    continue;
                users[currentUserIndex].UserPoblemsRaw[currentProblemIndex].NumberOfSubmissions++;
                users[currentUserIndex].UserPoblemsRaw[currentProblemIndex].Submissions.Add(submission);
                if (submissionVerdict != "Accepted") continue;
                users[currentUserIndex].NumberOfSolvedProblems++;
                users[currentUserIndex].UserPoblemsRaw[currentProblemIndex].problemPenality =
                    (int)submission.CreationTime.Subtract(contest.ContestStartTime).TotalMinutes
                    + countForProblemSubmissions * penaltyForWrongAnswer;
                users[currentUserIndex].UserPoblemsRaw[currentProblemIndex].Solved = true;
                if (firstAcceptedInTheContestMade) continue;
                firstAcceptedInTheContestMade = true;
                users[currentUserIndex].UserPoblemsRaw[currentProblemIndex].FirstAcceptedSubmission = true;
            }
            _addUsersRank(ref users);
            return users;
        }
        private static void _addUsersRank(ref List<UserInStanding> users)
        {
            users = users.OrderBy(u => -u.NumberOfSolvedProblems).ThenBy(u => u.TotalPenality).ToList();
            var rank = 1;
            for (var i = 0; i < users.Count; i++)
            {
                var j = i + 1;
                users[i].Rank = rank;
                while (j < users.Count &&
                       (users[i].NumberOfSolvedProblems == users[j].NumberOfSolvedProblems)
                       && (users[i].TotalPenality == users[j].TotalPenality)) {
                    users[j].Rank = rank;
                    j++;
                }
                rank++;
                i = j - 1;
            }
        }
        private static List<UserInStanding> _getUsersInStandingList(Contest contest)
        {
            var users = new List<UserInStanding>();
            var usersInContest = contest.UserContest.
                Where(u => u.IsRegistered).
                Select(u => u.User).ToList();
            var problemsInContest = contest.ContestProblems.ToList().OrderBy(u => u.Order).ToList();

            foreach (var u in usersInContest)
            {
                var emptyUserProblemsRaw = new List<GraduationProject.ViewModels.ContestViewsModel.Data>();
                foreach (var p in problemsInContest)
                {
                    emptyUserProblemsRaw.Add(new ViewModels.ContestViewsModel.Data
                    {
                        problemId = p.ProblemId,
                        Solved = false
                    });
                }
                users.Add(new UserInStanding
                {
                    userId = u.UserId,
                    userName = u.UserName,
                    UserPoblemsRaw = emptyUserProblemsRaw
                });
            }
            return users;
        }
        private CreateContestModel CreateContestView()
        {
            var myGroups = new List<GroupData>();
            // to select which group this contest will be in 
            foreach (var g in _user.UserGroup.Select(u => u.Group))
            {
                myGroups.Add(new GroupData { groupId = g.GroupId, groupTitle = g.GroupTitle });
            }
            return new CreateContestModel
            {
                groups = myGroups
            };
        }
        private Contest _createContestFromModel(CreateContestModel model)
        {
            var contestProblems = _getContestProblems(model.problems); 
            return new Contest
            {
                GroupId = model.CreateFromGroup == "0" ? null : model.groupId,
                ContestDuration = model.Duration,
                ContestStartTime = model.StartTime,
                ContestTitle = model.contestTitle,
                ContestVisibility = model.Visable == "1" ? "Public" : "Private",
                ContestProblems = contestProblems, 
                Password = model.Password,
                ContestId = model.contestId
            };
        }
        private List<ContestProblem> _getContestProblems(IList<ProblemData> modelproblems)
        {
            var list = new List<ContestProblem>();
            var order = 0; 
            foreach (var p in modelproblems)
            {
                var current = _problems.FindByName(p.PlatForm, p.problemId);
                
                if (current == null) continue;
                list.Add(
                new ContestProblem
                {
                    ProblemId = current.ProblemId,
                    Order = order++,
                    Alias = p.Alias,
                    PlatForm = p.PlatForm,
                    ProblemSourceId = p.problemId
                });
            }
            return list; 
        }

        private static CreateContestModel _getCreateContestModel(Contest contest)
        {
            var problems = new List<ProblemData>();
            foreach(var cp in contest.ContestProblems)
            {
                problems.Add(new ProblemData
                {
                    PlatForm = cp.PlatForm, 
                    Alias = cp.Alias, 
                    problemId = cp.ProblemSourceId
                }); 
            }
            return new CreateContestModel
            {
                Visable = contest.ContestVisibility,
                contestTitle = contest.ContestTitle,
                Duration = contest.ContestDuration,
                groupId = contest.GroupId ?? -1,
                problems = problems, 
                StartTime = contest.ContestStartTime,
                contestId = contest.ContestId
            };
        }
        private List<T> _getPageItems<T>(List<T> list, int pageNumber)
        {
            var totalPages = (list.Count + NumberOfItemsForPage - 1) / NumberOfItemsForPage;
            if (pageNumber < 1 || pageNumber > totalPages) pageNumber = 1;
            ViewBag.NumberOfPages = totalPages;
            ViewBag.PageNumber = pageNumber;
            var upperBound = pageNumber * NumberOfItemsForPage;
            // if list contains more than the upper bound limit we remove items from the end 
            if (list.Count > upperBound)
                list.RemoveRange(upperBound, list.Count - upperBound);
            // so here we fixed the end part of items If there is more than items 
            // it must be in the beginning of the list 
            if (list.Count > NumberOfItemsForPage)
                list.RemoveRange(0, list.Count - NumberOfItemsForPage);
            return list;
        }
        public ActionResult ContestPage(int pageNumber)
        {
            try
            {
                var list = _getPageItems(_getAllContests(), pageNumber);
                return View("Index", list);
            }catch
            {
                return View("Index"); 
            }
        }
        
        public ActionResult SubmitCode(int contestId, int problemId, string code, string lang)
        {
            try
            {
                var userId = _user.UserId;
                if (!CanAccessTheContest(contestId, userId))
                    return RedirectToAction("MySubmission", new { contestId });
                
                var problemSourceId = _problems.Find(problemId).ProblemSourceId; 
                
                var submissionId = _contests.Submit(userId, contestId, problemId, code, lang);
                APi.GetVerdict(problemSourceId, code, lang, submissionId); 
                RegisterInContest(contestId); 
                return RedirectToAction("MySubmission", new { id = contestId });
            }
            catch
            {
                return RedirectToAction("MySubmission", new { contestId });
            }
        }
        
        [HttpPost]
        public ActionResult GetTextSubmission(int submisionId)
        {
            var result = _submissions.Find(submisionId).SubmissionText;
            return Content(result, "text/plain");
        }
    }
}
