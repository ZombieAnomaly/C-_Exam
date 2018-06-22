using Microsoft.EntityFrameworkCore;
 
namespace C_Exam.Models
{
    public class ExamContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public ExamContext(DbContextOptions<ExamContext> options) : base(options) { }
        public DbSet<Users> Users {get;set;}
        public DbSet<Activities> Activities {get; set;}
        public DbSet<Participants> Participants {get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Activities>()
                .HasOne(a => a.Owner)
                .WithMany(u => u.CreatedActivities);

            modelBuilder.Entity<Participants>()
                .HasKey(p => new {p.UserId, p.ActivityId});

            modelBuilder.Entity<Participants>()
                .HasOne(p => p.User)
                .WithMany(a => a.Joined)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Participants>()
                .HasOne(p => p.Activity)
                .WithMany(a => a.Participants)
                .HasForeignKey(p => p.ActivityId);
        }
        
    }
}
