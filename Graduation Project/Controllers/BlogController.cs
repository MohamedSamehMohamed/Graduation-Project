using System;
using System.Collections.Generic;
using System.Linq;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using GraduationProject.ViewModels;
using X.PagedList;

namespace GraduationProject.Controllers
{
    [Authorize]
    public class BlogController : Controller
    {
        private readonly IBlogRepository<Blog> _blogs;
        private readonly IUserRepository<User> _userRepository;
        private readonly IRepository<Comment> _comments;
        private readonly User _user;
        private const int BlogsPerPage = 10;

        public BlogController(IBlogRepository<Blog> blogs
            , IUserRepository<User> userRepository
            , IHttpContextAccessor httpContextAccessor,
            IRepository<Comment>comments
            )
        {
            _blogs = blogs;
            _userRepository = userRepository;
            _comments = comments;
            var userId = httpContextAccessor.HttpContext.User.
                FindFirst(ClaimTypes.NameIdentifier).Value;
            _user = _userRepository.Find(userId);
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
                foreach (var item in _blogs.List())
                    list.Add(GetViewModelFromBlog(item));
                var pageNumber = page ?? 1;
                if (pageNumber < 0 || pageNumber > ViewBag.TotalPageProblem) pageNumber = 1;
                // ceil of number of blogs over 10
                ViewBag.TotalPageProblem = (list.Count + BlogsPerPage - 1 )/ BlogsPerPage;
                ViewBag.Pagenum = pageNumber;
                return View(list.ToPagedList(pageNumber, BlogsPerPage));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        public IList<ViewBlogModel> GetBlogsByUser()
        {
            var list = new List<ViewBlogModel>();
            var blog = _blogs.List();
            foreach (var item in blog) {
                var userBlog = item.UserBlog.FirstOrDefault(userBlog=> userBlog.UserId==_user.UserId
                                                                       &&userBlog.BlogOwner);
                if (userBlog != null)
                    list.Add(GetViewModelFromBlog(item));
            }
            return list;
        }
        private ViewBlogModel GetViewModelFromBlog(Blog blog)
        {
            var userBlog = blog.UserBlog.FirstOrDefault(b => b.BlogOwner);
            var isOwner = userBlog.User.UserIdentityId == _user.UserIdentityId;
            var isFavorite = _user.UserBlogs.
                FirstOrDefault(innerUserBlog => innerUserBlog.IsFavourite 
                                                && innerUserBlog.BlogId==blog.BlogId) != null;
            var model = new ViewBlogModel
            {
                blogId = blog.BlogId,
                blogtitle = blog.BlogTitle,
                blogOwner = userBlog.User.UserName,
                blogcontent = blog.BlogContent,
                blogvote = blog.BlogVote
                , creationTime = blog.CreationTime
                , Comments = blog.Comments
                ,UserBlogs=blog.UserBlog,
                CurrentUserId=_user.UserId,
                GroupId=blog.GroupId
                , isOwner = isOwner,
                isFavorite=isFavorite
            };
            return model;
        }


        // GET: HomeController/Details/5
        public ActionResult Details(int id)
        {
            try
            {
                var blog = _blogs.Find(id);
                if (blog != null)
                    return View(GetViewModelFromBlog(blog));
                return RedirectToAction(nameof(Index));
            }catch(Exception e){
                return RedirectToAction(nameof(Index));
            }
        }

        public ActionResult CreateComment (int blogId,string commentContent)
        {
            try { 
                var newComment = new Comment
                    {
                        Content = commentContent,
                        Upvote = 0,
                        DownVote = 0,
                        BlogId = blogId
                    };
                _comments.Add(newComment);
                var userId = _user.UserId;
                var commentId = newComment.CommentId;
                var commentVotes = CreateCommentRelation(userId, commentId);
                newComment.CommentVotes.Add(commentVotes);
                _comments.Update(newComment);
                return RedirectToAction("Details",new { id = blogId});
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        private CommentVote CreateCommentRelation(int userId, int commentId)
        {
            var commentVotes = new CommentVote
            {
                CommentId = commentId,
                UserId = userId,
                IsFavourite = false,
                Value = 0,
                User = _user
            };
            return commentVotes;
        }
        // GET: HomeController/Create
        public ActionResult Create(int? groupId)
        {
            try { 
                TempData["GroupID"] = groupId;
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
                var groupId = (int?)TempData["GroupID"];
                var newBlog = new Blog
                {
                    BlogTitle = model.BlogTitle,
                    BlogContent = model.BlogContent,
                    GroupId = groupId,
                    BlogVisibility = (groupId == null),
                    BlogVote = 0
                };
                _blogs.Add(newBlog);
                var userId = _user.UserId;
                var blogId = newBlog.BlogId;
                var userBlog= _createUserBlogRelation(userId, blogId);
                newBlog.UserBlog.Add(userBlog);
                _blogs.Update(newBlog);
                // if groupId is null, this means it's public blog
                return groupId==null ? RedirectToAction(nameof(Index)) : RedirectToAction("Details", "Group", new { id = groupId });
            }
            catch
            {
                return View();
            }
        }
        private UserBlog _createUserBlogRelation(int userId, int blogId)
        {
            var userBlog = new UserBlog { UserId = userId,
                BlogId = blogId,
                BlogOwner = true,
                IsFavourite = false,
                VoteValue=0,
                User=_user
            };
            return userBlog;
        }
        // GET: HomeController/Edit/5
        public ActionResult Edit(int id)
        {
            try 
            { 
                if (!_canEditTheBlog(id, _user.UserId))
                {
                    return RedirectToAction(nameof(Index));
                }
                var blog = _blogs.Find(id);
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
                if (!_canEditTheBlog(id, _user.UserId))
                {
                    return RedirectToAction(nameof(Index));
                }
                var blog = _blogs.Find(model.BlogId);
                var newBlog = new Blog
                {
                    BlogId = model.BlogId,
                    BlogTitle = model.BlogTitle,
                    BlogContent = model.BlogContent,
                    GroupId = blog.GroupId,
                    BlogVisibility = model.BlogVisibility,
                    BlogVote = blog.BlogVote
                };
                _blogs.Update(newBlog);
                return RedirectToAction("Details", new { id = model.BlogId });
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
                if (!_canEditTheBlog(id, _user.UserId)) 
                { 
                    return RedirectToAction(nameof(Index));
                }
                var blog = _blogs.Find(id);
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
                if (!_canEditTheBlog(id, _user.UserId))
                {
                    return RedirectToAction(nameof(Index));
                }
                _blogs.Remove(model.BlogId);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        private bool _canEditTheBlog(int blogId, int userId)
        {
            var blog = _blogs.Find(blogId);
            var rel = blog.UserBlog.FirstOrDefault(u => u.UserId == userId&&u.BlogOwner);
            return  rel != null;
        }
        public ActionResult Filter(string title, string preparedBy)
        {
            try
            {
                var list = new List<ViewBlogModel>();
                if (preparedBy!=null)
                {
                    var user = _userRepository.FindByUserName(preparedBy);
                    var userBlog = user.UserBlogs.Where(u => u.BlogOwner);
                    foreach(var item in userBlog){
                        var listItem = _blogs.Search(title, item);
                        if (listItem == null) continue;
                        foreach (var itemList in listItem)
                        {
                            list.Add(GetViewModelFromBlog(itemList));
                        }
                    }
                }else
                {
                    var listItem = _blogs.Search(title, null);
                    if (listItem != null)
                    {
                        foreach (var item in listItem)
                        {
                            list.Add(GetViewModelFromBlog(item));
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
                var blog = _blogs.Find(id);
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
                _blogs.UpdateVote(model.BlogId,_user.UserId,1);
                return RedirectToAction("Details", new { id = model.BlogId });
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
        public ActionResult DownVote(int id)
        {
            try { 
                var blog = _blogs.Find(id);
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
                _blogs.UpdateVote(model.BlogId, _user.UserId, -1);
                return RedirectToAction("Details", new { id = model.BlogId });
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
        public ActionResult Favourite(int id)
        {
            try { 
                var blog = _blogs.Find(id);
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
                _blogs.UpdateFavourite(model.BlogId, _user.UserId);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}