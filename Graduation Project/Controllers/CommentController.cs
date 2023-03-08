using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using GraduationProject.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace GraduationProject.Controllers
{
    public class CommentController : Controller
    {
        private readonly IRepository<Comment> _comments;
        private readonly User _user;
        private int _allVote;
        private int _blodId;
        public CommentController(IRepository<Comment> comments
            , IUserRepository<User> userRepository
            , IHttpContextAccessor httpContextAccessor
            )
        {
            _comments = comments;
            var userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            _user = userRepository.Find(userId);
        }

        // GET: CommentController
        public ActionResult Index()
        {
            var list = new List<ViewCommentModel>();
            foreach (var item in _comments.List())
                list.Add(_getViewModelFromComment(item));
            return View(list);
        }
        private ViewCommentModel _getViewModelFromComment(Comment comment)
        {
            var commentVote = comment.CommentVotes.FirstOrDefault(b => b.CommentId == comment.CommentId);
            var isOwner = commentVote != null && commentVote.User.UserIdentityId == _user.UserIdentityId;
            var ownerName = commentVote != null ? commentVote.User.FirstName : "";
            var model = new ViewCommentModel
            {
                commentId=comment.CommentId,
                commentOwner= ownerName,
                content= comment.Content,
                creationTime=comment.CreationTime,
                isOwner=isOwner
            };
            return model;
        }

        // GET: CommentController/Create
        public ActionResult Create()
        {
            var id = (int)TempData["mydata"]!;
            var newComment = new Comment{BlogId=id };
            return View(newComment);
        }

        // POST: CommentController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Comment model)
        {
            try
            {
                var newComment = new Comment
                {
                    Content=model.Content,
                    Upvote=0
                    ,DownVote=0,
                    BlogId=model.BlogId
                };
                _comments.Add(newComment);
                var userId = _user.UserId;
                var commentId = newComment.BlogId;
                var commentVotes = CreateRelation(userId, commentId);
                newComment.CommentVotes.Add(commentVotes);
                _comments.Update(newComment);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        private CommentVote CreateRelation(int userId, int commentId)
        {
            var commentVotes = new CommentVote 
            {
              CommentId=commentId,
              UserId=userId
              ,IsFavourite=false,
              Value=0,
              User=_user
            };
            return commentVotes;
        }
        // GET: CommentController/Edit/5
        public ActionResult Edit(int id)
        {
            var comment = _comments.Find(id);
            return View(comment);
        }

        // POST: CommentController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Comment model)
        {
            try
            {
                var newComment = new Comment
                {
                    Content = model.Content,
                    Upvote = 0,
                    DownVote = 0,
                    CreationTime = model.CreationTime,
                    BlogId = model.BlogId
                };
                _comments.Update(newComment);
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
            var comment = _comments.Find(id);
            return View(comment);
        }

        // POST: CommentController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, GraduationProject.Data.Models.Comment model)
        {
            try
            {
                _comments.Remove(model.CommentId);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
