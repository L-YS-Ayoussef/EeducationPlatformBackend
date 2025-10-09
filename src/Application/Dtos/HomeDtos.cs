namespace Application.Dtos;

public class HomeCourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string Level { get; set; } = default!;
    public int DurationHours { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal Price { get; set; }
    public decimal RatingAvg { get; set; }
    public int ReviewsCount { get; set; }
    public string InstructorName { get; set; } = default!;
}
