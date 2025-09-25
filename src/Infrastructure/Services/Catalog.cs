using Application.Dtos;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public interface ICourseCatalogService
{
    Task<PagedResult<CourseListItemDto>> QueryAsync(CourseQuery query);
    Task<CourseDetailsDto?> GetBySlugAsync(string slug);
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

    public async Task<CourseDetailsDto?> GetBySlugAsync(string slug)
    {
        var course = await _db.Courses.AsNoTracking()
            .Include(c => c.Instructor)
            .Include(c => c.Sections).ThenInclude(s => s.Lessons)
            .Include(c => c.Faqs)
            .SingleOrDefaultAsync(c => c.Slug == slug);

        return course is null ? null : _mapper.Map<CourseDetailsDto>(course);
    }
}

public class InstructorService : IInstructorService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    public InstructorService(AppDbContext db, IMapper mapper){ _db=db; _mapper=mapper; }

    public async Task<PagedResult<InstructorListItemDto>> QueryAsync(InstructorQuery q)
    {
        var ins = _db.Users.AsNoTracking().Where(u => u.Courses.Any());

        if (!string.IsNullOrWhiteSpace(q.Q))
            ins = ins.Where(u => EF.Functions.Like(u.FirstName, $"%{q.Q}%") || EF.Functions.Like(u.LastName, $"%{q.Q}%") || EF.Functions.Like(u.Bio ?? "", $"%{q.Q}%"));

        // category & language -> from courses taught
        if (!string.IsNullOrWhiteSpace(q.Category))
            ins = ins.Where(u => u.Courses.Any(c => c.Category == q.Category));
        if (!string.IsNullOrWhiteSpace(q.Language))
            ins = ins.Where(u => u.Courses.Any(c => c.Language == q.Language));

        // minRating -> avg of their courses
        var query = ins.Select(u => new InstructorListItemDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Title = u.Title,
            Bio = u.Bio,
            Username = u.Username,
            RatingAvg = u.Courses.Any() ? u.Courses.Average(c => c.RatingAvg) : 0,
            CoursesCount = u.Courses.Count
        });

        if (q.MinRating.HasValue) query = query.Where(x => x.RatingAvg >= q.MinRating.Value);

        query = q.Sort switch
        {
            "top"     => query.OrderByDescending(x => x.RatingAvg),
            "popular" => query.OrderByDescending(x => x.CoursesCount),
            "newest"  => query.OrderByDescending(x => x.Id), // naive
            _         => query.OrderByDescending(x => x.RatingAvg)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((q.Page-1)*q.PageSize).Take(q.PageSize).ToListAsync();
        return new PagedResult<InstructorListItemDto>(items, total, q.Page, q.PageSize);
    }

    public async Task<InstructorDetailsDto?> GetByUsernameAsync(string username)
    {
        var u = await _db.Users.AsNoTracking()
            .Include(x => x.Courses)
            .SingleOrDefaultAsync(x => x.Username == username);
        if (u is null) return null;

        return new InstructorDetailsDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Title = u.Title,
            Bio = u.Bio,
            Username = u.Username,
            RatingAvg = u.Courses.Any() ? u.Courses.Average(c => c.RatingAvg) : 0,
            Courses = u.Courses.Select(c => new CourseListItemDto
            {
                Id = c.Id, Title = c.Title, Slug = c.Slug, Price = c.Price,
                ThumbnailUrl = c.ThumbnailUrl, RatingAvg = c.RatingAvg, ReviewsCount = c.ReviewsCount,
                InstructorName = u.FirstName + " " + u.LastName
            }).ToList()
        };
    }
}
