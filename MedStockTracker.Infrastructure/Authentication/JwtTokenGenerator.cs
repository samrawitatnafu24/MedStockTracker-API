using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MedStockTracker.Application.Interfaces;
using MedStockTracker.Domain.Entities;

namespace MedStockTracker.Infrastructure.Authentication
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]
                ?? "SuperSecretSecureKeyThatIsLongEnoughAndProtectedSafely2026!");

            var expiryMinutes =
                double.TryParse(
                    _configuration["Jwt:ExpiryMinutes"],
                    out var minutes)
                    ? minutes
                    : 15;

            var claims = new ClaimsIdentity(new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Username ?? string.Empty),

                new Claim(
                    ClaimTypes.Role,
                    user.Role ?? "Pharmacist")
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,

                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),

                Issuer =
                    _configuration["Jwt:Issuer"]
                    ?? "MedStockTracker",

                Audience =
                    _configuration["Jwt:Audience"]
                    ?? "MedStockTrackerUsers",

                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
