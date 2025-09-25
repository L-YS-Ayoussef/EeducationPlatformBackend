namespace Application.Dtos;

public class InstructorListItemDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string Username { get; set; } = default!;
    public decimal RatingAvg { get; set; }
    public int CoursesCount { get; set; }
}
public class InstructorDetailsDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public string Username { get; set; } = default!;
    public decimal RatingAvg { get; set; }
    public List<CourseListItemDto> Courses { get; set; } = new();
}
