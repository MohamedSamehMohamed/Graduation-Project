using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.ViewModels.Rank;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace GraduationProject.Controllers
{
    public class RankController : Controller
    {
        private readonly IUserRepository<User> _userRepository;
        public RankController(IUserRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }
        public ActionResult Index(int? page)
        {
            ViewBag.function = "Index";
            var pageNumber = page ?? 1;
            ViewData["Countries"] = _getAllCountries();
            var list = _getAllUserRank().ToList();
            const int pageSize = 25;
            ViewBag.TotalPageProblem = (list.Count / pageSize) + (list.Count % pageSize == 0 ? 0 : 1);
            if (pageNumber <= 0 || pageNumber > ViewBag.TotalPageProblem) pageNumber = 1;
            ViewBag.Pagenum = pageNumber;
            var newList = list.ToPagedList(pageNumber, pageSize);
            return View(newList);
        }
        public ActionResult FilterRanking(int? page,string country,
            string userName,string birthYearLowerBoundString,string birthYearUpperBoundString,
            string ratingLowerBoundString,string ratingUpperBoundString)
        {
            var pageNumber = page ?? 1;
            ViewBag.function = "Filter";
            ViewData["Countries"] = _getAllCountries();
            country ??= "";
            userName ??= "";
            birthYearLowerBoundString ??= "";
            birthYearUpperBoundString ??= "";
            ratingLowerBoundString ??= "";
            ratingUpperBoundString ??= "";

            var birthYearLowerBound = int.Parse(birthYearLowerBoundString);
            var birthYearUpperBound = int.Parse(birthYearUpperBoundString);
            var ratingLowerBound = int.Parse(ratingLowerBoundString);
            var ratingUpperBound = int.Parse(ratingUpperBoundString);

            ViewBag.Country = country;
            ViewBag.UserName = userName;
            ViewBag.BirthYearLowerBound = birthYearLowerBoundString;
            ViewBag.BirthYearUpperBound = birthYearUpperBoundString;
            ViewBag.RatingLowerBound = ratingLowerBoundString;
            ViewBag.RatingUpperBound = ratingUpperBoundString;
            var list = _getAllUserRank();
            var newList = list.Where(e =>
                e.Country.Contains(country) &&
                (userName != "" ? e.UserName == userName : e.UserName.Contains("")) &&
                e.Birthyear >= birthYearLowerBound &&
                e.Birthyear <= birthYearUpperBound &&
                e.TotalSolved >= ratingLowerBound &&
                e.TotalSolved <= ratingUpperBound
            ).ToList();
            const int pageSize = 25;
            ViewBag.TotalPageProblem = (newList.Count / pageSize) + (newList.Count % pageSize == 0 ? 0 : 1);
            if (pageNumber < 0 || pageNumber > ViewBag.TotalPageProblem) pageNumber = 1;
            ViewBag.Pagenum = pageNumber;
            var model = newList.ToPagedList(pageNumber, pageSize);
            return View("Index", model);
        }
        private IOrderedEnumerable<RankViewModel> _getAllUserRank()
        {
            var users = _userRepository.List();
            var list = new List<RankViewModel>();
            foreach (var item in users)
            {
                var tmp = new RankViewModel
                {
                    userid = item.UserId,
                    Birthyear = item.BirthDateYear,
                    Country = item.Country,
                    UserName = item.UserName,
                    TotalSolved = item.Submissions.
                        Where(s => s.Verdict == "Accepted").
                        Select(p => p.ProblemId).Distinct().Count()
                };

                list.Add(tmp);
            }
            var newList = list.OrderByDescending(T => T.TotalSolved);
            return newList;
        }
        private static IEnumerable<string> _getAllCountries()
        {
            var countries = new List<string>();
            var cultureInfos = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (var cultureInfo in cultureInfos)
            {
                RegionInfo regionInfo = new(cultureInfo.LCID);
                if (!countries.Contains(regionInfo.EnglishName))
                {
                    countries.Add(regionInfo.EnglishName);
                }
            }
            countries.Remove("Israel");
            countries.Add("Palestine");
            countries.Sort();
            return countries;
        }
    }
    
}