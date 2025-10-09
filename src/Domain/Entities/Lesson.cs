namespace Domain.Entities;
public class Lesson
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public Section Section { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? VideoUrl { get; set; }
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
