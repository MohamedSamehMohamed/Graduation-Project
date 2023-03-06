using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class CommentDbRepository : IRepository<Comment>
    {
        readonly private EntitiesContext dbcontext;
        public CommentDbRepository(EntitiesContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        public Comment Add(Comment newComment)
        {
            dbcontext.Add(newComment);
            Commit();
            return newComment;
        }

        public void Commit()
        {
            dbcontext.SaveChanges();
        }

        public Comment Find(int Id)
        {
            var comment = dbcontext.Comments
                .Include(commentVote => commentVote.CommentVotes)
                .ThenInclude(user => user.User)
                .Include(Blog=>Blog.Blog)
                .FirstOrDefault(comment => comment.CommentId == Id);
            return comment;
        }

        public IList<Comment> List()
        {
            return dbcontext.Comments
                .Include(commentVote => commentVote.CommentVotes)
                .ThenInclude(user => user.User)
                .Include(Blog => Blog.Blog)
                .ToList();
        }

        public void Remove(int Id)
        {
            var comment = Find(Id);
            if (comment != null)
            {
                dbcontext.Comments.Remove(comment);
                Commit();
            }
        }

        

        public void Update(Comment newComment)
        {
            dbcontext.Comments.Update(newComment);
            Commit();
        }
    }
}
