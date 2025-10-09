using Application.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/instructors")]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _svc;

    public InstructorsController(IInstructorService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<InstructorListItemDto>>> List([FromQuery] InstructorQuery q)
        => Ok(await _svc.QueryAsync(q));

    [HttpGet("{username}")]
    public async Task<ActionResult<InstructorDetailsDto>> Get(string username)
    {
        var dto = await _svc.GetByUsernameAsync(username);
        return dto is null ? NotFound() : Ok(dto);
    }
}
