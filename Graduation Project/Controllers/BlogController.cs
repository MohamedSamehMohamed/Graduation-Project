using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GraduationProject.ViewModels;
using X.PagedList;

namespace GraduationProject.Controllers
{
    [Authorize]
    public class BlogController : Controller
    {
        private readonly IBlogRepository<Blog> blogs;
        private readonly IUserRepository<User> userrepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<Comment> comments;
        private User user;
        public BlogController(IBlogRepository<Blog> blogs
            , IUserRepository<User> Userrepository
            , IHttpContextAccessor httpContextAccessor,
            IRepository<Comment>comments
            )
        {
            this.blogs = blogs;
            userrepository = Userrepository;
            _httpContextAccessor = httpContextAccessor;
            this.comments = comments;
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            user = userrepository.Find(userId);

        }
        // GET: HomeController
        public ActionResult Index(int? page)
        {
            try 
            { 
                if (TempData["BlogsByUser"]!=null && TempData["BlogsByUser"].ToString()=="UserBlogs") {
                    TempData["BlogsUser"] = "blogUser";
                    return View(GetBlogsByUser());
                }
                var list = new List<ViewBlogModel>();
                foreach (var item in blogs.List())
                    list.Add(getViewModelFromBlog(item));
                int pagenumber = page ?? 1;
                ViewBag.TotalPageProblem = (list.Count() / 10) + (list.Count() % 10 == 0 ? 0 : 1);
                if (pagenumber < 0 || pagenumber > ViewBag.TotalPageProblem) pagenumber = 1;
                ViewBag.Pagenum = pagenumber;
               
                return View(list.ToPagedList(pagenumber, 10));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        public IList<ViewBlogModel> GetBlogsByUser()
        {
            var list = new List<ViewBlogModel>();
            var blog = blogs.List();
            foreach (var item in blog) {
                var userblog = item.userBlog.FirstOrDefault(userBlog=> userBlog.userId==user.UserId&&
                userBlog.blogId==item.blogId&&userBlog.blogOwenr==true);
                if (userblog != null)
                    list.Add(getViewModelFromBlog(item));
            }
            return list;
        }
        private ViewBlogModel getViewModelFromBlog(GraduationProject.Data.Models.Blog blog)
        {

            var userBlog = blog.userBlog.FirstOrDefault(b => b.blogId == blog.blogId&&b.blogOwenr==true);
            bool IsOwner = false;
            if (userBlog.User.UserIdentityId == user.UserIdentityId) {
                IsOwner = true;
            }
            var IsFavorite = user.userBlog.FirstOrDefault(userBlog => userBlog.isFavourite == true
                                 &&userBlog.blogId==blog.blogId);
            var model = new ViewBlogModel
            {
                blogId = blog.blogId,
                blogtitle = blog.blogtitle,
                blogOwner = userBlog.User.UserName,
                blogcontent = blog.blogcontent,
                blogvote = blog.blogvote
                , creationTime = blog.creationTime
                , Comments = blog.Comments
                ,UserBlogs=blog.userBlog,
                CurrentUserId=user.UserId,
                GroupId=blog.groupId
                , isOwner = IsOwner,
                isFavorite=(IsFavorite!=null)?true:false
            };
            return model;
        }


        // GET: HomeController/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                var blog = blogs.Find(id);
                if (blog != null)
                    return View(getViewModelFromBlog(blog));
                else
                    return RedirectToAction(nameof(Index));
            }catch(Exception e){
                return RedirectToAction(nameof(Index));
            }
        }

        public ActionResult CreateComment (int id,string CommentContent)
        {
            try { 
            var newComment = new Comment
            {
                content = CommentContent,
                upvote = 0
                    ,
                downvote = 0,
                creationTime = DateTime.Now,
                blogId = id
            };
            comments.Add(newComment);
            int userId = user.UserId;
            int commentId = newComment.blogId;
            var commentVotes = CreateCommentRelation(userId, commentId);
            newComment.CommentVotes.Add(commentVotes);
            comments.Update(newComment);
            return RedirectToAction("Details",new { id=id});
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        private commentVote CreateCommentRelation(int userId, int commentId)
        {
            var commentVotes = new commentVote
            {
                commentId = commentId,
                userId = userId
              ,
                isFavourite = false,
                value = 0,
                User = user
            };
            return commentVotes;
        }
        // GET: HomeController/Create
        public ActionResult Create(int? id)
        {
            try { 
            TempData["GroupID"] = id;
            return View();
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: HomeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GraduationProject.Data.Models.Blog model)
        {
            try
            {
                int? GrpupID = (int?)TempData["GroupID"];
                var newBlog = new GraduationProject.Data.Models.Blog
                {
                    blogtitle = model.blogtitle,
                    blogcontent = model.blogcontent,
                    groupId = GrpupID,
                    blogVisabilty = false,
                    blogvote = 0,
                    creationTime = DateTime.Now
                };
                blogs.Add(newBlog);
                int userId = user.UserId;
                int blogId = newBlog.blogId;
                var userBlog= CreateRelation(userId, blogId);
                newBlog.userBlog.Add(userBlog);
                blogs.Update(newBlog);
                if(GrpupID==null)
                return RedirectToAction(nameof(Index));
                else
                return RedirectToAction("Details", "Group", new { id = GrpupID });
            }
            catch
            {
                return View();
            }
        }
        private UserBlog CreateRelation(int userId, int blogId)
        {
            var usergroup = new UserBlog { userId = userId,
                blogId = blogId,
                blogOwenr = true,
                isFavourite = false,
                VoteValue=0
            ,User=user};
            return usergroup;
        }
        // GET: HomeController/Edit/5
        public ActionResult Edit(int id)
        {
            try { 
            if (!CanAccessTheBlog(id, user.UserId))
            {
                return RedirectToAction(nameof(Index));
            }
            var blog = blogs.Find(id);
            return View(blog);
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: HomeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, GraduationProject.Data.Models.Blog model)
        {
            try
            {
                if (!CanAccessTheBlog(id, user.UserId))
                {
                    return RedirectToAction(nameof(Index));
                }
                var blog = blogs.Find(model.blogId);
                var newBlog = new GraduationProject.Data.Models.Blog
                {

                    blogId = model.blogId,
                    blogtitle = model.blogtitle,
                    blogcontent = model.blogcontent,
                    groupId = blog.groupId,
                    blogVisabilty = model.blogVisabilty,
                    blogvote = blog.blogvote,
                    creationTime = blog.creationTime
                };
                blogs.Update(newBlog);
                return RedirectToAction("Details", new { id = model.blogId });
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController/Delete/5
        public ActionResult Delete(int id)
        {
            try { 
            if (!CanAccessTheBlog(id, user.UserId)) { 
                return RedirectToAction(nameof(Index));
            }
            var blog = blogs.Find(id);
            return View(blog);
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: HomeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, GraduationProject.Data.Models.Blog model)
        {
            try
            {
                if (!CanAccessTheBlog(id, user.UserId))
                {
                    return RedirectToAction(nameof(Index));
                }
                blogs.Remove(model.blogId);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        private Boolean CanAccessTheBlog(int blogId, int userId)
        {
            var c = blogs.Find(blogId);
            var rel = c.userBlog.FirstOrDefault(u => u.userId == userId&&u.blogOwenr==true);
            return  rel != null;
        }
        public ActionResult Filter(string Title,string PrepeardBy)
        {
            try { 
            var list = new List<ViewBlogModel>();
            
            if (PrepeardBy!=null)
            {
                var _user = userrepository.FindByUserName(PrepeardBy);
                var userBlog = _user.userBlog.Where(u => u.userId == _user.UserId && u.blogOwenr == true);
               foreach(var item in userBlog){
                    var listItem = blogs.Search(Title, item);
                    if (listItem != null)
                        foreach (var itemList in listItem)
                            list.Add(getViewModelFromBlog(itemList));
                }
                
            }else
            {
                var listItem = blogs.Search(Title, null);
             if(listItem!=null)
                foreach (var item in listItem)
                    list.Add(getViewModelFromBlog(item));
            }
            if(list.Count>0)
                return View("Index",list);
            else
                return View("Index");
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public ActionResult UpVote(int id)
        {
            try { 
            var blog = blogs.Find(id);
            return UpVote(blog);
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpVote(GraduationProject.Data.Models.Blog model )
        {
            try
            {

                blogs.UpdateVote(model.blogId,user.UserId,1);

                return RedirectToAction("Details", new { id = model.blogId });
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
        public ActionResult DownVote(int id)
        {
            try { 
            var blog = blogs.Find(id);
            return DownVote(blog);
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DownVote(GraduationProject.Data.Models.Blog model)
        {
            try
            {

                blogs.UpdateVote(model.blogId, user.UserId, -1);

                return RedirectToAction("Details", new { id = model.blogId });
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
        //--------
        public ActionResult Favourite(int id)
        {
            try { 
            var blog = blogs.Find(id);
            return Favourite(blog);
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Favourite(GraduationProject.Data.Models.Blog model)
        {
            try
            {

                blogs.UpdateFavourite(model.blogId, user.UserId);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}