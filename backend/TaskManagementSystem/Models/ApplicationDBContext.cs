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
             * USER
             */
            modelBuilder.Entity<User>()
                .Property(x => x.Username)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(x => x.Email)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(x => x.Role)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            /* 
             * CATEGORY
             */
            modelBuilder.Entity<Category>()
                .Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Category>()
                .Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();


            /* 
             * TASK ITEM
             */
            modelBuilder.Entity<TaskItem>()
                .Property(x => x.Title)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            modelBuilder.Entity<TaskItem>()
                .Property(x => x.Priority)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

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