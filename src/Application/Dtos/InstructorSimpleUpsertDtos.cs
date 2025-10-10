namespace Application.Dtos;

// ---------- Course ----------
public class CourseFaqItemDto
{
    public string Question { get; set; } = default!;
    public string Answer  { get; set; } = default!;
}

public class CourseSimpleUpsertDto
{
    public string Title { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string Level { get; set; } = default!;
    public string Language { get; set; } = default!;
    public decimal Price { get; set; }
    public int DurationHours { get; set; }
    public string? ThumbnailUrl { get; set; }
    public List<CourseFaqItemDto>? Faqs { get; set; }  // optional
}

// ---------- Section ----------
public class SectionUpsertDto
{
    public string Title { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string Description { get; set; } = default!;
}

// ---------- Lesson ----------
public class LessonUpsertDto
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? VideoUrl { get; set; } // nullable per schema
}

// ---------- Small returns ----------
public class SectionDtoSimple
{
    public int Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = default!;
    public string ShortDescription { get; set; } = default!;
    public string Description { get; set; } = default!;
}

public class LessonDtoSimple
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? VideoUrl { get; set; }
}
