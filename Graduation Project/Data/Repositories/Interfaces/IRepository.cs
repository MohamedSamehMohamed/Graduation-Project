using System.Collections.Generic;

namespace GraduationProject.Data.Repositories.Interfaces
{
    public interface IRepository<T>
    {
        IList<T> List();
        T Add(T entity);
        void Update(T entity);
        void Remove(int id);
        void Commit();
        T Find(int id);
    }
}
