namespace Domain.Entities;
public class Faq
{
    public int Id { get; set; }
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = default!;
    public string Question { get; set; } = default!;
    public string Answer { get; set; } = default!;
}
