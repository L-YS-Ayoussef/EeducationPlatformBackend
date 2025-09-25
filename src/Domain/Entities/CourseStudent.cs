namespace Domain.Entities;
public class CourseStudent
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public string Status { get; set; } = "active"; // active|completed|cancelled
    public DateTime EnrolledAt { get; set; }

    public User Student { get; set; } = default!;
    public Course Course { get; set; } = default!;
}
