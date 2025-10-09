namespace Application.Dtos;

public class CourseListItemDto
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

public class CourseDetailsDto
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
    public decimal RatingAvg { get; set; }
    public int ReviewsCount { get; set; }

    public InstructorSummaryDto Instructor { get; set; } = default!;
    public List<SectionDto> Sections { get; set; } = new();
    public List<FaqDto> Faqs { get; set; } = new();
    public bool IsEnrolled { get; set; }
}
public class SectionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string Description { get; set; } = default!;
    public List<LessonDto> Lessons { get; set; } = new();
}
public class LessonDto
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? VideoUrl { get; set; }
}
public class FaqDto
{
    public int Id { get; set; }
    public string Question { get; set; } = default!;
    public string Answer { get; set; } = default!;
}

public class InstructorSummaryDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string Email { get; set; } = default!;
    public string? AvatarUrl { get; set; }
}
