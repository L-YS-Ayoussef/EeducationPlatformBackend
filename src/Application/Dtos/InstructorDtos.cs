namespace Application.Dtos;

public class InstructorListItemDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string Email { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public decimal RatingAvg { get; set; }
    public int CoursesCount { get; set; }
}
public class InstructorDetailsDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public decimal RatingAvg { get; set; }
    public int CoursesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CourseListItemDto> Courses { get; set; } = new();
}
