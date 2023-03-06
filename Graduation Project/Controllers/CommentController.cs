using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GraduationProject.Controllers
{
    public class CommentController : Controller
    {
        private readonly IRepository<Comment> comments;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private User user;
        private int AllVote;
        private int BlodId;
        public CommentController(IRepository<GraduationProject.Data.Models.Comment> comments
            , IUserRepository<User> Userrepository
            , IHttpContextAccessor httpContextAccessor
            )
        {
            this.comments = comments;
            _httpContextAccessor = httpContextAccessor;
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            user = Userrepository.Find(userId);

        }

        // GET: CommentController
        public ActionResult Index()
        {
            var list = new List<ViewCommentModel>();
            foreach (var item in comments.List())
                list.Add(getViewModelFromComment(item));

            return View(list);
        }
        private ViewCommentModel getViewModelFromComment(GraduationProject.Data.Models.Comment comment)
        {
            
            var commentVote = comment.CommentVotes.FirstOrDefault(b => b.CommentId == comment.CommentId);
            bool IsOwner = false;
            if (commentVote.User.UserIdentityId == user.UserIdentityId)
            {
                IsOwner = true;
            }
            var model = new ViewCommentModel
            {
               commentId=comment.CommentId,
               commentOwner= commentVote.User.FirstName,
               content= comment.Content,
               creationTime=comment.CreationTime,
               vote=AllVote,
               isOwner=IsOwner
            };
            return model;
        }
        // GET: CommentController/Details/5
        public ActionResult Details(int id)
        {
            var comment = comments.Find(id);
            return View(comment);
        }

        // GET: CommentController/Create
        public ActionResult Create()
        {
            int id = (int)TempData["mydata"];
              var newComment = new Comment{BlogId=id };
            return View(newComment);
        }

        // POST: CommentController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GraduationProject.Data.Models.Comment model)
        {
            try
            {
                var newComment = new GraduationProject.Data.Models.Comment
                {
                    Content=model.Content,
                    Upvote=0
                    ,DownVote=0,
                    CreationTime=DateTime.Now,
                    BlogId=model.BlogId
                };
                comments.Add(newComment);
                int userId = user.UserId;
                int commentId = newComment.BlogId;
                var commentVotes = CreateRelation(userId, commentId);
                newComment.CommentVotes.Add(commentVotes);
                comments.Update(newComment);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        private CommentVote CreateRelation(int userId, int commentId)
        {
            var commentVotes = new CommentVote {
              CommentId=commentId,
              UserId=userId
              ,IsFavourite=false,
              Value=0,
              User=user
            };
            return commentVotes;
        }
        // GET: CommentController/Edit/5
        public ActionResult Edit(int id)
        {
            var comment = comments.Find(id);
            return View(comment);
        }

        // POST: CommentController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id,GraduationProject.Data.Models.Comment model)
        {
            try
            {
                var newComment = new GraduationProject.Data.Models.Comment
                {
                    Content = model.Content,
                    Upvote = 0
                  ,
                    DownVote = 0,
                    CreationTime = model.CreationTime,
                    BlogId = model.BlogId
                };
                comments.Update(newComment);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CommentController/Delete/5
        public ActionResult Delete(int id)
        {
            var comment = comments.Find(id);
            return View(comment);
        }

        // POST: CommentController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, GraduationProject.Data.Models.Comment model)
        {
            try
            {
                comments.Remove(model.CommentId);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
