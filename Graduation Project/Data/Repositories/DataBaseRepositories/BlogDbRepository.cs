using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class BlogDbRepository : IBlogRepository<Blog>
    {
        readonly private EntitiesContext dbcontext;
    public BlogDbRepository(EntitiesContext dbcontext)
    {
        this.dbcontext = dbcontext;
    }

        public Blog Add(Blog newBlog)
        {
            dbcontext.Add(newBlog);
            Commit();
            return newBlog;
        }

        public void Commit()
        {
            dbcontext.SaveChanges();
        }

        public Blog Find(int Id)
        {
            var blog = dbcontext.Blogs.Include(userBlog => userBlog.UserBlog)
                .ThenInclude(user=>user.User)
                .Include(comment=>comment.Comments)
                .ThenInclude(commentvote=>commentvote.CommentVotes)
                .Include(group=>group.Group)
                .FirstOrDefault(blog => blog.BlogId == Id);
            return blog;
        }

        public IList<Blog> List()
        {
            return dbcontext.Blogs.Include(userBlog=>userBlog.UserBlog)
                 .ThenInclude(user => user.User)
                .Include(comment => comment.Comments)
                .ThenInclude(commentvote => commentvote.CommentVotes)
                .Include(group=>group.Group)
                .ToList();
        }

        public void Remove(int Id)
        {
            var blog = Find(Id);
            if (blog != null)
            {
                dbcontext.Blogs.Remove(blog);
                Commit();
            }
        }

        

        public void Update(Blog newBlod)
        {
            var blog = Find(newBlod.BlogId);
            blog.BlogTitle = newBlod.BlogTitle;
            blog.BlogContent = newBlod.BlogContent;
            Commit();
        }
        public void UpdateVote(int blogId,int userId,int typeVote)
        {
            var blog = Find(blogId);
            var userBlog = blog.UserBlog.FirstOrDefault(User => User.UserId == userId);

            if (userBlog == null)
            {
                UserBlog newuserBlog = new UserBlog
                {
                    BlogId = blogId,
                    UserId = userId,
                    BlogOwner = false,
                    VoteValue = typeVote,
                    IsFavourite = false
                };
                blog.UserBlog.Add(newuserBlog);
            }
            else if (userBlog.VoteValue == 0)
            {
                userBlog.VoteValue = typeVote;
            }
            else {
                return;
            }
            blog.BlogVote = blog.BlogVote + typeVote;
            Commit();

        }
        public void UpdateFavourite(int blogId, int userId) {
              var blog = Find(blogId);
            var userBlog = blog.UserBlog.FirstOrDefault(User => User.UserId == userId);

            if (userBlog == null)
            {
                UserBlog newuserBlog = new UserBlog
                {
                    BlogId = blogId,
                    UserId = userId,
                    BlogOwner = false,
                    VoteValue = 0,
                    IsFavourite = true
                };
                blog.UserBlog.Add(newuserBlog);
            }
            else if (userBlog.IsFavourite == false)
            {
                userBlog.IsFavourite = true;
            }
            else if (userBlog.IsFavourite == true)
            {
                userBlog.IsFavourite = false;
            }
            Commit();
        }
        public IList<Blog> Search(string Title, UserBlog PrepeardBy) {
           
            if (Title!=null && PrepeardBy != null)
            {
                var result = dbcontext.Blogs.Include(userBlog => userBlog.UserBlog)
                    .ThenInclude(user => user.User)
                    .Include(comment => comment.Comments)
                    .Include(group => group.Group).Where(blog => blog.BlogTitle.Contains(Title) &&  blog.BlogId == PrepeardBy.BlogId
                   ).ToList();
                return result;

            }
            else if (PrepeardBy == null&& Title != null)
            {
                var result = dbcontext.Blogs.Include(userBlog => userBlog.UserBlog)
                     .ThenInclude(user => user.User)
                     .Include(comment => comment.Comments)
                     .Include(group => group.Group).Where(blog => blog.BlogTitle.Contains(Title) 
                    ).ToList();
                return result;
            }else if(Title == null && PrepeardBy != null)
            {
                var result = dbcontext.Blogs.Include(userBlog => userBlog.UserBlog)
                                     .ThenInclude(user => user.User)
                                     .Include(comment => comment.Comments)
                                     .Include(group => group.Group).Where(blog => blog.BlogId== PrepeardBy.BlogId
                                    ).ToList();
                return result;
            }else 
            return null;
        }
    }
}
