using Application.Dtos;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public interface IInstructorDashboardService
{
    Task<CourseDetailsDto> GetOwnedCourseAsync(Guid instructorId, Guid courseId);
    Task<CourseDetailsDto> CreateCourseSimpleAsync(Guid instructorId, CourseSimpleUpsertDto dto);
    Task<CourseDetailsDto> UpdateCourseSimpleAsync(Guid instructorId, Guid courseId, CourseSimpleUpsertDto dto);

    Task<SectionDtoSimple> CreateSectionAsync(Guid instructorId, Guid courseId, SectionUpsertDto dto);
    Task<SectionDtoSimple> UpdateSectionAsync(Guid instructorId, int sectionId, SectionUpsertDto dto);

    Task<LessonDtoSimple> CreateLessonAsync(Guid instructorId, int sectionId, LessonUpsertDto dto);
    Task<LessonDtoSimple> UpdateLessonAsync(Guid instructorId, int lessonId, LessonUpsertDto dto);

    Task<List<EnrollmentStudentDto>> GetOwnedCourseStudentsAsync(Guid instructorId, Guid courseId);
    Task GradeAttachmentAsync(Guid instructorId, int attachmentId, decimal grade);
    Task DeleteCourseAsync(Guid instructorId, Guid courseId);
}

public class InstructorDashboardService : IInstructorDashboardService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    // ------------- helpers -------------
    public InstructorDashboardService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<CourseDetailsDto> GetOwnedCourseAsync(Guid instructorId, Guid courseId)
    {
        var course = await _db.Courses.AsNoTracking()
            .Include(c => c.Instructor)
            .Include(c => c.Sections)
                .ThenInclude(s => s.Lessons)
                    .ThenInclude(l => l.Assignments) // assignments are under lessons now
            .Include(c => c.Faqs)
            .SingleOrDefaultAsync(c => c.Id == courseId);

        if (course is null) throw new KeyNotFoundException("Course not found.");
        if (course.InstructorId != instructorId) throw new UnauthorizedAccessException();

        return _mapper.Map<CourseDetailsDto>(course);
    }

    // ------------- Course Creation -------------
    public async Task<CourseDetailsDto> CreateCourseSimpleAsync(Guid instructorId, CourseSimpleUpsertDto dto)
    {
        var now = DateTime.UtcNow;
        var course = new Course
        {
            Id = Guid.NewGuid(),
            InstructorId = instructorId,
            Title = dto.Title,
            Slug = MakeSlug(dto.Title),
            ShortDescription = dto.ShortDescription,
            Description = dto.Description,
            Category = dto.Category,
            Level = dto.Level,
            Language = dto.Language,
            Instructions = "",        // empty for simple endpoint
            Price = dto.Price,
            DurationHours = dto.DurationHours,
            ThumbnailUrl = dto.ThumbnailUrl,
            CreatedAt = now,
            UpdatedAt = now,
            RatingAvg = 0,
            ReviewsCount = 0
        };

        if (dto.Faqs is not null && dto.Faqs.Count > 0)
        {
            foreach (var f in dto.Faqs)
                course.Faqs.Add(new Faq { Question = f.Question, Answer = f.Answer });
        }

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        return await GetOwnedCourseAsync(instructorId, course.Id);
    }

    private static string MakeSlug(string title)
    {
        // super-simple slug (replace spaces). Replace with your existing slugger if you have one.
        return title.Trim().ToLowerInvariant().Replace(' ', '-');
    }
    
    public async Task<CourseDetailsDto> UpdateCourseSimpleAsync(Guid instructorId, Guid courseId, CourseSimpleUpsertDto dto)
    {
        var c = await _db.Courses
            .Include(x => x.Faqs)
            .SingleOrDefaultAsync(x => x.Id == courseId)
            ?? throw new KeyNotFoundException("Course not found.");

        if (c.InstructorId != instructorId) throw new UnauthorizedAccessException();

        c.Title = dto.Title;
        c.Slug = MakeSlug(dto.Title); // keep slugs derived (or keep old if you prefer)
        c.ShortDescription = dto.ShortDescription;
        c.Description = dto.Description;
        c.Category = dto.Category;
        c.Level = dto.Level;
        c.Language = dto.Language;
        c.Price = dto.Price;
        c.DurationHours = dto.DurationHours;
        c.ThumbnailUrl = dto.ThumbnailUrl;
        c.UpdatedAt = DateTime.UtcNow;

        // Replace FAQs safely
        c.Faqs.Clear();
        if (dto.Faqs is not null && dto.Faqs.Count > 0)
        {
            foreach (var f in dto.Faqs)
                c.Faqs.Add(new Faq { Question = f.Question, Answer = f.Answer });
        }

        await _db.SaveChangesAsync();
        return await GetOwnedCourseAsync(instructorId, c.Id);
    }

    // ------------------ SECTION ------------------
    public async Task<SectionDtoSimple> CreateSectionAsync(Guid instructorId, Guid courseId, SectionUpsertDto dto)
    {
        var course = await _db.Courses.SingleOrDefaultAsync(c => c.Id == courseId)
            ?? throw new KeyNotFoundException("Course not found.");
        if (course.InstructorId != instructorId) throw new UnauthorizedAccessException();

        var s = new Section
        {
            CourseId = courseId,
            Title = dto.Title,
            ShortDescription = dto.ShortDescription,
            Description = dto.Description
        };
        _db.Sections.Add(s);
        await _db.SaveChangesAsync();

        return new SectionDtoSimple
        {
            Id = s.Id,
            CourseId = courseId,
            Title = s.Title,
            ShortDescription = s.ShortDescription,
            Description = s.Description
        };
    }

    public async Task<SectionDtoSimple> UpdateSectionAsync(Guid instructorId, int sectionId, SectionUpsertDto dto)
    {
        var s = await _db.Sections
            .Include(x => x.Course)
            .SingleOrDefaultAsync(x => x.Id == sectionId)
            ?? throw new KeyNotFoundException("Section not found.");
        if (s.Course.InstructorId != instructorId) throw new UnauthorizedAccessException();

        s.Title = dto.Title;
        s.ShortDescription = dto.ShortDescription;
        s.Description = dto.Description;
        await _db.SaveChangesAsync();

        return new SectionDtoSimple
        {
            Id = s.Id,
            CourseId = s.CourseId,
            Title = s.Title,
            ShortDescription = s.ShortDescription,
            Description = s.Description
        };
    }

    // ------------------ LESSON ------------------
    public async Task<LessonDtoSimple> CreateLessonAsync(Guid instructorId, int sectionId, LessonUpsertDto dto)
    {
        var section = await _db.Sections
            .Include(x => x.Course)
            .SingleOrDefaultAsync(x => x.Id == sectionId)
            ?? throw new KeyNotFoundException("Section not found.");
        if (section.Course.InstructorId != instructorId) throw new UnauthorizedAccessException();

        var l = new Lesson
        {
            SectionId = sectionId,
            Title = dto.Title,
            Description = dto.Description,
            VideoUrl = dto.VideoUrl
        };
        _db.Lessons.Add(l);
        await _db.SaveChangesAsync();

        return new LessonDtoSimple
        {
            Id = l.Id,
            SectionId = sectionId,
            Title = l.Title,
            Description = l.Description,
            VideoUrl = l.VideoUrl
        };
    }

    public async Task<LessonDtoSimple> UpdateLessonAsync(Guid instructorId, int lessonId, LessonUpsertDto dto)
    {
        var l = await _db.Lessons
            .Include(x => x.Section).ThenInclude(s => s.Course)
            .SingleOrDefaultAsync(x => x.Id == lessonId)
            ?? throw new KeyNotFoundException("Lesson not found.");
        if (l.Section.Course.InstructorId != instructorId) throw new UnauthorizedAccessException();

        l.Title = dto.Title;
        l.Description = dto.Description;
        l.VideoUrl = dto.VideoUrl;
        await _db.SaveChangesAsync();

        return new LessonDtoSimple
        {
            Id = l.Id,
            SectionId = l.SectionId,
            Title = l.Title,
            Description = l.Description,
            VideoUrl = l.VideoUrl
        };
    }

    // -----------------------------------------------------------------------------------------------
    public async Task<List<EnrollmentStudentDto>> GetOwnedCourseStudentsAsync(Guid instructorId, Guid courseId)
    {
        var ok = await _db.Courses.AnyAsync(c => c.Id == courseId && c.InstructorId == instructorId);
        if (!ok) throw new UnauthorizedAccessException();

        var q = from e in _db.CoursesStudents
                join u in _db.Users on e.StudentId equals u.Id
                where e.CourseId == courseId
                select new EnrollmentStudentDto
                {
                    StudentId = u.Id,
                    Name = u.FirstName + " " + u.LastName,
                    Email = u.Email,
                    Status = e.Status,
                    EnrolledAt = e.EnrolledAt,
                    ProgressPct = 0
                };
        return await q.ToListAsync();
    }

    public async Task GradeAttachmentAsync(Guid instructorId, int attachmentId, decimal grade)
    {
        var att = await _db.Attachments
            .Include(a => a.Assignment)
                .ThenInclude(asg => asg.Lesson)
                    .ThenInclude(l => l.Section)
                        .ThenInclude(s => s.Course)
            .SingleOrDefaultAsync(a => a.Id == attachmentId);

        if (att is null) throw new KeyNotFoundException("Attachment not found.");
        if (att.Assignment.Lesson.Section.Course.InstructorId != instructorId) throw new UnauthorizedAccessException();

        if (grade < 0 || grade > att.Assignment.MaxScore)
            throw new InvalidOperationException($"Grade must be between 0 and {att.Assignment.MaxScore}");

        att.Grade = grade;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteCourseAsync(Guid instructorId, Guid courseId)
    {
        var course = await _db.Courses
            .Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Assignments).ThenInclude(a => a.Attachments)
            .Include(c => c.Faqs)
            .Include(c => c.Reviews)
            .Include(c => c.Enrollments)
            .SingleOrDefaultAsync(c => c.Id == courseId);

        if (course is null) throw new KeyNotFoundException("Course not found.");
        if (course.InstructorId != instructorId) throw new UnauthorizedAccessException();

        // Manually clear dependents to avoid FK issues if cascade isn't configured
        _db.Attachments.RemoveRange(course.Sections.SelectMany(s => s.Lessons).SelectMany(l => l.Assignments).SelectMany(a => a.Attachments));
        _db.Assignments.RemoveRange(course.Sections.SelectMany(s => s.Lessons).SelectMany(l => l.Assignments));
        _db.Lessons.RemoveRange(course.Sections.SelectMany(s => s.Lessons));
        _db.Sections.RemoveRange(course.Sections);
        _db.FAQ.RemoveRange(course.Faqs);
        _db.Reviews.RemoveRange(course.Reviews);
        _db.CoursesStudents.RemoveRange(course.Enrollments);

        _db.Courses.Remove(course);
        await _db.SaveChangesAsync();
    }
}

static class CollectionExtensions
{
    public static void RemoveWhere<T>(this ICollection<T> coll, Func<T, bool> predicate)
    {
        var toRemove = coll.Where(predicate).ToList();
        foreach (var item in toRemove) coll.Remove(item);
    }
}
