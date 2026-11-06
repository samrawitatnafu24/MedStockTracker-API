using System.Collections.Generic;
using System.Threading.Tasks;
using MedStockTracker.Application.Interfaces;
using MedStockTracker.Application.DTOs;

namespace MedStockTracker.Application.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IMedicineRepository _repository;

        public MedicineService(IMedicineRepository repository)
        {
            _repository = repository;
        }

        public async Task<MedStockTracker.Application.DTOs.Medicine.MedicineDto?> GetMedicineByIdAsync(int id)
        {
            var medicine = await _repository.GetByIdAsync(id);
            if (medicine == null) return null;

            return new MedStockTracker.Application.DTOs.Medicine.MedicineDto
            {
                Id = medicine.Id,
                Name = medicine.Name,
                BatchNumber = medicine.BatchNumber,
                Quantity = medicine.Quantity,
                Price = medicine.Price,
                ExpiryDate = medicine.ExpiryDate
            };
        }

        public async Task<IEnumerable<MedStockTracker.Application.DTOs.Medicine.MedicineDto>> GetAllMedicinesAsync(
            MedStockTracker.Application.DTOs.Medicine.MedicineFilterDto filter)
        {
            var result = await _repository.GetAllAsync(filter);
            var dtos = new List<MedStockTracker.Application.DTOs.Medicine.MedicineDto>();

            if (result == null) return dtos;

            foreach (var m in result.Items)
            {
                dtos.Add(new MedStockTracker.Application.DTOs.Medicine.MedicineDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    BatchNumber = m.BatchNumber,
                    Quantity = m.Quantity,
                    Price = m.Price,
                    ExpiryDate = m.ExpiryDate
                });
            }

            return dtos;
        }

        public async Task<MedStockTracker.Application.DTOs.Medicine.MedicineDto> CreateMedicineAsync(
            MedStockTracker.Application.DTOs.Medicine.CreateMedicineDto dto)
        {
            throw new System.NotImplementedException(
                "Handled securely via MediatR Commands.");
        }

        public async Task<bool> UpdateMedicineAsync(
            int id,
            MedStockTracker.Application.DTOs.Medicine.CreateMedicineDto dto)
        {
            throw new System.NotImplementedException(
                "Handled securely via MediatR Commands.");
        }

        public async Task<bool> DeleteMedicineAsync(int id)
        {
            throw new System.NotImplementedException(
                "Handled securely via MediatR Commands.");
        }

        public async Task<IEnumerable<MedStockTracker.Application.DTOs.Medicine.MedicineDto>> GetNearExpiryMedicinesAsync()
        {
            throw new System.NotImplementedException(
                "Handled securely via MediatR Queries.");
        }

        public async Task<IEnumerable<StockHistoryDto>> GetStockHistoryAsync(int id)
        {
            throw new System.NotImplementedException(
                "Handled securely via MediatR Queries.");
        }
    }
}
