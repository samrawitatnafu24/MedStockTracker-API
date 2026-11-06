using System.Security.Claims;
using MedStockTracker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace MedStockTracker.Api.Authorization
{
    public sealed class CanEditMedicineHandler : AuthorizationHandler<CanEditMedicineRequirement, Medicine>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanEditMedicineRequirement requirement, Medicine medicine)
        {
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var assignedBranch = context.User.FindFirst("hospital_branch")?.Value;
            if (!string.IsNullOrWhiteSpace(assignedBranch) &&
                string.Equals(assignedBranch, medicine.HospitalBranch, StringComparison.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
