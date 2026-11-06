using MedStockTracker.Domain.Entities;

namespace MedStockTracker.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
    }
}
