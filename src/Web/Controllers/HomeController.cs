using Application.Dtos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/home")]
public class HomeController : ControllerBase
{
    private readonly IHomeService _home;
    public HomeController(IHomeService home) => _home = home;

    // GET /api/home/course?take=10
    [HttpGet("course")]
    [AllowAnonymous]
    public async Task<ActionResult<List<HomeCourseDto>>> LatestCourses([FromQuery] int take = 10)
    {
        if (take <= 0 || take > 50) take = 10; // small guardrail
        var items = await _home.GetLatestCoursesAsync(take);
        return Ok(items);
    }
}
