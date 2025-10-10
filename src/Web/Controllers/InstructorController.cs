using System.Security.Claims;
using Application.Dtos;
using Application.Validation;
using FluentValidation;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/instructor")]
[Authorize(Roles = "Instructor")]
public class InstructorController : ControllerBase
{
    private readonly IInstructorDashboardService _svc; // <-- dashboard service
    public InstructorController(IInstructorDashboardService svc) { _svc = svc; }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet("courses/{courseId:guid}")]
    public async Task<ActionResult<CourseDetailsDto>> GetOwnedCourse(Guid courseId)
    {
        var res = await _svc.GetOwnedCourseAsync(UserId, courseId);
        return Ok(res);
    }

    // 1) Create course (simple)
    [HttpPost("courses/simple")]
    public async Task<ActionResult<CourseDetailsDto>> CreateCourseSimple([FromBody] CourseSimpleUpsertDto dto)
    {
        new CourseSimpleUpsertDtoValidator().ValidateAndThrow(dto);
        var res = await _svc.CreateCourseSimpleAsync(UserId, dto);
        return Ok(res);
    }

    // 2) Edit course (simple)
    [HttpPut("courses/{courseId:guid}/simple")]
    public async Task<ActionResult<CourseDetailsDto>> UpdateCourseSimple(Guid courseId, [FromBody] CourseSimpleUpsertDto dto)
    {
        new CourseSimpleUpsertDtoValidator().ValidateAndThrow(dto);
        var res = await _svc.UpdateCourseSimpleAsync(UserId, courseId, dto);
        return Ok(res);
    }

    // 3) Create section
    [HttpPost("courses/{courseId:guid}/sections")]
    public async Task<ActionResult<SectionDto>> CreateSection(Guid courseId, [FromBody] SectionUpsertDto dto)
    {
        new SectionUpsertDtoValidator().ValidateAndThrow(dto);
        var res = await _svc.CreateSectionAsync(UserId, courseId, dto);
        return Ok(res);
    }

    // 4) Edit section
    [HttpPut("sections/{sectionId:int}")]
    public async Task<ActionResult<SectionDto>> UpdateSection(int sectionId, [FromBody] SectionUpsertDto dto)
    {
        new SectionUpsertDtoValidator().ValidateAndThrow(dto);
        var res = await _svc.UpdateSectionAsync(UserId, sectionId, dto);
        return Ok(res);
    }

    // 5) Create lesson
    [HttpPost("sections/{sectionId:int}/lessons")]
    public async Task<ActionResult<LessonDto>> CreateLesson(int sectionId, [FromBody] LessonUpsertDto dto)
    {
        new LessonUpsertDtoValidator().ValidateAndThrow(dto);
        var res = await _svc.CreateLessonAsync(UserId, sectionId, dto);
        return Ok(res);
    }

    // 6) Edit lesson
    [HttpPut("lessons/{lessonId:int}")]
    public async Task<ActionResult<LessonDto>> UpdateLesson(int lessonId, [FromBody] LessonUpsertDto dto)
    {
        new LessonUpsertDtoValidator().ValidateAndThrow(dto);
        var res = await _svc.UpdateLessonAsync(UserId, lessonId, dto);
        return Ok(res);
    }

    // -----------------------------------------------------------------------------------------------
    [HttpGet("courses/{id:guid}/students")]
    public async Task<ActionResult<List<EnrollmentStudentDto>>> EnrolledStudents(Guid id)
        => Ok(await _svc.GetOwnedCourseStudentsAsync(UserId, id));
    
    [HttpPut("attachments/{attachmentId:int}/grade")]
    public async Task<IActionResult> Grade(int attachmentId, [FromBody] GradeUpdateDto dto)
    {
        new GradeUpdateDtoValidator().ValidateAndThrow(dto);
        await _svc.GradeAttachmentAsync(UserId, attachmentId, dto.Grade);
        return NoContent();
    }

    [HttpDelete("courses/{id:guid}")]
    public async Task<IActionResult> DeleteCourse(Guid id)
    {
        await _svc.DeleteCourseAsync(UserId, id);
        return NoContent();
    }
}
