using Application.Dtos;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public interface IInstructorDashboardService
{
    Task<CourseDetailsDto> CreateCourseAsync(Guid instructorId, CourseCreateDto dto);
    Task<CourseDetailsDto> UpdateCourseAsync(Guid instructorId, Guid courseId, CourseUpdateDto dto);
    Task<PagedResult<CourseListItemDto>> ListOwnedCoursesAsync(Guid instructorId, int page, int pageSize);
    Task<CourseDetailsDto> GetOwnedCourseAsync(Guid instructorId, Guid courseId);
    Task<List<EnrollmentStudentDto>> GetOwnedCourseStudentsAsync(Guid instructorId, Guid courseId);
    Task GradeAttachmentAsync(Guid instructorId, int attachmentId, decimal grade);
}

public class InstructorDashboardService : IInstructorDashboardService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public InstructorDashboardService(AppDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<CourseDetailsDto> CreateCourseAsync(Guid instructorId, CourseCreateDto dto)
    {
        if (await _db.Courses.AnyAsync(c => c.Slug == dto.Slug))
            throw new InvalidOperationException("Slug already exists.");

        var now = DateTime.UtcNow;
        var course = _mapper.Map<Course>(dto);
        course.Id = Guid.NewGuid();
        course.InstructorId = instructorId;
        course.CreatedAt = now;
        course.UpdatedAt = now;
        course.RatingAvg = 0;
        course.ReviewsCount = 0;

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        return await GetOwnedCourseAsync(instructorId, course.Id);
    }

    public async Task<CourseDetailsDto> UpdateCourseAsync(Guid instructorId, Guid courseId, CourseUpdateDto dto)
    {
        var course = await _db.Courses
            .Include(c => c.Sections).ThenInclude(s => s.Lessons)
            .Include(c => c.Sections).ThenInclude(s => s.Assignments)
            .SingleOrDefaultAsync(c => c.Id == courseId);

        if (course is null) throw new KeyNotFoundException("Course not found.");
        if (course.InstructorId != instructorId) throw new UnauthorizedAccessException();

        // Scalars
        course.Title = dto.Title;
        course.Slug = dto.Slug;
        course.ShortDescription = dto.ShortDescription;
        course.Description = dto.Description;
        course.Category = dto.Category;
        course.Level = dto.Level;
        course.Language = dto.Language;
        course.Instructions = dto.Instructions;
        course.Price = dto.Price;
        course.DurationHours = dto.DurationHours;
        course.ThumbnailUrl = dto.ThumbnailUrl;
        course.PublishedAt = dto.PublishedAt;
        course.UpdatedAt = DateTime.UtcNow;

        // Sync sections
        var incomingSectionIds = dto.Sections.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToHashSet();
        course.Sections.RemoveWhere(s => !incomingSectionIds.Contains(s.Id) && dto.Sections.Any());

        foreach (var sDto in dto.Sections)
        {
            Section sEntity;
            if (sDto.Id.HasValue)
            {
                sEntity = course.Sections.Single(s => s.Id == sDto.Id);
                sEntity.Title = sDto.Title;
                sEntity.ShortDescription = sDto.ShortDescription;
                sEntity.Description = sDto.Description;
            }
            else
            {
                sEntity = new Section
                {
                    Title = sDto.Title,
                    ShortDescription = sDto.ShortDescription,
                    Description = sDto.Description
                };
                course.Sections.Add(sEntity);
            }

            // Lessons
            var incLessonIds = sDto.Lessons.Where(l => l.Id.HasValue).Select(l => l.Id!.Value).ToHashSet();
            sEntity.Lessons.RemoveWhere(l => !incLessonIds.Contains(l.Id) && sDto.Lessons.Any());
            foreach (var lDto in sDto.Lessons)
            {
                if (lDto.Id.HasValue)
                {
                    var l = sEntity.Lessons.Single(x => x.Id == lDto.Id);
                    l.Title = lDto.Title; l.Description = lDto.Description; l.VideoUrl = lDto.VideoUrl;
                }
                else
                {
                    sEntity.Lessons.Add(new Lesson { Title = lDto.Title, Description = lDto.Description, VideoUrl = lDto.VideoUrl });
                }
            }

            // Assignments
            var incAssIds = sDto.Assignments.Where(a => a.Id.HasValue).Select(a => a.Id!.Value).ToHashSet();
            sEntity.Assignments.RemoveWhere(a => !incAssIds.Contains(a.Id) && sDto.Assignments.Any());
            foreach (var aDto in sDto.Assignments)
            {
                if (aDto.Id.HasValue)
                {
                    var a = sEntity.Assignments.Single(x => x.Id == aDto.Id);
                    a.AssignmentNumber = aDto.AssignmentNumber;
                    a.Title = aDto.Title; a.Description = aDto.Description; a.PdfUrl = aDto.PdfUrl; a.MaxScore = aDto.MaxScore;
                }
                else
                {
                    sEntity.Assignments.Add(new Assignment
                    {
                        AssignmentNumber = aDto.AssignmentNumber,
                        Title = aDto.Title, Description = aDto.Description, PdfUrl = aDto.PdfUrl, MaxScore = aDto.MaxScore
                    });
                }
            }
        }

        await _db.SaveChangesAsync();
        return await GetOwnedCourseAsync(instructorId, course.Id);
    }

    public async Task<PagedResult<CourseListItemDto>> ListOwnedCoursesAsync(Guid instructorId, int page, int pageSize)
    {
        var q = _db.Courses.AsNoTracking().Where(c => c.InstructorId == instructorId)
            .OrderByDescending(c => c.CreatedAt);

        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .ProjectTo<CourseListItemDto>(_mapper.ConfigurationProvider).ToListAsync();

        return new PagedResult<CourseListItemDto>(items, total, page, pageSize);
    }

    public async Task<CourseDetailsDto> GetOwnedCourseAsync(Guid instructorId, Guid courseId)
    {
        var course = await _db.Courses.AsNoTracking()
            .Include(c => c.Instructor)
            .Include(c => c.Sections).ThenInclude(s => s.Lessons)
            .Include(c => c.Faqs)
            .SingleOrDefaultAsync(c => c.Id == courseId);

        if (course is null) throw new KeyNotFoundException("Course not found.");
        if (course.InstructorId != instructorId) throw new UnauthorizedAccessException();

        return _mapper.Map<CourseDetailsDto>(course);
    }

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
            .Include(a => a.Assignment).ThenInclude(asg => asg.Section).ThenInclude(s => s.Course)
            .SingleOrDefaultAsync(a => a.Id == attachmentId);

        if (att is null) throw new KeyNotFoundException("Attachment not found.");
        if (att.Assignment.Section.Course.InstructorId != instructorId) throw new UnauthorizedAccessException();

        if (grade < 0 || grade > att.Assignment.MaxScore)
            throw new InvalidOperationException($"Grade must be between 0 and {att.Assignment.MaxScore}");

        att.Grade = grade;
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
