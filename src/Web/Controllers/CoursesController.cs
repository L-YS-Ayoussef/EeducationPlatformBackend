using Application.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseCatalogService _svc;
    public CoursesController(ICourseCatalogService svc) { _svc = svc; }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CourseListItemDto>>> List([FromQuery] CourseQuery q)
        => Ok(await _svc.QueryAsync(q));

    [HttpGet("{slug}")]
    public async Task<ActionResult<CourseDetailsDto>> GetBySlug(string slug)
    {
        var dto = await _svc.GetBySlugAsync(slug);
        return dto is null ? NotFound() : Ok(dto);
    }
}
