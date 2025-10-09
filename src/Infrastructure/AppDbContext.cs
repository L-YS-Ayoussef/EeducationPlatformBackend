using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<CourseStudent> CoursesStudents => Set<CourseStudent>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Faq> FAQ => Set<Faq>();
    public DbSet<EmailCode> EmailCodes => Set<EmailCode>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // All strings unicode (Arabic-friendly)
        foreach (var entity in b.Model.GetEntityTypes())
        {
            foreach (var prop in entity.GetProperties().Where(p => p.ClrType == typeof(string)))
            {
                prop.SetIsUnicode(true);
            }
        }

        // Users
        b.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(256);
            e.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            e.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Title).HasMaxLength(200);
            e.Property(x => x.Bio).HasMaxLength(2000);
            e.HasIndex(x => x.Email).IsUnique();
        });

        // Courses
        b.Entity<Course>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(300);
            e.Property(x => x.Slug).IsRequired().HasMaxLength(350);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.ShortDescription).IsRequired().HasMaxLength(500);
            e.Property(x => x.Category).IsRequired().HasMaxLength(100);
            e.Property(x => x.Level).IsRequired().HasMaxLength(50);
            e.Property(x => x.Language).IsRequired().HasMaxLength(50);
            e.Property(x => x.Instructions).IsRequired().HasMaxLength(2000);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.RatingAvg).HasColumnType("decimal(3,2)");
            e.HasOne(x => x.Instructor)
                .WithMany(u => u.Courses)
                .HasForeignKey(x => x.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Section
        b.Entity<Section>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(250);
            e.Property(x => x.ShortDescription).IsRequired().HasMaxLength(500);
            e.Property(x => x.Description).IsRequired().HasMaxLength(4000);
            e.HasOne(x => x.Course).WithMany(c => c.Sections).HasForeignKey(x => x.CourseId);
        });

        // Lessons
        b.Entity<Lesson>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(250);
            e.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            e.HasOne(x => x.Section).WithMany(s => s.Lessons).HasForeignKey(x => x.SectionId);
        });

        // Assignments
        b.Entity<Assignment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(250);
            e.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            e.Property(x => x.MaxScore).HasColumnType("decimal(5,2)");
            e.HasOne(x => x.Lesson).WithMany(l => l.Assignments).HasForeignKey(x => x.LessonId);
        });

        // Attachments (submissions)
        b.Entity<Attachment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).IsRequired().HasMaxLength(2000);
            e.Property(x => x.Grade).HasColumnType("decimal(5,2)");
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Assignment).WithMany(a => a.Attachments).HasForeignKey(x => x.AssignmentId);
        });

        // Enrollment composite key
        b.Entity<CourseStudent>(e =>
        {
            e.HasKey(x => new { x.StudentId, x.CourseId });
            e.Property(x => x.Status).IsRequired().HasMaxLength(20);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Course).WithMany(c => c.Enrollments).HasForeignKey(x => x.CourseId);
        });

        // Reviews with unique (StudentId, CourseId)
        b.Entity<Review>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ReviewContent).IsRequired().HasMaxLength(2000);
            e.HasIndex(x => new { x.StudentId, x.CourseId }).IsUnique();
            e.HasOne(x => x.Course).WithMany(c => c.Reviews).HasForeignKey(x => x.CourseId);
            e.HasOne(x => x.Student).WithMany().HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
        });

        // FAQ
        b.Entity<Faq>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Question).IsRequired().HasMaxLength(500);
            e.Property(x => x.Answer).IsRequired().HasMaxLength(2000);
            e.HasOne(x => x.Course).WithMany(c => c.Faqs).HasForeignKey(x => x.CourseId);
        });

        // EmailCodes
        b.Entity<EmailCode>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.Property(x => x.Purpose).IsRequired().HasMaxLength(50);
            e.Property(x => x.CodeHash).IsRequired().HasMaxLength(128);
            e.HasIndex(x => new { x.Email, x.Purpose });
        });
    }
}
