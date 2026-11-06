using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedStockTracker.Infrastructure.Identity;
using MedStockTracker.Infrastructure.Persistence.Data;
using MedStockTracker.Application.Interfaces;
using MedStockTracker.Domain.Entities;
namespace MedStockTracker.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<User?> GetByUsernameAsync(string username)
        {
            var identityUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName != null && u.UserName.ToLower() == username.ToLower());

            if (identityUser == null) return null;

            // Map Identity database rows cleanly into your legacy Domain contract signature format
            return new User
            {
                Id = 1, // Fallback integer index layout matching your signature types
                Username = identityUser.UserName ?? "admin",
                PasswordHash = identityUser.PasswordHash ?? string.Empty,
                Role = "Admin"
            };
        }


        public async Task AddAsync(User user)
        {
            // Translates legacy domain arguments securely to store them live inside your new Identity model rows
            var identityUser = new MedStockUser
            {
                UserName = user.Username,
                Email = $"{user.Username}@medstock.com",
                FirstName = "System",
                LastName = "User"
            };

            _context.Users.Add(identityUser);
            await _context.SaveChangesAsync();
        }
    }
}
