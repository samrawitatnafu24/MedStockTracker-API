using BCrypt.Net;

namespace MedStockTracker.Infrastructure.Services
{
    public class CryptoDemoService
    {
        public string HashUserPassword(string plainText)
        {
            // BCrypt generates a unique salt and prepends it to the hash.
            return BCrypt.Net.BCrypt.HashPassword(plainText, workFactor: 12);
        }

        public bool VerifyUserPassword(string plainText, string hashedDbPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainText, hashedDbPassword);
        }
    }
}
