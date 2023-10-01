using FTask.Repository.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FTask.API.Service;

public interface IJWTTokenService<T> where T : IdentityUser<Guid>
{
    string CreateToken(T user, IEnumerable<string> roleNames);
}

public class JWTTokenService<T> : IJWTTokenService<T> where T : IdentityUser<Guid>
{
    private readonly IConfiguration _configuration;
    private readonly IKeyManager  _keyManager;

    public JWTTokenService(IConfiguration configuration, IKeyManager keyManager)
    {
        _configuration = configuration;
        _keyManager = keyManager;
    }

    public string CreateToken(T user, IEnumerable<string> roleNames)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
        };

        if (roleNames!.Count() > 0)
        {
            foreach(var item in roleNames!)
            {
                claims.Add(new Claim("scope", item));
            }
        }

        var key = new RsaSecurityKey(_keyManager.RsaKey);
        var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds); 

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
}
