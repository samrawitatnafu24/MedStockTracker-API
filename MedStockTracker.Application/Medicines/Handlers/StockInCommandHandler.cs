using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MedStockTracker.Application.Interfaces;
using MedStockTracker.Application.Medicines.Commands;

namespace MedStockTracker.Application.Medicines.Handlers
{
    public class StockInCommandHandler : IRequestHandler<StockInCommand, bool>
    {
        private readonly IMedicineRepository _medicineRepository;

        public StockInCommandHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<bool> Handle(
            StockInCommand request,
            CancellationToken cancellationToken)
        {
            if (request.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Stock In quantity must be greater than zero.");
            }

            var medicine = await _medicineRepository.GetByIdAsync(
                request.MedicineId);

            if (medicine == null)
            {
                return false;
            }

            medicine.Quantity += request.Quantity;

            await _medicineRepository.UpdateAsync(medicine);

            await _medicineRepository.LogStockChangeAsync(
                medicine.Id,
                request.Quantity,
                "STOCK_IN");

            return true;
        }
    }
}
