using System.Collections.Generic;

namespace GraduationProject.Data.Repositories.Interfaces
{
    public interface IUserRepository<T>
    {
        IList<T> List();
        T Add(T entity);
        void Update(T entity);
        void Remove(int id);
        void Commit();
        T Find(int id);
        T Find(string id);
        T FindByUserName(string name);
    }
}
