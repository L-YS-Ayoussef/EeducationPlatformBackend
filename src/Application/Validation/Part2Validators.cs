using Application.Dtos;
using FluentValidation;

namespace Application.Validation;

public class CourseCreateDtoValidator : AbstractValidator<CourseCreateDto>
{
    public CourseCreateDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(350);
        RuleFor(x => x.ShortDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Level).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Language).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Instructions).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleForEach(x => x.Sections).SetValidator(new SectionCreateDtoValidator());
    }
}
public class SectionCreateDtoValidator : AbstractValidator<SectionCreateDto>
{
    public SectionCreateDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.ShortDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleForEach(x => x.Lessons).SetValidator(new LessonCreateDtoValidator());
        RuleForEach(x => x.Assignments).SetValidator(new AssignmentCreateDtoValidator());
    }
}
public class LessonCreateDtoValidator : AbstractValidator<LessonCreateDto>
{
    public LessonCreateDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}
public class AssignmentCreateDtoValidator : AbstractValidator<AssignmentCreateDto>
{
    public AssignmentCreateDtoValidator()
    {
        RuleFor(x => x.AssignmentNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.MaxScore).InclusiveBetween(1, 1000);
    }
}
public class CourseUpdateDtoValidator : AbstractValidator<CourseUpdateDto>
{
    public CourseUpdateDtoValidator()
    {
        Include(new CourseCreateDtoValidator());
        RuleForEach(x => x.Sections).SetValidator(new SectionUpdateDtoValidator());
    }
}
public class SectionUpdateDtoValidator : AbstractValidator<SectionUpdateDto>
{
    public SectionUpdateDtoValidator()
    {
        Include(new SectionCreateDtoValidator());
        RuleForEach(x => x.Lessons).SetValidator(new LessonUpdateDtoValidator());
        RuleForEach(x => x.Assignments).SetValidator(new AssignmentUpdateDtoValidator());
    }
}
public class LessonUpdateDtoValidator : AbstractValidator<LessonUpdateDto>
{
    public LessonUpdateDtoValidator() => Include(new LessonCreateDtoValidator());
}
public class AssignmentUpdateDtoValidator : AbstractValidator<AssignmentUpdateDto>
{
    public AssignmentUpdateDtoValidator() => Include(new AssignmentCreateDtoValidator());
}

public class EnrollRequestDtoValidator : AbstractValidator<EnrollRequestDto>
{
    public EnrollRequestDtoValidator() => RuleFor(x => x.CourseId).NotEmpty();
}
public class ReviewCreateDtoValidator : AbstractValidator<ReviewCreateDto>
{
    public ReviewCreateDtoValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.ReviewContent).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Rate).InclusiveBetween((byte)1, (byte)5);
    }
}
public class StudentAttachmentCreateDtoValidator : AbstractValidator<StudentAttachmentCreateDto>
{
    public StudentAttachmentCreateDtoValidator()
    {
        RuleFor(x => x.AssignmentId).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}
public class GradeUpdateDtoValidator : AbstractValidator<GradeUpdateDto>
{
    public GradeUpdateDtoValidator() => RuleFor(x => x.Grade).GreaterThanOrEqualTo(0);
}
