using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class BlogDbRepository : IBlogRepository<Blog>
    { 
        private readonly EntitiesContext _dbContext;
        public BlogDbRepository(EntitiesContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Blog Add(Blog newBlog)
        {
            _dbContext.Add(newBlog);
            Commit();
            return newBlog;
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }

        
        public Blog Find(int id)
        {
            var blog = List().FirstOrDefault(blog => blog.BlogId == id);
            return blog;
        }

        public IList<Blog> List()
        {
            return _dbContext.Blogs.Include(userBlog=>userBlog.UserBlog)
                .ThenInclude(user => user.User)
                .Include(comment => comment.Comments)
                .ThenInclude(commentVote => commentVote.CommentVotes)
                .Include(group=>group.Group)
                .ToList();
        }

        public void Remove(int id)
        {
            var blog = Find(id);
            if (blog == null) return;
            _dbContext.Blogs.Remove(blog);
            Commit();
        }
        
        public void Update(Blog newBlog)
        {
            var blog = Find(newBlog.BlogId);
            blog.BlogTitle = newBlog.BlogTitle;
            blog.BlogContent = newBlog.BlogContent;
            Commit();
        }
        public void UpdateVote(int blogId,int userId,int voteValue)
        {
            var blog = Find(blogId);
            var userBlog = blog.UserBlog.FirstOrDefault(user => user.UserId == userId);

            if (userBlog == null)
            {
                var newUserBlog = new UserBlog
                {
                    BlogId = blogId,
                    UserId = userId,
                    BlogOwner = false,
                    VoteValue = voteValue,
                    IsFavourite = false
                };
                blog.UserBlog.Add(newUserBlog);
            }
            else if (userBlog.VoteValue == 0)
            {
                userBlog.VoteValue = voteValue;
                blog.BlogVote += voteValue;
            }
            Commit();
        }
        public void UpdateFavourite(int blogId, int userId) {
              var blog = Find(blogId);
            var userBlog = blog.UserBlog.FirstOrDefault(user => user.UserId == userId);

            if (userBlog == null)
            {
                var newUserBlog = new UserBlog
                {
                    BlogId = blogId,
                    UserId = userId,
                    BlogOwner = false,
                    VoteValue = 0,
                    IsFavourite = true
                };
                blog.UserBlog.Add(newUserBlog);
            }
            else
                userBlog.IsFavourite = userBlog.IsFavourite switch
                {
                    false => true,
                    true => false
                };

            Commit();
        }
        public IList<Blog> Search(string title, UserBlog preparedBy)
        {
            var blogs = _getBlogs();
            if (title!=null && preparedBy != null)
            {
                return blogs.Where(blog => blog.BlogTitle.Contains(title) &&  
                                    blog.BlogId == preparedBy.BlogId).ToList();
            }
            if (preparedBy == null && title != null)
            {
                return blogs.Where(blog => blog.BlogTitle.Contains(title)).ToList();
            }
            return preparedBy != null ? blogs.Where(blog => blog.BlogId== preparedBy.BlogId).ToList() : blogs;
        }
    }
}
