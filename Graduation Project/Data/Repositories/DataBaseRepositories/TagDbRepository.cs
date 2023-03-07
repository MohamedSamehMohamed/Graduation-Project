using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraduationProject.Data.Models;
using GraduationProject.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Data.Repositories.DataBaseRepositories
{
    public class TagDbRepository : IRepository<Tag>
    {
        readonly private EntitiesContext dbcontext;
        public TagDbRepository(EntitiesContext dbcontext)
        {
            this.dbcontext = dbcontext; 
        }
        public Tag Add(Tag newAtCoderStatistics)
        {
            dbcontext.Tags.Add(newAtCoderStatistics);
            Commit();
            return newAtCoderStatistics; 
        }

        public void Commit()
        {
            dbcontext.SaveChanges(); 
        }

        public Tag Find(int Id)
        {
            var tag = dbcontext.Tags
                .Include(pu => pu.ProblemTag)
                .Include(b => b.BlogTag)
                .FirstOrDefault(tag => tag.TagId == Id);
            return tag; 
        }

        public IList<Tag> List()
        {
            return dbcontext.Tags
                .Include(pu => pu.ProblemTag)
                .Include(b => b.BlogTag)
                .ToList(); 
        }

        public void Remove(int Id)
        {
            var tag = Find(Id);
            if (tag != null)
            {
                dbcontext.Tags.Remove(tag);
                Commit(); 
            }
        }

        

        public void Update(Tag newAtCoderStatistics)
        {
            dbcontext.Tags.Update(newAtCoderStatistics); 
            Commit(); 
        }
    }
}
