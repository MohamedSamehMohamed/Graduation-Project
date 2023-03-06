using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraduationProject.Data.Repositories
{
    public class atcoderStatisticsDbRepository : IRepository<AtcoderStatistics>
    {
        readonly private EntitiesContext dbcontext;
        public atcoderStatisticsDbRepository(EntitiesContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }

        public AtcoderStatistics Add(AtcoderStatistics newatcoderStatistics)
        {
            dbcontext.Add(newatcoderStatistics);
            Commit();
            return newatcoderStatistics;
        }

        public void Commit()
        {
            dbcontext.SaveChanges();
        }

        public AtcoderStatistics Find(int Id)
        {
            var atcoderstatistics = dbcontext.atcoderStatistics.FirstOrDefault(atcoder => atcoder.AtcoderStatisticsId == Id);
            return atcoderstatistics;
        }

        public IList<AtcoderStatistics> List()
        {
            return dbcontext.atcoderStatistics.ToList();
        }

        public void Remove(int Id)
        {
            var atcoder = Find(Id);
            if (atcoder != null)
            {
                dbcontext.atcoderStatistics.Remove(atcoder);
                Commit();
            }
        }

       

        public void Update(AtcoderStatistics newAtcoderStatistics)
        {
            dbcontext.atcoderStatistics.Update(newAtcoderStatistics);
            Commit();
        }
    }
}
