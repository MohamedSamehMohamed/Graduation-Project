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
        private readonly int blogsPerPage = 10;
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
                // ceil of number of blogs over 10
                ViewBag.TotalPageProblem = (list.Count() + blogsPerPage - 1 )/ blogsPerPage;
                if (pagenumber < 0 || pagenumber > ViewBag.TotalPageProblem) pagenumber = 1;
                ViewBag.Pagenum = pagenumber;
               
                return View(list.ToPagedList(pagenumber, blogsPerPage));
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
        private ViewBlogModel getViewModelFromBlog(Blog blog)
        {
            var userBlog = blog.userBlog.FirstOrDefault(b => b.blogId == blog.blogId&&b.blogOwenr);
            bool IsOwner = false || userBlog.User.UserIdentityId == user.UserIdentityId;
            var IsFavorite = user.userBlog.FirstOrDefault(userBlog => userBlog.isFavourite
                                                                      && userBlog.blogId==blog.blogId);
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
                isFavorite=(IsFavorite!=null)
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
                return RedirectToAction(nameof(Index));
            }catch(Exception e){
                return RedirectToAction(nameof(Index));
            }
        }

        public ActionResult CreateComment (int id,string commentContent)
        {
            try { 
                var newComment = new Comment
                    {
                        content = commentContent,
                        upvote = 0,
                        downvote = 0,
                        creationTime = DateTime.Now,
                        blogId = id
                    };
                comments.Add(newComment);
                int userId = user.UserId;
                int commentId = newComment.commentId;
                var commentVotes = CreateCommentRelation(userId, commentId);
                newComment.CommentVotes.Add(commentVotes);
                comments.Update(newComment);
                return RedirectToAction("Details",new {id});
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
                userId = userId,
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
        public ActionResult Create(Blog model)
        {
            try
            {
                int? groupID = (int?)TempData["GroupID"];
                var newBlog = new Blog
                {
                    blogtitle = model.blogtitle,
                    blogcontent = model.blogcontent,
                    groupId = groupID,
                    blogVisabilty = (groupID == null),
                    blogvote = 0,
                    creationTime = DateTime.Now
                };
                blogs.Add(newBlog);
                int userId = user.UserId;
                int blogId = newBlog.blogId;
                var userBlog= CreateRelation(userId, blogId);
                newBlog.userBlog.Add(userBlog);
                blogs.Update(newBlog);
                // if groupId is null, this means it's public blog
                if(groupID==null)
                    return RedirectToAction(nameof(Index));
                return RedirectToAction("Details", "Group", new { id = groupID });
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
                VoteValue=0,
                User=user
            };
            return usergroup;
        }
        // GET: HomeController/Edit/5
        public ActionResult Edit(int id)
        {
            try 
            { 
                if (!CanEditTheBlog(id, user.UserId))
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
        public ActionResult Edit(int id, Blog model)
        {
            try
            {
                if (!CanEditTheBlog(id, user.UserId))
                {
                    return RedirectToAction(nameof(Index));
                }
                var blog = blogs.Find(model.blogId);
                var newBlog = new Blog
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
            try 
            { 
                if (!CanEditTheBlog(id, user.UserId)) 
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

        // POST: HomeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, Blog model)
        {
            try
            {
                if (!CanEditTheBlog(id, user.UserId))
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
        private Boolean CanEditTheBlog(int blogId, int userId)
        {
            var c = blogs.Find(blogId);
            var rel = c.userBlog.FirstOrDefault(u => u.userId == userId&&u.blogOwenr);
            return  rel != null;
        }
        public ActionResult Filter(string title, string preparedBy)
        {
            try
            {
                var list = new List<ViewBlogModel>();
                if (preparedBy!=null)
                {
                    var _user = userrepository.FindByUserName(preparedBy);
                    var userBlog = _user.userBlog.Where(u => u.userId == _user.UserId && u.blogOwenr);
                    foreach(var item in userBlog){
                        var listItem = blogs.Search(title, item);
                        if (listItem != null)
                        {
                            foreach (var itemList in listItem)
                            {
                                list.Add(getViewModelFromBlog(itemList));
                            }
                        }
                    }
                }else
                {
                    var listItem = blogs.Search(title, null);
                    if (listItem != null)
                    {
                        foreach (var item in listItem)
                        {
                            list.Add(getViewModelFromBlog(item));
                        }
                    }
                }
                return View("Index", list);
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
        public ActionResult UpVote(Blog model )
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
        public ActionResult DownVote(Blog model)
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
        public ActionResult Favourite(Blog model)
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