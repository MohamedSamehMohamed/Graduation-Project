using System.Collections.Generic;
using System.Linq;
using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class UserDbRepository : IUserRepository<User>
    {
        private readonly EntitiesContext _dbContext; 
        public UserDbRepository(EntitiesContext dbContext)
        {
            _dbContext = dbContext; 
        }
        public User Add(User newUser)
        {
            _dbContext.Users.Add(newUser);
            Commit();
            return newUser; 
        }
        public User Find(int id)
        {
            var user = List().FirstOrDefault(user => user.UserId == id);
            return user; 
        }
        public User Find(string id)
        {
            var user = List().FirstOrDefault(user => user.UserIdentityId == id);
            return user;
        }
        public IList<User> List()
        {
            return _dbContext.Users
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

        public void Remove(int id)
        {
            var user = Find(id);
            if (user == null) return;
            _dbContext.Users.Remove(user);
            Commit();
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
            _dbContext.SaveChanges();
        }

        public User FindByUserName(string name)
        {
            var user = List().FirstOrDefault(u => u.UserName == name);
            return user; 
        }
    }
}
