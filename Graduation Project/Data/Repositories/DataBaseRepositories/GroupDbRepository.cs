using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class GroupDbRepository : IGroupRepository<Group>
    {
        readonly private EntitiesContext dbcontext;
        public GroupDbRepository(EntitiesContext dbcontext)
        {
            this.dbcontext = dbcontext;
            foreach(var item in this.dbcontext.Groups.ToList())
                LoadCurrentGroup(item);
        }
        public Group Add(Group newGroup)
        {
            dbcontext.Add(newGroup);
            Commit();
            return newGroup;
        }

        public void Commit()
        {
            dbcontext.SaveChanges();
        }

        public Group Find(int Id)
        {
            var group = dbcontext.Groups.FirstOrDefault(group => group.GroupId == Id);
           
            return group;
        }

        public IList<Group> List()
        {
            var list = dbcontext.Groups;
            
            return dbcontext.Groups.ToList();
        }

        public void Remove(int Id)
        {
            var group = Find(Id);
            if (group != null)
            {
                dbcontext.Groups.Remove(group);
                Commit();
            }
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
            newGroup.CreationTime = DateTime.Now; 
            Add(newGroup);
            RemoveUserRole(userId, newGroup.GroupId); 
            newGroup.UserGroup.Add(CreateUserRole(userId, newGroup.GroupId, "Creator", DateTime.Now, false));
            Commit(); 
        }

        private UserGroup CreateUserRole(int userId, int groupId, string role, DateTime MemberSince, Boolean isFavourite)
        {
            return new UserGroup { UserId = userId, GroupId = groupId, UserRole = role, MemberSince = MemberSince, IsFavourite = isFavourite };
        }
        private void RemoveUserRole(int userId, int groupId)
        {
            var group = Find(groupId);
            var userGroup = group.UserGroup.FirstOrDefault(u => u.GroupId == groupId && u.UserId == userId);
            if (userGroup != null)
            {
                group.UserGroup.Remove(userGroup);
                Commit(); 
            }
        }

        private void LoadCurrentGroup(Group group)
        {
           dbcontext.Entry(group).Collection(c => c.UserGroup).Load();
           dbcontext.Entry(group).Collection(c => c.Contests).Load();
           dbcontext.Entry(group).Collection(c => c.Blogs).Load();
            foreach (var blog in group.Blogs)
                dbcontext.Entry(blog).Collection(c => c.UserBlog).Load(); 
            foreach (var real in group.UserGroup)
                dbcontext.Entry(real).Reference(c => c.User).Load();
            foreach (var contest in group.Contests)
            {
                dbcontext.Entry(contest).Collection(c => c.UserContest).Load();
                foreach(var uc in contest.UserContest)
                    dbcontext.Entry(uc).Reference(c => c.User).Load();
            }
            
            foreach (var blog in group.Blogs)
                dbcontext.Entry(blog).Collection(c => c.UserBlog).Load();
        }
        private Boolean ValidUser(string role)
        {
            if (role == "Creator")
                return true;
            if (role == "Manager")
                return true;
            if (role == "Participant")
                return true;
            return false; 
        }
        private Boolean IsUserExist(int groupId, int userId)
        {

            var group = Find(groupId);
            var rel = group.UserGroup.FirstOrDefault(u => u.UserId == userId);
            if (rel != null && ValidUser(rel.UserRole))
                return true;
            return false; 
        }
        public void AddUser(int groupId, int userId)
        {
            if (IsUserExist(groupId, userId))
                return; 
            RemoveUserRole(userId, groupId);
            var group = Find(groupId);
            group.UserGroup.Add(CreateUserRole(userId, groupId, "Participant", DateTime.Now, false));
            Commit(); 
        }

        public void RemoveUser(int userId, int groupId)
        {
            var oldRol = Find(groupId).UserGroup.FirstOrDefault(u => u.GroupId == groupId && u.UserId == userId);
            if (oldRol != null)
            {
                RemoveUserRole(userId, groupId);
                var role = oldRol.UserRole;
                if (role == "Creator")
                {
                    SetNewCreator(groupId); 
                }
            }
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
            var anyCreator = group.UserGroup.FirstOrDefault(u => u.UserRole == "Manager");
            if (anyCreator == null)
                anyCreator = group.UserGroup.FirstOrDefault(u => u.UserRole == "Participant");
            ChangeUserRole(groupId, anyCreator.UserId, "Creator"); 
        }

        private UserGroup getUserGroupRole(int groupId, int userId)
        {
            return Find(groupId).UserGroup.FirstOrDefault(u => u.GroupId == groupId && u.UserId == userId);
        }
        public void FlipFavourite(int groupId, int userId)
        {
            var currentUserGroup = getUserGroupRole(groupId, userId); 
            if (currentUserGroup == null)
                return;
            currentUserGroup.IsFavourite ^= true;
            Commit(); 
        }
        public void ChangeUserRole(int groupId, int userId, string role)
        {
            var currentUserGroup = getUserGroupRole(groupId, userId);
            if (currentUserGroup == null)
                return;
            currentUserGroup.UserRole = role;
            Commit(); 
        }
        public void InviteUser(int groupId, int userId)
        {
            if (IsUserExist(groupId, userId))
                return; 
            RemoveUserRole(userId, groupId);
            var group = Find(groupId);
            group.UserGroup.Add(CreateUserRole(userId, groupId, "Invite", DateTime.Now, false));
            Commit();
        }
        public IList<Group> MyGroups(int userId)
        {
            IList<Group> list = new List<Group>();
            foreach (var g in dbcontext.Groups)
            {
                var rel = g.UserGroup.FirstOrDefault(u => u.UserId == userId);
                if (rel != null && ValidUser(rel.UserRole))
                    list.Add(g);
            }
            return list; 
        }
        public Boolean IsOwner(int groupId, int userId)
        {
            try
            {
                var rel = Find(groupId).UserGroup.FirstOrDefault(u => u.UserId == userId);
                return rel.UserRole == "Creator" || rel.UserRole == "Manager"; 
            }catch
            {
                return false; 
            }
        }
    }
}
