using Application.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.AccountType, m => m.MapFrom(s => s.AccountType.ToString()));
        CreateMap<User, InstructorSummaryDto>();

        CreateMap<Course, CourseListItemDto>()
            .ForMember(d => d.InstructorName, m => m.MapFrom(s => s.Instructor.FirstName + " " + s.Instructor.LastName));

        CreateMap<Lesson, LessonDto>();
        CreateMap<Section, SectionDto>();
        CreateMap<Faq, FaqDto>();

        CreateMap<Course, CourseDetailsDto>()
            .ForMember(d => d.Instructor, m => m.MapFrom(s => s.Instructor))
            .ForMember(d => d.Sections, m => m.MapFrom(s => s.Sections))
            .ForMember(d => d.Faqs, m => m.MapFrom(s => s.Faqs));

        // Create mappings FROM DTOs -> Entities (for create)
        CreateMap<CourseCreateDto, Course>();
        CreateMap<SectionCreateDto, Section>();
        CreateMap<LessonCreateDto, Lesson>();
        CreateMap<AssignmentCreateDto, Assignment>();
    }
}
