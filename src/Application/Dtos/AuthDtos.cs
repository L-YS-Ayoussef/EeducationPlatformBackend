namespace Application.Dtos;

public class RegisterDto
{
    public string FirstName { get; set; } = default!;
    public string LastName  { get; set; } = default!;
    public string Email     { get; set; } = default!;
    public string Password  { get; set; } = default!;
}
public class LoginDto
{
    public string Email    { get; set; } = default!;
    public string Password { get; set; } = default!;
}
public class CourseBriefDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string? ThumbnailUrl { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public string AccountType { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    // NEW:
    public List<CourseBriefDto> EnrolledCourses { get; set; } = new();
    public List<CourseBriefDto> TeachingCourses { get; set; } = new();
}

public class UpdateProfileDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
}
