using GraduationProject.Data.Models;
using System.Collections.Generic;

namespace GraduationProject.Data.Repositories.Interfaces
{
    public interface IBlogRepository<T>
    {
        IList<T> List();
        T Add(T entity);
        void Update(T entity);
        void Remove(int id);
        void Commit();
        T Find(int id);
        void UpdateVote(int blogId, int userId, int typeVote);
        void UpdateFavourite(int blogId, int userId);
        IList<T> Search(string title, UserBlog preparedBy);
    }
}
