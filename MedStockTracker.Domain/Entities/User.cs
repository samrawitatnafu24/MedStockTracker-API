using System;

namespace MedStockTracker.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // This determines the user's access level (e.g., "Admin", "Pharmacist")
        public string Role { get; set; } = "Pharmacist";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
