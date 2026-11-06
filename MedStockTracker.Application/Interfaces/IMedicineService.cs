using System.Collections.Generic;
using System.Threading.Tasks;
using MedStockTracker.Application.DTOs; // ✅ Force it to look in the main DTOs namespace

namespace MedStockTracker.Application.Interfaces
{
    public interface IMedicineService
    {
        Task<MedStockTracker.Application.DTOs.Medicine.MedicineDto?> GetMedicineByIdAsync(int id);
        Task<IEnumerable<MedStockTracker.Application.DTOs.Medicine.MedicineDto>> GetAllMedicinesAsync(MedStockTracker.Application.DTOs.Medicine.MedicineFilterDto filter);
        Task<MedStockTracker.Application.DTOs.Medicine.MedicineDto> CreateMedicineAsync(MedStockTracker.Application.DTOs.Medicine.CreateMedicineDto dto);
        Task<bool> UpdateMedicineAsync(int id, MedStockTracker.Application.DTOs.Medicine.CreateMedicineDto dto);
        Task<bool> DeleteMedicineAsync(int id);
        Task<IEnumerable<MedStockTracker.Application.DTOs.Medicine.MedicineDto>> GetNearExpiryMedicinesAsync();
        Task<IEnumerable<StockHistoryDto>> GetStockHistoryAsync(int id);
    }
}
