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

    [HttpPost("courses")]
    public async Task<ActionResult<CourseDetailsDto>> CreateCourse([FromBody] CourseCreateDto dto)
    {
        new CourseCreateDtoValidator().ValidateAndThrow(dto);
        var res = await _svc.CreateCourseAsync(UserId, dto);
        return Ok(res);
    }

    [HttpPut("courses/{id:guid}")]
    public async Task<ActionResult<CourseDetailsDto>> UpdateCourse(Guid id, [FromBody] CourseUpdateDto dto)
    {
        new CourseUpdateDtoValidator().ValidateAndThrow(dto);
        var res = await _svc.UpdateCourseAsync(UserId, id, dto);
        return Ok(res);
    }

    // ****************************
    [HttpGet("courses")]
    public async Task<ActionResult<PagedResult<CourseListItemDto>>> MyCourses([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        => Ok(await _svc.ListOwnedCoursesAsync(UserId, page, pageSize));

    [HttpGet("courses/{id:guid}")]
    public async Task<ActionResult<CourseDetailsDto>> MyCourse(Guid id)
        => Ok(await _svc.GetOwnedCourseAsync(UserId, id));
    // ****************************

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
