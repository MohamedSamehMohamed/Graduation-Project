using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.ViewModels.Rank;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace GraduationProject.Controllers.Rank
{
    public class RankController : Controller
    {
        private readonly ISubmissionRepository<Submission> SubmissionRepository;
        private readonly IUserRepository<User> userRepository;
        public RankController(IUserRepository<User> _userRepository, ISubmissionRepository<Submission> _SubmissionRepository)
        {
            SubmissionRepository = _SubmissionRepository;
            userRepository = _userRepository;
        }
        public ActionResult Index(int? page)
        {
            ViewBag.function = "Index";
            int pagenumber = page ?? 1;
            ViewData["Countries"] = GetAllCountries();
            IOrderedEnumerable<RankViewModel> list = GetAllUserRank();
            ViewBag.TotalPageProblem = (list.Count() / 25) + (list.Count() % 25 == 0 ? 0 : 1);
            if (pagenumber <= 0 || pagenumber > ViewBag.TotalPageProblem) pagenumber = 1;
            ViewBag.Pagenum = pagenumber;
            var newlist = list.ToPagedList(pagenumber, 25);
            return View(newlist);
        }
        public ActionResult FilterRanking(int? page,string Country,string UserName,string BirthYearLowerBound,string BirthYearUpperBound,string RatingLowerBound,string RatingUpperBound)
        {
            int pagenum = page ?? 1;
            ViewBag.function = "Filter";
            ViewData["Countries"] = GetAllCountries();
            Country = (Country == null ? "" : Country);
            UserName = (UserName == null ? "" : UserName);
            BirthYearLowerBound = ((BirthYearLowerBound == null ) ? "" : BirthYearLowerBound);
            BirthYearUpperBound = ((BirthYearUpperBound == null ) ? "" : BirthYearUpperBound);
            RatingLowerBound = ((RatingLowerBound == null ) ? "" : RatingLowerBound);
            RatingUpperBound = ((RatingUpperBound == null ) ? "" : RatingUpperBound);

            var birthYearLowerBound = Int32.Parse(BirthYearLowerBound);
            var birthYearUpperBound = Int32.Parse(BirthYearUpperBound);
            var ratingLowerBound = Int32.Parse(RatingLowerBound);
            var ratingUpperBound = Int32.Parse(RatingUpperBound);

            ViewBag.Country = Country;
            ViewBag.UserName = UserName;
            ViewBag.BirthYearLowerBound = BirthYearLowerBound;
            ViewBag.BirthYearUpperBound = BirthYearUpperBound;
            ViewBag.RatingLowerBound = RatingLowerBound;
            ViewBag.RatingUpperBound = RatingUpperBound;
            IOrderedEnumerable<RankViewModel> list = GetAllUserRank();
            var newlist = list.Where(e =>
                e.Country.Contains(Country)&&
                (UserName != "" ? e.UserName==UserName:e.UserName.Contains("")) &&
                e.Birthyear >= birthYearLowerBound &&
                e.Birthyear <= birthYearUpperBound &&
                e.TotalSolved >= ratingLowerBound &&
                e.TotalSolved <= ratingUpperBound 
                );

            ViewBag.TotalPageProblem = (newlist.Count() / 25) + (newlist.Count() % 25 == 0 ? 0 : 1);
            if (pagenum < 0 || pagenum > ViewBag.TotalPageProblem) pagenum = 1;
            ViewBag.Pagenum = pagenum;
            var model = newlist.ToPagedList(pagenum, 25);
            return View("Index", model);
        }
        IOrderedEnumerable<RankViewModel> GetAllUserRank()
        {
            var users = userRepository.List();
            IList<RankViewModel> list = new List<RankViewModel>();
            foreach (var item in users)
            {
                RankViewModel tmp = new RankViewModel
                {
                    userid = item.UserId,
                    Birthyear = item.BirthDateYear,
                    Country = item.Country,
                    UserName = item.UserName
                };
                tmp.TotalSolved = item.Submissions.Where(s => s.Verdict == "Accepted").Select(p => p.ProblemId).Distinct().Count();

                list.Add(tmp);
            }
            IOrderedEnumerable<RankViewModel> newlist = list.OrderByDescending(T => T.TotalSolved);
            return newlist;
        }
        IEnumerable<string> GetAllCountries()
        {
            List<string> Countries = new List<string>();
            CultureInfo[] cultureInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo cultureInfo in cultureInfos)
            {
                RegionInfo regionInfo = new(cultureInfo.LCID);
                if (!Countries.Contains(regionInfo.EnglishName))
                {
                    Countries.Add(regionInfo.EnglishName);
                }
            }
            Countries.Remove("Israel");
            Countries.Add("Palastine");
            Countries.Sort();
            return Countries;
        }
    }
    
}