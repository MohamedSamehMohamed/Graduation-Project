using System.Collections.Generic;
using GraduationProject.Data.Models;

namespace GraduationProject.Data.Repositories.Interfaces
{
    public interface IGroupRepository<T>
    {
        IList<T> List();
        T Add(T entity);
        void Update(T entity);
        void Remove(int id);
        void Commit();
        T Find(int id);
        void CreateNewGroup(int userId, Group newGroup);
        void AddUser(int groupId, int userId);
        void InviteUser(int groupId, int userId); 
        void RemoveUser(int userId, int groupId);
        void FlipFavourite(int groupId, int userId);
        void ChangeUserRole(int groupId, int userId, string role);
        IList<T> MyGroups(int id);
        bool IsOwner(int groupId, int userId); 
    }
}
