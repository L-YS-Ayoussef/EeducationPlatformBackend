using Application.Dtos;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services;

public interface ICourseCatalogService
{
    Task<PagedResult<CourseListItemDto>> QueryAsync(CourseQuery query);
    Task<CourseDetailsDto?> GetBySlugAsync(string slug, Guid? studentId = null);
}
public interface IInstructorService
{
    Task<PagedResult<InstructorListItemDto>> QueryAsync(InstructorQuery query);
    Task<InstructorDetailsDto?> GetByUsernameAsync(string username);
}

public class CourseCatalogService : ICourseCatalogService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    public CourseCatalogService(AppDbContext db, IMapper mapper){ _db=db; _mapper=mapper; }

    public async Task<PagedResult<CourseListItemDto>> QueryAsync(CourseQuery q)
    {
        var query = _db.Courses.AsNoTracking().Include(c => c.Instructor).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Q))
            query = query.Where(c => EF.Functions.Like(c.Title, $"%{q.Q}%") || EF.Functions.Like(c.ShortDescription, $"%{q.Q}%"));
        if (!string.IsNullOrWhiteSpace(q.Category))
            query = query.Where(c => c.Category == q.Category);
        if (!string.IsNullOrWhiteSpace(q.Level))
            query = query.Where(c => c.Level == q.Level);
        if (!string.IsNullOrWhiteSpace(q.Language))
            query = query.Where(c => c.Language == q.Language);
        if (q.PriceMin.HasValue) query = query.Where(c => c.Price >= q.PriceMin);
        if (q.PriceMax.HasValue) query = query.Where(c => c.Price <= q.PriceMax);

        query = q.Sort switch
        {
            "newest"    => query.OrderByDescending(c => c.PublishedAt ?? c.CreatedAt),
            "top"       => query.OrderByDescending(c => c.RatingAvg),
            "popular"   => query.OrderByDescending(c => c.ReviewsCount),
            "priceAsc"  => query.OrderBy(c => c.Price),
            "priceDesc" => query.OrderByDescending(c => c.Price),
            _           => query.OrderByDescending(c => c.CreatedAt)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((q.Page-1)*q.PageSize).Take(q.PageSize)
            .ProjectTo<CourseListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResult<CourseListItemDto>(items, total, q.Page, q.PageSize);
    }

    public async Task<CourseDetailsDto?> GetBySlugAsync(string slug, Guid? studentId = null)
    {
        var course = await _db.Courses.AsNoTracking()
            .Include(c => c.Instructor)
            .Include(c => c.Sections).ThenInclude(s => s.Lessons)
            .Include(c => c.Faqs)
            .SingleOrDefaultAsync(c => c.Slug == slug);

        if (course is null) return null;

        var dto = _mapper.Map<CourseDetailsDto>(course);

        if (studentId.HasValue)
        {
            dto.IsEnrolled = await _db.CoursesStudents.AsNoTracking()
                .AnyAsync(e => e.CourseId == course.Id && e.StudentId == studentId.Value);
            // (optional) Only count active:
            // .AnyAsync(e => e.CourseId == course.Id && e.StudentId == studentId.Value && e.Status == "active");
        }

        return dto;
    }
}

public class InstructorService : IInstructorService
{
    private readonly AppDbContext _db;
    public InstructorService(AppDbContext db) => _db = db;

    public async Task<PagedResult<InstructorListItemDto>> QueryAsync(InstructorQuery q)
    {
        // only real instructors with at least 1 course
        var ins = _db.Users.AsNoTracking()
            .Where(u => u.AccountType == AccountType.Instructor && u.Courses.Any());

        // Project to DTO using SQL-translatable expressions
        var query = ins.Select(u => new InstructorListItemDto
        {
            Id = u.Id,
            // compute a username string the same way you expose it in the API, but SQL-friendly:
            Username = (u.FirstName + "-" + u.LastName),   // keep it simple; add .ToLower() if your API expects lower
            Title = u.Title,
            Bio = u.Bio,
            Email = u.Email,
            AvatarUrl = u.AvatarUrl,
            RatingAvg = u.Courses.Any() ? u.Courses.Average(c => c.RatingAvg) : 0,
            CoursesCount = u.Courses.Count
        });

        var total = await query.CountAsync();

        // Use ORDER BY on fields that EF can translate (e.g., CoursesCount then FirstName/LastName)
        var items = await query
            .OrderByDescending(x => x.CoursesCount)
            .ThenBy(x => x.Username) // safe now because it's computed inside the projection (SQL concat)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync();

        return new PagedResult<InstructorListItemDto>(items, total, q.Page, q.PageSize);
    }

    public async Task<InstructorDetailsDto?> GetByUsernameAsync(string username)
    {
        // Normalize to lower if your frontend uses lower-case usernames
        var uname = username.ToLower();

        var u = await _db.Users.AsNoTracking()
            .Include(x => x.Courses)
            .Where(x => x.AccountType == AccountType.Instructor)
            // Compare with the same expression you use to build username on the fly:
            .SingleOrDefaultAsync(x =>
                (x.FirstName + "-" + x.LastName).ToLower() == uname);

        if (u is null) return null;

        return new InstructorDetailsDto
        {
            Id = u.Id,
            Title = u.Title,
            Bio = u.Bio,
            Username = (u.FirstName + "-" + u.LastName), // return the same computed username
            Email = u.Email,
            Phone = u.Phone,
            AvatarUrl = u.AvatarUrl,
            RatingAvg = u.Courses.Any() ? u.Courses.Average(c => c.RatingAvg) : 0,
            CoursesCount = u.Courses.Count,
            CreatedAt = u.CreatedAt,
            Courses = u.Courses.Select(c => new CourseListItemDto
            {
                Id = c.Id,
                Title = c.Title,
                Slug = c.Slug,
                Price = c.Price,
                ThumbnailUrl = c.ThumbnailUrl,
                RatingAvg = c.RatingAvg,
                ReviewsCount = c.ReviewsCount,
                InstructorName = u.FirstName + " " + u.LastName
            }).ToList()
        };
    }
}