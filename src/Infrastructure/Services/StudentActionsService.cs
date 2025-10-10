using Application.Dtos;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public interface IStudentActionsService
{
    Task EnrollAsync(Guid studentId, Guid courseId);
    Task CancelEnrollmentAsync(Guid studentId, Guid courseId); // soft cancel
    Task CreateOrUpdateReviewAsync(Guid studentId, ReviewCreateDto dto);
    Task SubmitAttachmentAsync(Guid studentId, StudentAttachmentCreateDto dto);
}

public class StudentActionsService : IStudentActionsService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    public StudentActionsService(AppDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task EnrollAsync(Guid studentId, Guid courseId)
    {
        var exists = await _db.CoursesStudents.FindAsync(studentId, courseId);
        if (exists is null)
        {
            _db.CoursesStudents.Add(new CourseStudent
            {
                StudentId = studentId,
                CourseId = courseId,
                Status = "active",
                EnrolledAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
        else if (exists.Status == "cancelled")
        {
            exists.Status = "active";
            exists.EnrolledAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        // else already active -> no-op
    }

    public async Task CancelEnrollmentAsync(Guid studentId, Guid courseId)
    {
        var e = await _db.CoursesStudents.FindAsync(studentId, courseId);
        if (e is null) return;
        e.Status = "cancelled"; // documented choice: soft-cancel, keep record
        await _db.SaveChangesAsync();
    }

    // -----------------------------------------------------------------------------------------------
    public async Task CreateOrUpdateReviewAsync(Guid studentId, ReviewCreateDto dto)
    {
        // single review per (student, course) enforced by unique index
        var rev = await _db.Reviews.SingleOrDefaultAsync(r => r.StudentId == studentId && r.CourseId == dto.CourseId);
        if (rev is null)
        {
            _db.Reviews.Add(new Review
            {
                CourseId = dto.CourseId,
                StudentId = studentId,
                ReviewContent = dto.ReviewContent,
                Rate = dto.Rate,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            rev.ReviewContent = dto.ReviewContent;
            rev.Rate = dto.Rate;
        }
        await _db.SaveChangesAsync();

        // recalc aggregates
        await RecalculateCourseRatingAsync(dto.CourseId);
    }

    public async Task SubmitAttachmentAsync(Guid studentId, StudentAttachmentCreateDto dto)
    {
        // ensure assignment exists
        var asg = await _db.Assignments.SingleOrDefaultAsync(a => a.Id == dto.AssignmentId);
        if (asg is null) throw new KeyNotFoundException("Assignment not found.");

        _db.Attachments.Add(new Attachment
        {
            Title = dto.Title,
            Description = dto.Description,
            PdfUrl = dto.PdfUrl,
            StudentId = studentId,
            AssignmentId = dto.AssignmentId,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    private async Task RecalculateCourseRatingAsync(Guid courseId)
    {
        var agg = await _db.Reviews.Where(r => r.CourseId == courseId)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Avg = g.Average(x => (decimal)x.Rate) })
            .SingleOrDefaultAsync();

        var c = await _db.Courses.FindAsync(courseId);
        if (c is null) return;
        c.ReviewsCount = agg?.Count ?? 0;
        c.RatingAvg = agg?.Avg ?? 0;
        await _db.SaveChangesAsync();
    }
}
