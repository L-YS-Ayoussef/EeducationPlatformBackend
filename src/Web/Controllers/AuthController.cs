using Application.Dtos;
using Application.Validation;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) { _auth = auth; }

    [HttpPost("register")]
    public async Task<ActionResult<object>> Register(RegisterDto dto)
    {
        new RegisterDtoValidator().ValidateAndThrow(dto);
        var (token, user) = await _auth.RegisterStudentAsync(dto);
        return Ok(new { token, user });
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login(LoginDto dto)
    {
        new LoginDtoValidator().ValidateAndThrow(dto);
        var (token, user) = await _auth.LoginAsync(dto);
        return Ok(new { token, user });
    }

    [HttpPost("forgot")]
    public ActionResult<object> Forgot([FromBody] Dictionary<string,string> body)
    {
        // dev-stub only
        var code = "123456";
        return Ok(new { email = body.GetValueOrDefault("email"), code });
    }

    [HttpPost("verify-code")]
    public ActionResult<object> VerifyCode([FromBody] Dictionary<string,string> body)
    {
        // dev-stub only
        return Ok(new { ok = true });
    }
}
