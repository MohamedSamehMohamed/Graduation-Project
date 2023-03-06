using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.API;
using Microsoft.Extensions.DependencyInjection;

namespace GraduationProject.Data.Models
{
    public class DbInitializer
    {
        static EntitiesContext dbcontext;
        public static void Seed(IServiceProvider serviceProvider)
        {
            
            var rand = new Random();
            dbcontext =
                serviceProvider.GetRequiredService<EntitiesContext>();



            //////////////// Add Problem ////////////////////////
            //GetAllProblem();
            //return;
            //////////////  End Add problem /////////////////
            var d = DateTime.Now;

            //dbcontext.Submissions.RemoveRange(dbcontext.Submissions); 
            //dbcontext.SaveChanges();

            string code = "#include<iostream>;" +
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
            var users = dbcontext.Users;
            var problems = dbcontext.Problems;
                foreach(var p in problems)
                {
                    foreach(var u in users)
                    {
                        int lim = 1 + rand.Next(3); 
                        for (int i = 0; i < lim; i++)
                        {
                            AddSubmission(11, p.ProblemId, u.UserId, verdicts[i], code);

                        }
                        
                    }

                }
            dbcontext.SaveChanges(); 
            


        }
        private static void GetAllProblem()
        {
            for (int i = 33; i <= 60; i++)
            {
                for (char ca = 'a'; ca <= 'd'; ca++)
                {
                    var ProblemSourceId = i.ToString() + ca;
                    var OnlineJudge = "CodeForces";
                    var problem = dbcontext.Problems.FirstOrDefault(u => u.problemSourceId == ProblemSourceId && u.ProblemSource == OnlineJudge);
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
                            Problem newproblem = new Problem()
                            {
                                ProblemSource = p.Source,
                                problemSourceId = p.ProblemId,
                                problemTitle = p.Title.Substring(2),
                                problemType = 2,
                                ProblemHtml = p.Problem,
                                rating = p.Rate,
                                UrlSource = "https://codeforces.com/problemset/problem/" + id + "/" + c
                            };
                            dbcontext.Add(newproblem);
                            dbcontext.SaveChanges();
                        }
                    }

                }

            }
        }
        private static void AddSubmission(int? contestId, int problemId, int userId, string verdict, string code)
        {
            var sub = new Submission
            {
                contestId = contestId,
                CreationTime = DateTime.Now,
                ProblemId = problemId,
                MemoryConsumeBytes = "200",
                ProgrammingLanguage = "C++",
                Visable = false,
                userId = userId,
                Verdict = verdict,
                SubmissionText = code,
                TimeConsumeMillis = "3055"
            };
            dbcontext.Submissions.Add(sub);

        }
        private static void LoadCurrentContest(Contest contest)
        {
            dbcontext.Entry(contest).Collection(c => c.ContestProblems).Load();
            dbcontext.Entry(contest).Collection(c => c.UserContest).Load();
            dbcontext.Entry(contest).Collection(c => c.Submissions).Load();
            foreach (var cp in contest.ContestProblems)
                dbcontext.Entry(cp).Reference(c => c.problem).Load();
            foreach (var uc in contest.UserContest)
                dbcontext.Entry(uc).Reference(u => u.User).Load();
        }
        
        private static Problem CreateProblem(string name)
        {
            return new Problem
            {
                problemTitle = name
            }; 
        }
        private static Submission CreateSubmission(int userId, int problemId, int contestId, string Verdict)
        {
            return new Submission
            {
                CreationTime = DateTime.Now,
                contestId = contestId,
                ProblemId = problemId,
                Verdict = Verdict, 
                userId = userId
            }; 
        }
    }
}
