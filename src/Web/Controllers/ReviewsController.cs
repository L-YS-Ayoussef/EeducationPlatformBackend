using System.Security.Claims;
using Application.Dtos;
using Application.Validation;
using FluentValidation;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Web.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize(Roles = "Student")]
public class ReviewsController : ControllerBase
{
    private readonly IStudentActionsService _svc;
    public ReviewsController(IStudentActionsService svc) { _svc = svc; }
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpPost]
    public async Task<IActionResult> CreateOrUpdate([FromBody] ReviewCreateDto dto)
    {
        new ReviewCreateDtoValidator().ValidateAndThrow(dto);
        await _svc.CreateOrUpdateReviewAsync(UserId, dto);
        return NoContent();
    }
}
