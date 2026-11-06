using System.Collections.Generic;
using System.Threading.Tasks;
using MedStockTracker.Domain.Entities;
using MedStockTracker.Application.DTOs.Medicine;
using MedStockTracker.Application.Common;

namespace MedStockTracker.Application.Interfaces
{
    public interface IMedicineRepository
    {
        Task<Medicine?> GetByIdAsync(int id);

        Task<PagedResult<Medicine>> GetAllAsync(
            MedicineFilterDto filter);

        Task AddAsync(Medicine medicine);
        Task UpdateAsync(Medicine medicine);
        Task DeleteAsync(Medicine medicine);

        Task LogStockChangeAsync(
            int medicineId,
            int changeQuantity,
            string actionType);

        Task<IEnumerable<StockHistory>> GetStockHistoryByIdAsync(
            int medicineId);
    }
}
