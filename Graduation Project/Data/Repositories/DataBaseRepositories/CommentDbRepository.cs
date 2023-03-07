using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class CommentDbRepository : IRepository<Comment>
    {
        private readonly EntitiesContext _dbContext;
        public CommentDbRepository(EntitiesContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Comment Add(Comment newAtCoderStatistics)
        {
            _dbContext.Add(newAtCoderStatistics);
            Commit();
            return newAtCoderStatistics;
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }

        
        public Comment Find(int id)
        {
            return List().FirstOrDefault(comment => comment.CommentId == id);
        }

        public IList<Comment> List()
        {
            return _dbContext.Comments
                .Include(commentVote => commentVote.CommentVotes)
                .ThenInclude(user => user.User)
                .Include(blog => blog.Blog).ToList();
        }

        public void Remove(int Id)
        {
            var comment = Find(Id);
            if (comment != null)
            {
                _dbContext.Comments.Remove(comment);
                Commit();
            }
        }

        

        public void Update(Comment newAtCoderStatistics)
        {
            _dbContext.Comments.Update(newAtCoderStatistics);
            Commit();
        }
    }
}
