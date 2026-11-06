using Microsoft.AspNetCore.Authorization;

namespace MedStockTracker.Api.Authorization
{
    public sealed class CanEditMedicineRequirement : IAuthorizationRequirement
    {
    }
}
