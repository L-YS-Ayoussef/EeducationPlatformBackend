using System.Security.Claims;
using Application.Dtos;
using Application.Validation;
using FluentValidation;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Web.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize(Roles = "Student")]
public class EnrollmentsController : ControllerBase
{
    private readonly IStudentActionsService _svc;
    public EnrollmentsController(IStudentActionsService svc) { _svc = svc; }
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpPost]
    public async Task<IActionResult> Enroll([FromBody] EnrollRequestDto dto)
    {
        new EnrollRequestDtoValidator().ValidateAndThrow(dto);
        await _svc.EnrollAsync(UserId, dto.CourseId);
        return NoContent();
    }

    // Documented choice: soft-cancel (status="cancelled"), not hard delete
    [HttpDelete("{courseId:guid}")]
    public async Task<IActionResult> Cancel(Guid courseId)
    {
        await _svc.CancelEnrollmentAsync(UserId, courseId);
        return NoContent();
    }
}
