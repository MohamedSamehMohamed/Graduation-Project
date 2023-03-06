using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Data.Repositories
{
    public class UserDbRepository : IUserRepository<User>
    {
        readonly private EntitiesContext dbcontext; 
        public UserDbRepository(EntitiesContext dbcontext)
        {
            this.dbcontext = dbcontext; 
        }
        public User Add(User newUser)
        {
            dbcontext.Users.Add(newUser);
            Commit();
            return newUser; 
        }
        public User Find(int Id)
        {
            var user = dbcontext.Users
                .Include(s => s.Submissions)
                .Include(pu => pu.UserProblems)
                .ThenInclude(pu => pu.Problem)
                .Include(u => u.UserContest)
                .ThenInclude(c => c.Contest)
                .Include(u => u.UserGroup)
                .ThenInclude(ug => ug.Group)
                .Include(u => u.UserBlogs)
                .ThenInclude(ub => ub.Blog)
                .FirstOrDefault(user => user.UserId == Id);
            return user; 
        }
        public User Find(string Id)
        {
            var user = dbcontext.Users
                .Include(s => s.Submissions)
                .Include(pu => pu.UserProblems)
                .ThenInclude(pu=>pu.Problem)
                .Include(u => u.UserContest)
                .ThenInclude(c => c.Contest)
                .FirstOrDefault(user => user.UserIdentityId == Id);
            return user;
        }
        public IList<User> List()
        {
            return dbcontext.Users
                .Include(s => s.Submissions)
                .Include(pu => pu.UserProblems)
                .ThenInclude(pu => pu.Problem)
                .Include(u => u.UserContest)
                .ThenInclude(c => c.Contest)
                .Include(u => u.UserGroup)
                .ThenInclude(ug => ug.Group)
                .Include(u => u.UserBlogs)
                .ThenInclude(ub => ub.Blog)
                .ToList(); 
        }

        public void Remove(int Id)
        {
            var user = Find(Id);
            if (user != null)
            {
                dbcontext.Users.Remove(user);
                Commit();
            }
        }

        public void Update(User newUser)
        {
            var user = Find(newUser.UserId);
            user.FirstName = newUser.FirstName;
            user.LastName = newUser.LastName;
            user.Country = newUser.Country;
            user.BirthDateYear = newUser.BirthDateYear;
            user.PhotoUrl = newUser.PhotoUrl;
            Commit();
        }
        public void Commit()
        {
            dbcontext.SaveChanges();
        }

        public User FindByUserName(string name)
        {
            var user = dbcontext.Users.Include(s => s.Submissions)
                .Include(pu => pu.UserProblems)
                .ThenInclude(pu => pu.Problem)
                .Include(u => u.UserContest)
                .ThenInclude(c => c.Contest)
                .Include(u => u.UserGroup)
                .ThenInclude(ug => ug.Group)
                .Include(u => u.UserBlogs)
                .ThenInclude(ub => ub.Blog).FirstOrDefault(u => u.UserName == name);
            return user; 
        }
    }
}
