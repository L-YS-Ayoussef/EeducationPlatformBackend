namespace Domain.Entities;
public class Attachment
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? PdfUrl { get; set; }
    public decimal? Grade { get; set; }
    public Guid StudentId { get; set; }
    public User Student { get; set; } = default!;
    public int AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
