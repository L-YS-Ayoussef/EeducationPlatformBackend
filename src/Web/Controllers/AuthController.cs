using Application.Dtos;
using Application.Validation;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IVerificationCodeService _codes;

    public AuthController(IAuthService auth, IVerificationCodeService codes /*...*/)
    {
        _auth = auth;
        _codes = codes;
    }

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
    [AllowAnonymous]
    public async Task<ActionResult<object>> Forgot([FromBody] Dictionary<string,string> body)
    {
        var email = body.GetValueOrDefault("email")?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email)) return BadRequest(new { error = "email required" });

        // Ensure user exists (don’t reveal if not, to avoid user enumeration)
        // OPTIONAL: If you prefer strict, check user exists and still return 200 either way
        var status = await _codes.CreateAndEmailCodeAsync(
            email,
            purpose: "password_reset",
            ttl: TimeSpan.FromMinutes(10),
            emailBodyFactory: code =>
                $@"<p>مرحبًا،</p>
                <p>رمز استعادة كلمة المرور الخاص بك هو: <b style=""font-size:18px"">{code}</b></p>
                <p>صالح لمدة 10 دقائق.</p>
                <p>فريق Arabic E-Learning</p>"
        );

        return Ok(new { ok = true, status }); // status = "sent" or "recently_sent"
    }

    [HttpPost("verify-code")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> VerifyCode([FromBody] Dictionary<string,string> body)
    {
        var email = body.GetValueOrDefault("email")?.Trim().ToLowerInvariant();
        var code  = body.GetValueOrDefault("code")?.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            return BadRequest(new { error = "email and code required" });

        var ok = await _codes.VerifyAsync(email, "password_reset", code, consumeOnSuccess: true);
        return Ok(new { ok });
    }
}
