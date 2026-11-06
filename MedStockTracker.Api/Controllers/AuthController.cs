using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedStockTracker.Application.DTOs.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.RateLimiting;
using MedStockTracker.Domain.Entities;
using MedStockTracker.Infrastructure.Identity;
using MedStockTracker.Infrastructure.Persistence.Data;

namespace MedStockTracker.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<MedStockUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<MedStockUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        public record RegisterRequest(
            string Email,
            string Password,
            string FirstName,
            string LastName,
            string Role);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                return Ok(new { message = "Registration request received." });
            }

            var user = new MedStockUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { errors });
            }

            if (!await _roleManager.RoleExistsAsync(request.Role))
            {
                await _roleManager.CreateAsync(new IdentityRole(request.Role));
            }

            await _userManager.AddToRoleAsync(user, request.Role);

            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto request,
            [FromServices] IWebHostEnvironment env)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    detail = "Payload mapping fields are empty."
                });
            }

            var user =
                await _userManager.FindByEmailAsync(request.Username)
                ?? await _userManager.FindByNameAsync(request.Username);

            if (user == null)
            {
                return Unauthorized(new
                {
                    detail = "Invalid credentials parameters."
                });
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return StatusCode(423, new
                {
                    detail = "Account locked due to multiple failed login attempts. Try again in 15 minutes."
                });
            }

            var validPassword =
                await _userManager.CheckPasswordAsync(user, request.Password);

            if (!validPassword)
            {
                await _userManager.AccessFailedAsync(user);

                return Unauthorized(new
                {
                    detail = "Invalid credentials parameters."
                });
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var jwtToken = CreateJwtToken(user, roles);

            Response.Cookies.Append(
                "medstock_auth",
                jwtToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !env.IsDevelopment(),
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(15)
                });

            await IssueRefreshTokenAsync(user.Id, env);

            var role = roles.FirstOrDefault() ?? "Pharmacist";

            return Ok(new AuthResponseDto
            {
                Token = jwtToken,
                Username = user.UserName ?? user.Email ?? string.Empty,
                Role = role,
                HospitalBranch = user.HospitalBranch
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromServices] IWebHostEnvironment env)
        {
            if (!Request.Cookies.TryGetValue(
                    "medstock_refresh",
                    out var rawToken) ||
                string.IsNullOrWhiteSpace(rawToken))
            {
                return Unauthorized(new
                {
                    detail = "Refresh token is missing."
                });
            }

            var storedToken =
                await _context.RefreshTokens
                    .SingleOrDefaultAsync(
                        token => token.Token == HashToken(rawToken));

            if (storedToken == null)
            {
                return Unauthorized(new
                {
                    detail = "Invalid refresh token."
                });
            }

            if (storedToken.IsUsed ||
                storedToken.IsRevoked ||
                storedToken.ExpiresAt <= DateTime.UtcNow)
            {
                var userTokens =
                    await _context.RefreshTokens
                        .Where(token =>
                            token.UserId == storedToken.UserId &&
                            !token.IsRevoked)
                        .ToListAsync();

                userTokens.ForEach(token => token.IsRevoked = true);

                await _context.SaveChangesAsync();

                Response.Cookies.Delete("medstock_refresh");

                return Unauthorized(new
                {
                    detail = "Refresh token reuse detected. Please sign in again."
                });
            }

            var user =
                await _userManager.FindByIdAsync(storedToken.UserId);

            if (user == null)
            {
                return Unauthorized(new
                {
                    detail = "User no longer exists."
                });
            }

            storedToken.IsUsed = true;
            storedToken.IsRevoked = true;

            var roles = await _userManager.GetRolesAsync(user);
            var jwtToken = CreateJwtToken(user, roles);

            await _context.SaveChangesAsync();
            await IssueRefreshTokenAsync(user.Id, env);

            return Ok(new AuthResponseDto
            {
                Token = jwtToken,
                Username = user.UserName ?? user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Pharmacist",
                HospitalBranch = user.HospitalBranch
            });
        }

        private async Task IssueRefreshTokenAsync(
            string userId,
            IWebHostEnvironment env)
        {
            var rawToken =
                Convert.ToBase64String(
                    RandomNumberGenerator.GetBytes(64));

            await _context.RefreshTokens.AddAsync(
                new RefreshToken
                {
                    Token = HashToken(rawToken),
                    UserId = userId,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                });

            await _context.SaveChangesAsync();

            Response.Cookies.Append(
                "medstock_refresh",
                rawToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !env.IsDevelopment(),
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
        }

        private static string HashToken(string token)
        {
            return Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(token)));
        }

        private string CreateJwtToken(
            MedStockUser user,
            IList<string> roles)
        {
            var key = Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]
                ?? "SuperSecretSecureKeyThatIsLongEnoughAndProtectedSafely2026!");

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        user.Id),

                    new Claim(
                        ClaimTypes.Email,
                        user.Email ?? string.Empty),

                    new Claim(
                        ClaimTypes.Name,
                        user.UserName ?? string.Empty),

                    new Claim(
                        ClaimTypes.Role,
                        roles.FirstOrDefault() ?? "Pharmacist"),

                    new Claim(
                        "hospital_branch",
                        user.HospitalBranch ?? string.Empty)
                }),

                Expires = DateTime.UtcNow.AddMinutes(
                    double.TryParse(
                        _configuration["Jwt:ExpiryMinutes"],
                        out var minutes)
                        ? minutes
                        : 15),

                Issuer =
                    _configuration["Jwt:Issuer"]
                    ?? "MedStockTracker",

                Audience =
                    _configuration["Jwt:Audience"]
                    ?? "MedStockTrackerUsers",

                SigningCredentials =
                    new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();

            return handler.WriteToken(
                handler.CreateToken(descriptor));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new
                {
                    detail = "Not authenticated."
                });
            }

            var identifier =
                User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(identifier))
            {
                return Unauthorized(new
                {
                    detail = "Token is missing identity claims."
                });
            }

            var user =
                await _userManager.FindByEmailAsync(identifier)
                ?? await _userManager.FindByIdAsync(identifier);

            if (user == null)
            {
                return NotFound(new
                {
                    detail = "User not found."
                });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                displayName =
                    $"{user.FirstName} {user.LastName}".Trim(),
                role =
                    roles.FirstOrDefault() ?? "Pharmacist",
                hospitalBranch = user.HospitalBranch
            });
        }
    }
}
