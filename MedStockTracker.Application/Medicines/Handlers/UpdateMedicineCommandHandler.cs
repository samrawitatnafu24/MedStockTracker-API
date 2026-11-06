using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MedStockTracker.Application.Interfaces;
using MedStockTracker.Application.Medicines.Commands;

namespace MedStockTracker.Application.Medicines.Handlers
{
    public class UpdateMedicineCommandHandler : IRequestHandler<UpdateMedicineCommand, bool>
    {
        private readonly IMedicineRepository _medicineRepository;

        public UpdateMedicineCommandHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<bool> Handle(
            UpdateMedicineCommand request,
            CancellationToken cancellationToken)
        {
            var medicineEntity = await _medicineRepository.GetByIdAsync(request.Id);

            if (medicineEntity == null)
            {
                return false;
            }

            if (medicineEntity.Quantity != request.Quantity)
            {
                var differentialStockAmount =
                    request.Quantity - medicineEntity.Quantity;

                await _medicineRepository.LogStockChangeAsync(
                    medicineEntity.Id,
                    differentialStockAmount,
                    "Inventory batch parameters modified manually via update manager tools."
                );
            }

            medicineEntity.Name = request.Name;
            medicineEntity.BatchNumber = request.BatchNumber;
            medicineEntity.Quantity = request.Quantity;
            medicineEntity.Price = request.Price;
            medicineEntity.ExpiryDate = request.ExpiryDate;
            medicineEntity.HospitalBranch = request.HospitalBranch;
            medicineEntity.MinimumQuantity = request.MinimumQuantity;

            await _medicineRepository.UpdateAsync(medicineEntity);

            return true;
        }
    }
}
