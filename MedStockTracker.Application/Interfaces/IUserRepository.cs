using System.Threading.Tasks;
using MedStockTracker.Domain.Entities;

namespace MedStockTracker.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task AddAsync(User user);
    }
}
