namespace Application.Dtos;

public class CourseCreateDto
{
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string Level { get; set; } = default!;
    public string Language { get; set; } = "ar";
    public string Instructions { get; set; } = default!;
    public decimal Price { get; set; }
    public int DurationHours { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public List<SectionCreateDto> Sections { get; set; } = new();
}

public class SectionCreateDto
{
    public string Title { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string Description { get; set; } = default!;
    public List<LessonCreateDto> Lessons { get; set; } = new();
    public List<AssignmentCreateDto> Assignments { get; set; } = new();
}

public class LessonCreateDto
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? VideoUrl { get; set; }
}

public class AssignmentCreateDto
{
    public int AssignmentNumber { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? PdfUrl { get; set; }
    public decimal MaxScore { get; set; }
}

// Update DTOs include IDs to sync add/update/delete
public class CourseUpdateDto : CourseCreateDto
{
    public new List<SectionUpdateDto> Sections { get; set; } = new();
}

public class SectionUpdateDto : SectionCreateDto
{
    public int? Id { get; set; }
    public new List<LessonUpdateDto> Lessons { get; set; } = new();
    public new List<AssignmentUpdateDto> Assignments { get; set; } = new();
}
public class LessonUpdateDto : LessonCreateDto
{
    public int? Id { get; set; }
}
public class AssignmentUpdateDto : AssignmentCreateDto
{
    public int? Id { get; set; }
}

public class EnrollmentStudentDto
{
    public Guid StudentId { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime EnrolledAt { get; set; }
    public int ProgressPct { get; set; } // 0 for now
}
