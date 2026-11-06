using Microsoft.AspNetCore.Identity;

namespace MedStockTracker.Infrastructure.Identity
{
    public class MedStockUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Department { get; set; }

        public string HospitalBranch { get; set; } = "4 Kilo";
    }
}
