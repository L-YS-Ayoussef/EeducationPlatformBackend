namespace Domain.Entities;
public class Section
{
    public int Id { get; set; }
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string Description { get; set; } = default!;
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
