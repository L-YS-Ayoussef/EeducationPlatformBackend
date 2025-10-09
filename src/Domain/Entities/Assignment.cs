namespace Domain.Entities;
public class Assignment
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = default!;
    public int AssignmentNumber { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? PdfUrl { get; set; }
    public decimal MaxScore { get; set; }
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
