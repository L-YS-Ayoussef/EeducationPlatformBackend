namespace Domain.Entities;
public class Review
{
    public int Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid StudentId { get; set; }
    public string ReviewContent { get; set; } = default!;
    public byte Rate { get; set; } // 1..5
    public DateTime CreatedAt { get; set; }

    public Course Course { get; set; } = default!;
    public User Student { get; set; } = default!;
}
