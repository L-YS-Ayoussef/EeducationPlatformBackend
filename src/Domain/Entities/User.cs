using Domain.Enums;

namespace Domain.Entities;
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public AccountType AccountType { get; set; }
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Computed Username/Slug for instructor profile URLs (First-Last in Arabic-safe slug)
    public string Username => $"{FirstName}-{LastName}".Trim().Replace(' ', '-');
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
