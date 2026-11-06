using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MedStockTracker.Application.DTOs.Medicine;
using MedStockTracker.Application.Interfaces;
using MedStockTracker.Application.Medicines.Commands;
using MedStockTracker.Domain.Entities;

namespace MedStockTracker.Application.Medicines.Handlers
{
    public class CreateMedicineCommandHandler : IRequestHandler<CreateMedicineCommand, MedicineDto>
    {
        private readonly IMedicineRepository _medicineRepository;

        public CreateMedicineCommandHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<MedicineDto> Handle(
            CreateMedicineCommand request,
            CancellationToken cancellationToken)
        {
            var medicineEntity = new Medicine
            {
                Name = request.Name,
                BatchNumber = request.BatchNumber,
                Quantity = request.Quantity,
                Price = request.Price,
                ExpiryDate = request.ExpiryDate,
                HospitalBranch = request.HospitalBranch,
                MinimumQuantity = request.MinimumQuantity
            };

            await _medicineRepository.AddAsync(medicineEntity);

            await _medicineRepository.LogStockChangeAsync(
                medicineEntity.Id,
                request.Quantity,
                $"Initial setup database inventory seed load for batch code: {request.BatchNumber}"
            );

            return new MedicineDto
            {
                Id = medicineEntity.Id,
                Name = medicineEntity.Name,
                BatchNumber = medicineEntity.BatchNumber,
                Quantity = medicineEntity.Quantity,
                Price = medicineEntity.Price,
                ExpiryDate = medicineEntity.ExpiryDate,
                HospitalBranch = medicineEntity.HospitalBranch,
                MinimumQuantity = medicineEntity.MinimumQuantity,
                ReservedQuantity = medicineEntity.ReservedQuantity,
                IsExpired = medicineEntity.IsExpired,
                CreatedAt = medicineEntity.CreatedAt
            };
        }
    }
}
