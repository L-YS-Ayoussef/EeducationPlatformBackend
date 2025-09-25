using System.Security.Claims;
using Application.Dtos;
using Application.Validation;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace Web.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _acct;
    public AccountController(IAccountService acct) { _acct = acct; }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                             User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                             User.FindFirstValue(ClaimTypes.Name) ??
                                             User.FindFirstValue(ClaimTypes.Sid) ??
                                             User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Profile()
        => Ok(await _acct.GetCurrentAsync(GetUserId()));

    [HttpPut("profile")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<UserDto>> Update(UpdateProfileDto dto)
    {
        new UpdateProfileDtoValidator().ValidateAndThrow(dto);
        return Ok(await _acct.UpdateStudentProfileAsync(GetUserId(), dto));
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }
}
