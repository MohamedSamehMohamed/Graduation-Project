using System;
using System.Linq;
using GraduationProject.Data.API;
using Microsoft.Extensions.DependencyInjection;

namespace GraduationProject.Data.Models
{
    public class DbInitializer
    {
        private static EntitiesContext _dbContext;
        public static void Seed(IServiceProvider serviceProvider)
        {
            
            var rand = new Random();
            _dbContext =
                serviceProvider.GetRequiredService<EntitiesContext>();
            
            var d = DateTime.Now;

            
            var code = "#include<iostream>;" +
                "using namespace std;" +
                "int main(){" +
                "int n;" +
                "cin >> n;" +
                "for(int i = 0; i < n; i++)" +
                "{" +
                "int x;" +
                "cin >> x;" +
                "if (x > 3)" +
                "   cout << \"Hello From the other hand\";" +
                "else " +
                "cout << \"Hello From The hand\";" +
                "}" +
                "}";

            string[] verdicts = { "Accepted", "Wrong Answer", "Time limit exceeded" };
            var users = _dbContext.Users;
            var problems = _dbContext.Problems;
                foreach(var p in problems)
                {
                    foreach(var u in users)
                    {
                        var lim = 1 + rand.Next(3); 
                        for (var i = 0; i < lim; i++)
                        {
                            AddSubmission(11, p.ProblemId, u.UserId, verdicts[i], code);
                        }
                    }
                }
            _dbContext.SaveChanges();
        }
        private static void GetAllProblem()
        {
            for (int i = 33; i <= 60; i++)
            {
                for (char ca = 'a'; ca <= 'd'; ca++)
                {
                    var ProblemSourceId = i.ToString() + ca;
                    var OnlineJudge = "CodeForces";
                    var problem = _dbContext.Problems.FirstOrDefault(u => u.ProblemSourceId == ProblemSourceId && u.ProblemSource == OnlineJudge);
                    if (problem == null)
                    {
                        if (OnlineJudge == "CodeForces")
                        {
                            // --------------- Convert Source Id to Contest Id and problem char
                            Boolean flag = true;
                            string id = "";
                            string c = "";
                            foreach (var item in ProblemSourceId)
                            {
                                if (Char.IsLetter(item))
                                {
                                    flag = false;
                                    c += item;
                                    continue;
                                }
                                _ = (flag == true) ? id += item : c += item;
                            }
                            c = c.ToUpper();
                            // ------------------------------------------------------------------

                            var p = APi.GetProblem(OnlineJudge, id, c);

                            if (p == null) continue;
                            var newProblem = new Problem()
                            {
                                ProblemSource = p.Source,
                                ProblemSourceId = p.ProblemId,
                                ProblemTitle = p.Title.Substring(2),
                                ProblemType = 2,
                                ProblemInHtmlForm = p.Problem,
                                Rating = p.Rate,
                                UrlSource = "https://codeforces.com/problemset/problem/" + id + "/" + c
                            };
                            _dbContext.Add(newProblem);
                            _dbContext.SaveChanges();
                        }
                    }

                }

            }
        }
        private static void AddSubmission(int? contestId, int problemId, int userId, string verdict, string code)
        {
            var sub = new Submission
            {
                ContestId = contestId,
                CreationTime = DateTime.Now,
                ProblemId = problemId,
                MemoryConsumeBytes = "200",
                ProgrammingLanguage = "C++",
                Visible = false,
                UserId = userId,
                Verdict = verdict,
                SubmissionText = code,
                TimeConsumeMillis = "3055"
            };
            _dbContext.Submissions.Add(sub);

        }
        private static void LoadCurrentContest(Contest contest)
        {
            _dbContext.Entry(contest).Collection(c => c.ContestProblems).Load();
            _dbContext.Entry(contest).Collection(c => c.UserContest).Load();
            _dbContext.Entry(contest).Collection(c => c.Submissions).Load();
            foreach (var cp in contest.ContestProblems)
                _dbContext.Entry(cp).Reference(c => c.Problem).Load();
            foreach (var uc in contest.UserContest)
                _dbContext.Entry(uc).Reference(u => u.User).Load();
        }
        
        private static Problem CreateProblem(string name)
        {
            return new Problem
            {
                ProblemTitle = name
            }; 
        }
        private static Submission CreateSubmission(int userId, int problemId, int contestId, string Verdict)
        {
            return new Submission
            {
                CreationTime = DateTime.Now,
                ContestId = contestId,
                ProblemId = problemId,
                Verdict = Verdict, 
                UserId = userId
            }; 
        }
    }
}
