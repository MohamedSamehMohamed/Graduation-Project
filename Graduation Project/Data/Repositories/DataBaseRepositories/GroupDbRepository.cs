using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class GroupDbRepository : IGroupRepository<Group>
    {
        private readonly EntitiesContext _dbContext;
        public GroupDbRepository(EntitiesContext dbContext)
        {
            _dbContext = dbContext;
            foreach(var item in _dbContext.Groups.ToList())
                LoadCurrentGroup(item);
        }
        public Group Add(Group newGroup)
        {
            _dbContext.Add(newGroup);
            Commit();
            return newGroup;
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }

        public Group Find(int id)
        {
            var group = _dbContext.Groups.FirstOrDefault(group => group.GroupId == id);
            return group;
        }

        public IList<Group> List()
        {
            var list = _dbContext.Groups;
            return _dbContext.Groups.ToList();
        }

        public void Remove(int id)
        {
            var group = Find(id);
            if (group == null) return;
            _dbContext.Groups.Remove(group);
            Commit();
        }
        
        public void Update(Group newGroup)
        {
            var group = Find(newGroup.GroupId);
            group.GroupTitle = newGroup.GroupTitle;
            group.GroupDescription = newGroup.GroupDescription;
            group.Password = newGroup.Password;
            group.Visible = newGroup.Visible; 
            Commit();
        }

        public void CreateNewGroup(int userId, Group newGroup)
        {
            Add(newGroup);
            _removeUserRole(userId, newGroup.GroupId); 
            newGroup.UserGroup.Add(_CreateUserRole(userId, newGroup.GroupId, "Creator", 
                DateTime.Now, false));
            Commit(); 
        }

        private static UserGroup _CreateUserRole(int userId, int groupId, string role, 
            DateTime memberSince, bool isFavourite)
        {
            return new UserGroup { UserId = userId, GroupId = groupId, UserRole = role, 
                MemberSince = memberSince, IsFavourite = isFavourite };
        }
        private void _removeUserRole(int userId, int groupId)
        {
            var group = Find(groupId);
            var userGroup = group.UserGroup.FirstOrDefault(u => u.GroupId == groupId && 
                                                                u.UserId == userId);
            if (userGroup == null) return;
            group.UserGroup.Remove(userGroup);
            Commit();
        }

        private void LoadCurrentGroup(Group group)
        {
           _dbContext.Entry(group).Collection(c => c.UserGroup).Load();
           _dbContext.Entry(group).Collection(c => c.Contests).Load();
           _dbContext.Entry(group).Collection(c => c.Blogs).Load();
            foreach (var blog in group.Blogs)
                _dbContext.Entry(blog).Collection(c => c.UserBlog).Load(); 
            foreach (var real in group.UserGroup)
                _dbContext.Entry(real).Reference(c => c.User).Load();
            foreach (var contest in group.Contests)
            {
                _dbContext.Entry(contest).Collection(c => c.UserContest).Load();
                foreach(var uc in contest.UserContest)
                    _dbContext.Entry(uc).Reference(c => c.User).Load();
            }
            
            foreach (var blog in group.Blogs)
                _dbContext.Entry(blog).Collection(c => c.UserBlog).Load();
        }
        private static bool _validUser(string role)
        {
            switch (role)
            {
                case "Creator":
                case "Manager":
                case "Participant":
                    return true;
                default:
                    return false;
            }
        }
        private bool IsUserExist(int groupId, int userId)
        {
            var group = Find(groupId);
            var rel = group.UserGroup.FirstOrDefault(u => u.UserId == userId);
            return rel != null && _validUser(rel.UserRole);
        }
        public void AddUser(int groupId, int userId)
        {
            if (IsUserExist(groupId, userId))
                return; 
            _removeUserRole(userId, groupId);
            var group = Find(groupId);
            group.UserGroup.Add(_CreateUserRole(userId, groupId, "Participant", DateTime.Now, false));
            Commit(); 
        }

        public void RemoveUser(int userId, int groupId)
        {
            var oldRol = Find(groupId).UserGroup.FirstOrDefault(u => u.GroupId == groupId && u.UserId == userId);
            if (oldRol == null) return;
            _removeUserRole(userId, groupId);
            var role = oldRol.UserRole;
            if (role == "Creator")
            {
                SetNewCreator(groupId); 
            }
            Commit();
            // Assign New Use As a creator if the deleted user is the creator 
        }

        private void SetNewCreator(int groupId)
        {
            var group = Find(groupId);
            if(group.UserGroup.Count == 0)
            {
                Remove(groupId);
                return;
            }
            var anyCreator = group.UserGroup.FirstOrDefault(u => u.UserRole == "Manager") 
                             ?? group.UserGroup.FirstOrDefault(u => u.UserRole == "Participant");
            if (anyCreator == null) return;
            ChangeUserRole(groupId, anyCreator.UserId, "Creator"); 
        }

        private UserGroup GetUserGroupRole(int groupId, int userId)
        {
            return Find(groupId).UserGroup.FirstOrDefault(u => u.GroupId == groupId &&
                                                               u.UserId == userId);
        }
        public void FlipFavourite(int groupId, int userId)
        {
            var currentUserGroup = GetUserGroupRole(groupId, userId); 
            if (currentUserGroup == null)
                return;
            currentUserGroup.IsFavourite ^= true;
            Commit(); 
        }
        public void ChangeUserRole(int groupId, int userId, string role)
        {
            var currentUserGroup = GetUserGroupRole(groupId, userId);
            if (currentUserGroup == null)
                return;
            currentUserGroup.UserRole = role;
            Commit(); 
        }
        public void InviteUser(int groupId, int userId)
        {
            if (IsUserExist(groupId, userId))
                return; 
            _removeUserRole(userId, groupId);
            var group = Find(groupId);
            group.UserGroup.Add(_CreateUserRole(userId, groupId, "Invite", 
                DateTime.Now, false));
            Commit();
        }
        public IList<Group> MyGroups(int userId)
        {
            var myGroups = new List<Group>();
            foreach (var group in List())
            {
                var userGroup = group.UserGroup.FirstOrDefault(u => u.UserId == userId);
                if (userGroup == null || !_validUser(userGroup.UserRole)) continue;
                myGroups.Add(group);
            }
            return myGroups;
        }
        public bool IsOwner(int groupId, int userId)
        {
            var rel = Find(groupId).UserGroup.FirstOrDefault(u => u.UserId == userId);
            return rel != null && (rel.UserRole == "Creator" || rel.UserRole == "Manager");
        }
    }
}
