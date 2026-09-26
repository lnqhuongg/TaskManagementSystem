using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Models.Entity;

namespace TaskManagementSystem.Models
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* 
             * Enum → String
             */
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>();

            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Status)
                .HasConversion<string>();

            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Priority)
                .HasConversion<string>();

            /* 
             * RELATIONSHIP BETWEEN TABLES 
             */
            // User 1 → N TaskItem
            modelBuilder.Entity<User>()
                .HasMany(u => u.Tasks)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);

            // Category 1 → N TaskItem
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Tasks)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId);
        }
    }
}