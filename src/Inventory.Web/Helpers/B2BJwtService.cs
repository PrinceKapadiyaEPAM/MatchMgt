using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Inventory.Web.Helpers;

public class B2BJwtService
{
    private readonly IConfiguration _config;

    public B2BJwtService(IConfiguration config) => _config = config;

    public (string token, DateTime expiresAt) GenerateToken(int b2bUserId)
    {
        var secret = _config["JwtSettings:SecretKey"]!;
        var expiryMinutes = _config.GetValue<int>("JwtSettings:ExpiryMinutes", 60);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            claims: [new Claim(ClaimTypes.NameIdentifier, b2bUserId.ToString())],
            expires: expiresAt,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
