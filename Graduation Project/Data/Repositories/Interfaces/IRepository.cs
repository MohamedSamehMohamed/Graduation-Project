using System.Collections.Generic;

namespace GraduationProject.Data.Repositories.Interfaces
{
    public interface IRepository<T>
    {
        IList<T> List();
        T Add(T newAtCoderStatistics);
        void Update(T newAtCoderStatistics);
        void Remove(int id);
        void Commit();
        T Find(int id);
    }
}
