using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Pochtachi.Application.Auth;
using Pochtachi.Domain.Common;
using Pochtachi.Domain.Entities;

namespace Pochtachi.Infrastructure.Auth;

public class AuthService(IUnitOfWork uow, IConfiguration configuration) : IAuthService
{
    private readonly PasswordHasher<User> _hasher = new();

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await uow.Repository<User>().ListAsync(u => u.Email == email, ct);
        if (existing.Count > 0)
            throw new AuthException("Bu email bilan foydalanuvchi allaqachon mavjud.");

        var user = new User { Email = email, FullName = request.FullName };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        await uow.Repository<User>().AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);

        return new AuthResponse(CreateToken(user), ToDto(user));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = (await uow.Repository<User>().ListAsync(u => u.Email == email, ct)).FirstOrDefault()
            ?? throw new AuthException("Email yoki parol noto'g'ri.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new AuthException("Email yoki parol noto'g'ri.");

        return new AuthResponse(CreateToken(user), ToDto(user));
    }

    private string CreateToken(User user)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"] ?? "dev-only-placeholder-key-change-me"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("fullName", user.FullName),
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto ToDto(User user) => new(user.Id, user.Email, user.FullName);
}
