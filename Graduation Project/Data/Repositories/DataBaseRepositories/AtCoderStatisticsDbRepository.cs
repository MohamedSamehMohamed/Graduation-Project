using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class AtCoderStatisticsDbRepository : IRepository<AtcoderStatistics>
    {
        private readonly EntitiesContext _dbContext;
        public AtCoderStatisticsDbRepository(EntitiesContext dbContext)
        {
            _dbContext = dbContext;
        }

        public AtcoderStatistics Add(AtcoderStatistics newAtCoderStatistics)
        {
            _dbContext.Add(newAtCoderStatistics);
            Commit();
            return newAtCoderStatistics;
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }

        public AtcoderStatistics Find(int id)
        {
            var atCoderStatistics = _dbContext.AtCoderStatistics.
                FirstOrDefault(atCoder => atCoder.AtcoderStatisticsId == id);
            return atCoderStatistics;
        }

        public IList<AtcoderStatistics> List()
        {
            return _dbContext.AtCoderStatistics.ToList();
        }

        public void Remove(int id)
        {
            var atCoderStatistics = Find(id);
            if (atCoderStatistics == null) return;
            _dbContext.AtCoderStatistics.Remove(atCoderStatistics);
            Commit();
        }
        
        public void Update(AtcoderStatistics newAtCoderStatistics)
        {
            _dbContext.AtCoderStatistics.Update(newAtCoderStatistics);
            Commit();
        }
    }
}
