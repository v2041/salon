using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

public static class JwtTokenGenerator
{
    public static string Create(User user, IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt");
        var now = DateTime.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("sub", user.Id.ToString()) }),
            Issuer = jwt["Issuer"],
            Audience = jwt["Audience"],
            IssuedAt = now,
            Expires = now.AddMinutes(int.Parse(jwt["ExpiryMinutes"] ?? "60")),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
                SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object> { ["role"] = user.Role.ToString() }
        };
        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(descriptor);
    }
}
