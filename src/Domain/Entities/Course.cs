namespace Domain.Entities;
public class Course
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string Level { get; set; } = default!;
    public string Language { get; set; } = default!;
    public string Instructions { get; set; } = default!;
    public decimal Price { get; set; }
    public int DurationHours { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public decimal RatingAvg { get; set; }
    public int ReviewsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid InstructorId { get; set; }
    public User Instructor { get; set; } = default!;

    public ICollection<Section> Sections { get; set; } = new List<Section>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Faq> Faqs { get; set; } = new List<Faq>();
    public ICollection<CourseStudent> Enrollments { get; set; } = new List<CourseStudent>();
}
