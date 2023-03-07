using System.Collections.Generic;
using System.Linq;
using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class SubmissionDbRepository : ISubmissionRepository<Submission>
    {
        private readonly EntitiesContext _dbContext;
        public SubmissionDbRepository(EntitiesContext dbContext)
        {
            _dbContext = dbContext;
        }
        public Submission Add(Submission newSubmission)
        {
            _dbContext.Submissions.Add(newSubmission);
            Commit();
            return newSubmission;
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }

        public Submission Find(int id)
        {
            var submission = List().FirstOrDefault(innerSubmission => innerSubmission.SubmissionId == id);
            return submission;
        }

        public IList<Submission> List()
        {
            return _dbContext.Submissions
                .Include(p => p.Problem)
                .Include(p => p.User)
                .ToList();
        }

        public void Remove(int id)
        {
            var submission = Find(id);
            if (submission == null) return;
            _dbContext.Submissions.Remove(submission);
            Commit();
        }
        public void Update(Submission newSubmission)
        {
            _dbContext.Submissions.Update(newSubmission);
            Commit();
        }

        public IList<Submission> GetSpecificSubmission(int problemType, string userName, 
            string problemName, string problemSource, string problemResult, string problemLanguage, int? contestId)
        {
            return _dbContext.Submissions
               .Include(p => p.Problem)
               .Include(p => p.User)
               .Where(s =>
                s.Problem.ProblemType == problemType &&
                (userName==""? s.User.UserName.Contains(userName) : s.User.UserName == userName) &&
                s.Problem.ProblemSourceId.Contains(problemName) &&
                s.Problem.ProblemSource.Contains(problemSource) &&
                s.Verdict.Contains(problemResult) &&
                s.ProgrammingLanguage.Contains(problemLanguage) &&
                s.ContestId == contestId
               ).ToList();
        }

        public IList<Submission> FindUserSubmissions(int id)
        {
            return List().Where(p => p.UserId == id).ToList();
        }
    }
}
