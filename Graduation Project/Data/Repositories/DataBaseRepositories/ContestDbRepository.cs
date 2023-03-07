using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.ViewModels.ContestViewsModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class ContestDbRepository : IContestRepository<Contest>
    {
        private readonly EntitiesContext _dbContext;
        public ContestDbRepository(EntitiesContext dbContext)
        {
            _dbContext = dbContext;
            foreach(var item in _dbContext.Contests.ToList())
                LoadCurrentContest(item);
        }
    
        public Contest Add(Contest newContest)
        {
            _dbContext.Add(newContest);
            Commit();
            return newContest;
        }
        private static UserContest _createUserContest(int contestId, int userId, bool isRegistered,
                bool isFavourite, bool isOwner)
        {
            return new UserContest
            {
                ContestId = contestId,
                UserId = userId,
                IsRegistered = isRegistered,
                IsFavourite = isFavourite,
                IsOwner = isOwner
            };
        }
        public Contest CreateNewContest(int userId, Contest newContest)
        {
            var contest = new Contest
            {
                ContestDuration = newContest.ContestDuration,
                ContestStartTime = newContest.ContestStartTime,
                ContestTitle = newContest.ContestTitle,
                ContestVisibility = newContest.ContestVisibility,
                GroupId = newContest.GroupId,
                CreationTime = DateTime.Now
            };
            Add(contest); 
            contest.UserContest.Add( _createUserContest(contest.ContestId, userId, true, false, true) );
            contest.ContestProblems = newContest.ContestProblems; 
            Commit();
            return contest; 
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }

        public Contest Find(int id)
        {
            var contest = _dbContext.Contests.FirstOrDefault(contest => contest.ContestId == id);
            return contest;
        }

        public IList<Contest> List()
        {
            return _dbContext.Contests.ToList();
        }
        public IList<Contest> PublicContests()
        {
            return _dbContext.Contests.Where(u => u.ContestVisibility == "Public" 
                                                  && u.InGroup == false).ToList(); 
        }

        public void Remove(int id)
        {
            var contest = Find(id);
            if (contest == null) return;
            _dbContext.Contests.Remove(contest);
            Commit();
        }


        public void Update(Contest newContest)
        {
            var contest = Find(newContest.ContestId);
            contest.ContestDuration = newContest.ContestDuration;
            contest.ContestTitle = newContest.ContestTitle;
            contest.ContestVisibility = newContest.ContestVisibility;
            contest.ContestStartTime = newContest.ContestStartTime;
            contest.ContestProblems = newContest.ContestProblems; 
            Commit();
        }

        public void AddProblemToContest(int problemId, int contestId)
        {
            var contest = Find(contestId);
            var currentNumberOfProblems = contest.ContestProblems.Count;
            var problemOrder = currentNumberOfProblems + 1;
            if (contest.ContestProblems.FirstOrDefault(u => u.ProblemId == problemId) != null)
                return;
            contest.ContestProblems.Add(_createNewProblemRelation(contestId, problemId, problemOrder));
            Commit(); 
        }
        private static ContestProblem _createNewProblemRelation(int contestId, int problemId, int order)
        {
            return new ContestProblem {
                ContestId = contestId,
                ProblemId = problemId,
                Order = order
            };
        }
        public void RegisterInContest(int userId, int contestId)
        {
            var contest = Find(contestId);
            if (contest == null)
                return;
            var userContest = contest.UserContest.FirstOrDefault(u => u.UserId == userId);
            if (userContest == null)
            {
                userContest = _createUserContest(contestId, userId, true, false, false);
                contest.UserContest.Add(userContest);
            }
            else
            {
                userContest.IsRegistered = true;
            }
            Commit();
        }
        public void FlipFavourite(int contestId, int userId)
        {
            var currentUserContest = _getUserContestRole(contestId, userId);
            if (currentUserContest == null)
                return;
            currentUserContest.IsFavourite ^= true;
            Commit();
        }

        private UserContest _getUserContestRole(int contestId, int userId)
        {
            return Find(contestId).UserContest.FirstOrDefault(u => u.UserId == userId); 
        }

        private void LoadCurrentContest(Contest contest)
        {
            _dbContext.Entry(contest).Collection(c => c.ContestProblems).Load();
            _dbContext.Entry(contest).Collection(c => c.UserContest).Load();
            _dbContext.Entry(contest).Collection(c => c.Submissions).Load();
            foreach (var cp in contest.ContestProblems)
                _dbContext.Entry(cp).Reference(c => c.Problem).Load();
            foreach (var uc in contest.UserContest)
                _dbContext.Entry(uc).Reference(u => u.User).Load(); 
        }
        private static bool _isOwner(UserContest userContest, string name)
        {
            if (userContest == null) return false;
            return name == null || userContest.User.UserName.Contains(name);
        }
        private static string _getContestType(bool inGroup)
        {
            return inGroup ? "group" : "classical"; 
        }
        private static string _getContestStatus(int num)
        {
            return num switch
            {
                -1 => "scheduled",
                0 => "running",
                1 => "ended",
                _ => ""
            };
        }
        
        private static string _removeNull(string x)
        {
            x ??= "";
            return x;
        }
        private static string _changeToAll(string x)
        {
            x = _removeNull(x); 
            if (x.Contains("All")) x = "";
            return x; 
        }
        private static ContestFilter _fixFilterParameters(ContestFilter model)
        {
            model.ContestTitle = _changeToAll(model.ContestTitle);
            model.ContestStatus = _changeToAll(model.ContestStatus);
            model.ContestType =_changeToAll(model.ContestType);
            model.PreparedBy= _changeToAll(model.PreparedBy);
            model.ContestX = _changeToAll(model.ContestX);
            model.ContestPrivacy = _changeToAll(model.ContestPrivacy); 
            return model;
        }
        private static bool _isContestStatus(int contestStatusNum, string status)
        {
            // upcoming, running, ended 
            return _getContestStatus(contestStatusNum).Contains(status);
        }
        private static bool _isContestType(bool inGroup, string type)
        {
            if (type == "") return true;
            var currentType = _getContestType(inGroup);
            return currentType.Contains(type);
        }
        private static bool _isContestMe(Contest contest, string contestMe, bool isFav, 
            bool isOwner, bool hasSubmission, bool isParticipant)
        {
            if (contestMe == "") return true;
            return contestMe switch
            {
                "Contests" => hasSubmission,
                "Participation" => isParticipant,
                "Arrangement" => isOwner,
                "Favorites" => isFav,
                _ => false
            };
        }
        private static bool _isCurrentPrivacy(string contestPrivacy, string privacy)
        {
            return privacy == "" || contestPrivacy.Contains(privacy);
        }
        private static bool _isParticipant(Contest contest, int userId)
        {
            var dateDuringContest = contest.ContestStartTime.AddMinutes(contest.ContestDuration);
            var mySubmissions = contest.Submissions.
                FirstOrDefault(u => u.UserId == userId && u.CreationTime <= dateDuringContest);
            return mySubmissions != null; 
        }
        public IList<Contest> Filter(ContestFilter model)
        {
            model = _fixFilterParameters(model);
            var list = new List<Contest>();
            var userId = model.UserId; 
            foreach(var contest in _dbContext.Contests)
            {
                LoadCurrentContest(contest); 
                // see if user can see the contest 
                var role = _getUserContestRole(contest.ContestId, userId);
                if (role == null && contest.ContestVisibility == "Private")
                    continue;
                var isFav = role?.IsFavourite ?? false;
                var isOwner = role?.IsOwner ?? false;
                var hasSubmission = contest.Submissions.FirstOrDefault(u => u.UserId == userId) != null ? true : false;
                var isParticipant = _isParticipant(contest, userId); 
                if (!contest.ContestTitle.Contains(model.ContestTitle))
                    continue;
                if (!_isOwner(contest.UserContest.FirstOrDefault(u => u.IsOwner), model.PreparedBy))
                    continue;
                if (!_isContestStatus(contest.ContestStatus, model.ContestStatus))
                    continue;
                if (!_isContestType(contest.InGroup, model.ContestType))
                    continue;
                if (!_isContestMe(contest, model.ContestX, isFav, isOwner, hasSubmission, isParticipant))
                    continue;
                if (!_isCurrentPrivacy(contest.ContestVisibility, model.ContestPrivacy))
                    continue;
                list.Add(contest); 
            }
            return list; 
        }
        public int Submit(int userId, int contestId, int problemId, string code, string language)
        {
            var contest = Find(contestId);
            var newSubmission = _createNewSubmission(userId, contestId, problemId, code, language);
            contest.Submissions.Add(newSubmission);
            Commit(); 
            return newSubmission.SubmissionId;
        }
        private static Submission _createNewSubmission(int userId, int contestId, int problemId, string code, string lang)
        {
            return new Submission { 
                ContestId = contestId, 
                CreationTime = DateTime.Now, 
                ProblemId = problemId, 
                ProgrammingLanguage = lang, 
                SubmissionText = code, 
                UserId = userId, 
                Visible = false,
                Verdict = "Inqueue",
                MemoryConsumeBytes = "",
                TimeConsumeMillis = ""
                
            };
        }
        public bool IsOwner(int contestId, int userId)
        {
            var contest = Find(contestId);
            var rel = contest.UserContest.FirstOrDefault(u => u.IsOwner);
            return rel != null && rel.UserId == userId;
        }
    }
}
