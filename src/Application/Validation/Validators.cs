using Application.Dtos;
using FluentValidation;

namespace Application.Validation;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
    }
}
public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.FirstName).MaximumLength(100).When(x => x.FirstName != null);
        RuleFor(x => x.LastName).MaximumLength(100).When(x => x.LastName != null);
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone != null);
        RuleFor(x => x.AvatarUrl).MaximumLength(500).When(x => x.AvatarUrl != null);
    }
}

// ---- Course ----
public class CourseSimpleUpsertDtoValidator : AbstractValidator<CourseSimpleUpsertDto>
{
    public CourseSimpleUpsertDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ShortDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Level).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Language).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationHours).GreaterThanOrEqualTo(0);
        When(x => x.Faqs != null, () =>
        {
            RuleForEach(x => x.Faqs!).SetValidator(new CourseFaqItemDtoValidator());
        });
    }
}

public class CourseFaqItemDtoValidator : AbstractValidator<CourseFaqItemDto>
{
    public CourseFaqItemDtoValidator()
    {
        RuleFor(x => x.Question).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Answer).NotEmpty().MaximumLength(2000);
    }
}

// ---- Section ----
public class SectionUpsertDtoValidator : AbstractValidator<SectionUpsertDto>
{
    public SectionUpsertDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.ShortDescription).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
    }
}

// ---- Lesson ----
public class LessonUpsertDtoValidator : AbstractValidator<LessonUpsertDto>
{
    public LessonUpsertDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        // VideoUrl is optional; add URL rule if needed
    }
}