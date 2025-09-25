using Application.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public interface IAuthService
{
    Task<(string token, UserDto user)> RegisterStudentAsync(RegisterDto dto);
    Task<(string token, UserDto user)> LoginAsync(LoginDto dto);
}
public interface IAccountService
{
    Task<UserDto> GetCurrentAsync(Guid userId);
    Task<UserDto> UpdateStudentProfileAsync(Guid userId, UpdateProfileDto dto);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _token;
    private readonly IMapper _mapper;

    public AuthService(AppDbContext db, IPasswordHasher hasher, ITokenService token, IMapper mapper)
    {
        _db = db; _hasher = hasher; _token = token; _mapper = mapper;
    }

    public async Task<(string token, UserDto user)> RegisterStudentAsync(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(x => x.Email == dto.Email))
            throw new InvalidOperationException("Email already registered.");

        var now = DateTime.UtcNow;
        var u = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = _hasher.Hash(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            AccountType = AccountType.Student,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.Users.Add(u);
        await _db.SaveChangesAsync();

        var token = _token.Create(u);
        return (token, _mapper.Map<UserDto>(u));
    }

    public async Task<(string token, UserDto user)> LoginAsync(LoginDto dto)
    {
        var u = await _db.Users.SingleOrDefaultAsync(x => x.Email == dto.Email);
        if (u is null || !_hasher.Verify(u.PasswordHash, dto.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");
        var token = _token.Create(u);
        return (token, _mapper.Map<UserDto>(u));
    }
}

public class AccountService : IAccountService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public AccountService(AppDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

    public async Task<UserDto> GetCurrentAsync(Guid userId)
    {
        var u = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException();
        return _mapper.Map<UserDto>(u);
    }

    public async Task<UserDto> UpdateStudentProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var u = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException();
        u.FirstName = dto.FirstName ?? u.FirstName;
        u.LastName  = dto.LastName  ?? u.LastName;
        u.Phone     = dto.Phone     ?? u.Phone;
        u.AvatarUrl = dto.AvatarUrl ?? u.AvatarUrl;
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return _mapper.Map<UserDto>(u);
    }
}
