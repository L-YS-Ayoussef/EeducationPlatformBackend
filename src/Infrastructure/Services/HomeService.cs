using Application.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public interface IHomeService
{
    Task<List<HomeCourseDto>> GetLatestCoursesAsync(int take = 10);
}

public class HomeService : IHomeService
{
    private readonly AppDbContext _db;
    public HomeService(AppDbContext db) => _db = db;

    public async Task<List<HomeCourseDto>> GetLatestCoursesAsync(int take = 10)
    {
        // "Latest" = by PublishedAt if present, otherwise CreatedAt
        // Order: first by (PublishedAt ?? CreatedAt) desc, then CreatedAt desc for stability
        return await _db.Courses.AsNoTracking()
            .OrderByDescending(c => c.PublishedAt ?? c.CreatedAt)
            .ThenByDescending(c => c.CreatedAt)
            .Take(take)
            .Select(c => new HomeCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Slug = c.Slug,
                ShortDescription = c.ShortDescription,
                Level = c.Level,
                DurationHours = c.DurationHours,
                ThumbnailUrl = c.ThumbnailUrl,
                Price = c.Price,
                RatingAvg = c.RatingAvg,
                ReviewsCount = c.ReviewsCount,
                InstructorName = c.Instructor.FirstName + " " + c.Instructor.LastName
            })
            .ToListAsync();
    }
}
