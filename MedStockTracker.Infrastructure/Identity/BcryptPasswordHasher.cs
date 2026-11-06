using Microsoft.AspNetCore.Identity;

namespace MedStockTracker.Infrastructure.Identity
{
    public sealed class BcryptPasswordHasher : IPasswordHasher<MedStockUser>
    {
        public string HashPassword(MedStockUser user, string password)
            => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        public PasswordVerificationResult VerifyHashedPassword(MedStockUser user, string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword) || !BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword))
                return PasswordVerificationResult.Failed;

            return PasswordVerificationResult.Success;
        }
    }
}
