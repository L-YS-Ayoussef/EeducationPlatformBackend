namespace Application.Dtos;

public record PagedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);
public record CourseQuery
{
    public string? Q { get; init; }
    public string? Category { get; init; }
    public string? Level { get; init; }
    public string? Language { get; init; }
    public decimal? PriceMin { get; init; }
    public decimal? PriceMax { get; init; }
    public string? Sort { get; init; } = "newest";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}
public record InstructorQuery
{
    public string? Q { get; init; }
    public string? Category { get; init; }
    public string? Language { get; init; }
    public decimal? MinRating { get; init; }
    public string? Sort { get; init; } = "top";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}
