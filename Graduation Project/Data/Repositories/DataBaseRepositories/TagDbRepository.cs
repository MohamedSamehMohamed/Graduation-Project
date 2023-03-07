using System.Collections.Generic;
using System.Linq;
using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class TagDbRepository : IRepository<Tag>
    {
        private readonly EntitiesContext _dbContext;
        public TagDbRepository(EntitiesContext dbContext)
        {
            _dbContext = dbContext; 
        }
        public Tag Add(Tag newAtCoderStatistics)
        {
            _dbContext.Tags.Add(newAtCoderStatistics);
            Commit();
            return newAtCoderStatistics; 
        }

        public void Commit()
        {
            _dbContext.SaveChanges(); 
        }

        public Tag Find(int id)
        {
            var tag = List().FirstOrDefault(tag => tag.TagId == id);
            return tag; 
        }

        public IList<Tag> List()
        {
            return _dbContext.Tags
                .Include(pu => pu.ProblemTag)
                .Include(b => b.BlogTag)
                .ToList(); 
        }

        public void Remove(int id)
        {
            var tag = Find(id);
            if (tag == null) return;
            _dbContext.Tags.Remove(tag);
            Commit();
        }

        public void Update(Tag newAtCoderStatistics)
        {
            _dbContext.Tags.Update(newAtCoderStatistics); 
            Commit(); 
        }
    }
}
