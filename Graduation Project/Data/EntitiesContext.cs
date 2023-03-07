using GraduationProject.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GraduationProject.Data
{
    public class EntitiesContext : DbContext
    {
        public EntitiesContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Contest> Contests { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<UserContest> UserContest { get; set; }
        public DbSet<ProblemTag> ProblemTag { get; set; }
        public DbSet<Comment>Comments  { get; set; }
        public DbSet<Blog> Blogs { get; set; }
      

        public DbSet<Handle> Handles { get; set; }
        public DbSet<AtcoderStatistics> AtCoderStatistics { get; set; }
        public DbSet<CodeforcesStatistics> CodeforcesStatistics { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProblemTag>()
            .HasKey(p => new { p.ProblemId, p.TagId });
            modelBuilder.Entity<UserContest>()
            .HasKey(p => new { p.UserId, p.ContestId });
            modelBuilder.Entity<UserGroup>()
           .HasKey(p => new { p.UserId, p.GroupId });
            modelBuilder.Entity<BlogTag>()
            .HasKey(p => new { p.BlogId, p.TagId });
            modelBuilder.Entity<ContestProblem>()
            .HasKey(p => new { contestId = p.ContestId, problemId = p.ProblemId });
            modelBuilder.Entity<UserBlog>()
            .HasKey(p => new { userId = p.UserId, blogId = p.BlogId });
            modelBuilder.Entity<CommentVote>()
            .HasKey(p => new { commentId = p.CommentId, userId = p.UserId });


            modelBuilder.Entity<ProblemUser>().HasKey(pu => new
            {
                pu.UserId,
                pu.ProblemId
            });

        }
    }
}
