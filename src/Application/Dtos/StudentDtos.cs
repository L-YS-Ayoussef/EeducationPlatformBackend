namespace Application.Dtos;

public class EnrollRequestDto
{
    public Guid CourseId { get; set; }
}

public class EnrollmentItemDto
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = default!;
    public string CourseSlug { get; set; } = default!;
    public string? ThumbnailUrl { get; set; }
    public string Status { get; set; } = default!;
    public DateTime EnrolledAt { get; set; }
}

public class ReviewCreateDto
{
    public Guid CourseId { get; set; }
    public string ReviewContent { get; set; } = default!;
    public byte Rate { get; set; } // 1..5
}

public class StudentAttachmentCreateDto
{
    public int AssignmentId { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? PdfUrl { get; set; }
}

public class GradeUpdateDto
{
    public decimal Grade { get; set; }
}
