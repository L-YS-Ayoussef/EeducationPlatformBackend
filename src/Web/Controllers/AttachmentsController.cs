using System.Security.Claims;
using Application.Dtos;
using Application.Validation;
using FluentValidation;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Web.Controllers;

[ApiController]
[Route("api/attachments")]
[Authorize(Roles = "Student")]
public class AttachmentsController : ControllerBase
{
    private readonly IStudentActionsService _svc;
    public AttachmentsController(IStudentActionsService svc) { _svc = svc; }
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] StudentAttachmentCreateDto dto)
    {
        new StudentAttachmentCreateDtoValidator().ValidateAndThrow(dto);
        await _svc.SubmitAttachmentAsync(UserId, dto);
        return NoContent();
    }
}
