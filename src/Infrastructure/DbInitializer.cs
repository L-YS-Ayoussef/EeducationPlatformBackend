using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Services;
namespace Infrastructure;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher)
    {
        await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync()) return; // already seeded

        var now = DateTime.UtcNow;

        var ins1 = new User
        {
            Id = Guid.NewGuid(), Email = "instructor1@example.com",
            PasswordHash = hasher.Hash("P@ssw0rd!"), FirstName = "أحمد", LastName = "سعيد",
            AccountType = AccountType.Instructor, Title = "مهندس برمجيات", Bio = "مدرب برمجة",
            CreatedAt = now, UpdatedAt = now
        };
        var ins2 = new User
        {
            Id = Guid.NewGuid(), Email = "instructor2@example.com",
            PasswordHash = hasher.Hash("P@ssw0rd!"), FirstName = "ليلى", LastName = "حسن",
            AccountType = AccountType.Instructor, Title = "خبيرة علوم بيانات", Bio = "متخصصة تعلم آلي",
            CreatedAt = now, UpdatedAt = now
        };

        db.Users.AddRange(ins1, ins2);

        var c1 = new Course
        {
            Id = Guid.NewGuid(), Title = "أساسيات بايثون", Slug = "python-basics-ar",
            ShortDescription = "تعلم أساسيات بايثون.", Description = "محتوى مفصل عن بايثون.",
            Category = "Programming", Level = "Beginner", Language = "ar", Instructions = "متابعة الدروس بالترتيب.",
            Price = 19.99m, DurationHours = 10, ThumbnailUrl = null, PublishedAt = now,
            RatingAvg = 4.5m, ReviewsCount = 1, CreatedAt = now, UpdatedAt = now, InstructorId = ins1.Id,
            Sections = new List<Section>
            {
                new Section{
                    Title="مقدمة", ShortDescription="تعريف", Description="نظرة عامة",
                    Lessons = new List<Lesson>{
                        new Lesson{ Title="ما هي بايثون؟", Description="تعريف", VideoUrl=null }
                    },
                }
            },
            Faqs = new List<Faq>{
                new Faq{ Question="هل تحتاج خبرة مسبقة؟", Answer="لا" }
            }
        };

        var c2 = new Course
        {
            Id = Guid.NewGuid(), Title = "مقدمة تعلم الآلة", Slug = "ml-intro-ar",
            ShortDescription = "مبادئ تعلم الآلة.", Description = "خوارزميات أساسية.",
            Category = "Data Science", Level = "Intermediate", Language = "ar", Instructions = "معرفة مسبقة بالرياضيات مفيدة.",
            Price = 29.99m, DurationHours = 14, ThumbnailUrl = null, PublishedAt = now,
            RatingAvg = 0, ReviewsCount = 0, CreatedAt = now, UpdatedAt = now, InstructorId = ins2.Id,
            Sections = new List<Section>
            {
                new Section{
                    Title="مقدمة", ShortDescription="تعريف", Description="ما هو ML؟",
                    Lessons = new List<Lesson>{
                        new Lesson{ Title="أنواع التعلم", Description="مشرف وغير مشرف", VideoUrl=null }
                    },
                }
            }
        };

        db.Courses.AddRange(c1, c2);

        await db.SaveChangesAsync();
    }
}
