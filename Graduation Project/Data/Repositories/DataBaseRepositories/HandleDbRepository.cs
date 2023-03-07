using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class HandleDbRepository : IRepository<Handle>
    {
        private readonly EntitiesContext _dbContext;
        public HandleDbRepository(EntitiesContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Handle Add(Handle newAtCoderStatistics)
        {
            _dbContext.Add(newAtCoderStatistics);
            Commit();
            return newAtCoderStatistics;
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }

        public Handle Find(int id)
        {
            var handle = _dbContext.Handles.FirstOrDefault(handle => handle.HandleId == id);
            return handle;
        }

        public IList<Handle> List()
        {
            return _dbContext.Handles.ToList();
        }

        public void Remove(int id)
        {
            var handle = Find(id);
            if (handle == null) return;
            _dbContext.Handles.Remove(handle);
            Commit();
        }

        public void Update(Handle newAtCoderStatistics)
        {
            _dbContext.Handles.Update(newAtCoderStatistics);
            Commit();
        }
    }
}
