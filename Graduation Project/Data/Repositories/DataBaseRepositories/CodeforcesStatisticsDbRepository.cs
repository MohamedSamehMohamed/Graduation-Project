using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class CodeforcesStatisticsDbRepository : IRepository<CodeforcesStatistics>
    {
        private readonly EntitiesContext _dbContext;
        public CodeforcesStatisticsDbRepository(EntitiesContext dbContext)
        {
            _dbContext = dbContext;
        }

        public CodeforcesStatistics Add(CodeforcesStatistics newAtCoderStatistics)
        {
            _dbContext.Add(newAtCoderStatistics);
            Commit();
            return newAtCoderStatistics;
        }

        public void Commit()
        {
            _dbContext.SaveChanges();
        }

        public CodeforcesStatistics Find(int id)
        {
            var codeforcesStatistics = _dbContext.CodeforcesStatistics.
                FirstOrDefault(codeforces=>codeforces.CodeforcesStatisticsId==id);
            return codeforcesStatistics;
        }

        public IList<CodeforcesStatistics> List()
        {
           return _dbContext.CodeforcesStatistics.ToList();
        }

        public void Remove(int id)
        {
            var codeforcesStatistics = Find(id);
            if (codeforcesStatistics == null) return;
            _dbContext.CodeforcesStatistics.Remove(codeforcesStatistics);
            Commit();
        }
        
        public void Update(CodeforcesStatistics newAtCoderStatistics)
        {
            _dbContext.CodeforcesStatistics.Update(newAtCoderStatistics);
            Commit();
        }
    }
}
