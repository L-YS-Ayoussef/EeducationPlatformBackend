using System.Security.Cryptography;
using System.Text;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public interface IVerificationCodeService
{
    Task<string> CreateAndEmailCodeAsync(string email, string purpose, TimeSpan ttl, Func<string, string> emailBodyFactory);
    Task<bool> VerifyAsync(string email, string purpose, string code, bool consumeOnSuccess = true);
}

public class VerificationCodeService : IVerificationCodeService
{
    private readonly AppDbContext _db;
    private readonly IEmailSender _email;

    public VerificationCodeService(AppDbContext db, IEmailSender email)
    {
        _db = db; _email = email;
    }

    public async Task<string> CreateAndEmailCodeAsync(string email, string purpose, TimeSpan ttl, Func<string, string> emailBodyFactory)
    {
        // throttle: last request within 60s? (optional)
        var since = DateTime.UtcNow.AddSeconds(-60);
        var recent = await _db.EmailCodes.AnyAsync(x => x.Email == email && x.Purpose == purpose && x.CreatedAt >= since);
        if (recent) return "recently_sent";

        var code = GenerateNumericCode(6);
        var codeHash = Hash(code);
        var ec = new EmailCode
        {
            Email = email,
            Purpose = purpose,
            CodeHash = codeHash,
            ExpiresAt = DateTime.UtcNow.Add(ttl),
            CreatedAt = DateTime.UtcNow,
            Attempts = 0
        };
        _db.EmailCodes.Add(ec);
        await _db.SaveChangesAsync();

        var html = emailBodyFactory(code);
        await _email.SendAsync(email, "رمز استعادة كلمة المرور", html);

        return "sent";
    }

    public async Task<bool> VerifyAsync(string email, string purpose, string code, bool consumeOnSuccess = true)
    {
        var now = DateTime.UtcNow;
        var candidate = await _db.EmailCodes
            .Where(x => x.Email == email && x.Purpose == purpose && x.ExpiresAt >= now && x.ConsumedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (candidate is null) return false;

        // simple attempt limiter
        candidate.Attempts += 1;
        if (candidate.Attempts > 5)
        {
            candidate.ExpiresAt = now; // kill it
            await _db.SaveChangesAsync();
            return false;
        }

        var ok = SlowEquals(candidate.CodeHash, Hash(code));
        if (ok && consumeOnSuccess)
        {
            candidate.ConsumedAt = now;
        }
        await _db.SaveChangesAsync();
        return ok;
    }

    private static string GenerateNumericCode(int digits)
    {
        // cryptographically strong numeric code
        var bytes = RandomNumberGenerator.GetBytes(digits);
        var sb = new StringBuilder(digits);
        foreach (var b in bytes) sb.Append((b % 10).ToString());
        return sb.ToString(0, digits);
    }

    private static string Hash(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes); // 64 chars uppercase
    }

    private static bool SlowEquals(string a, string b)
    {
        // constant-time compare
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }
}
