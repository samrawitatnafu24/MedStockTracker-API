using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MedStockTracker.Application.Interfaces;
using MedStockTracker.Application.Medicines.Commands;

namespace MedStockTracker.Application.Medicines.Handlers
{
    // ✅ Now perfectly maps since DeleteMedicineCommand officially signs the IRequest<bool> contract
    public class DeleteMedicineCommandHandler : IRequestHandler<DeleteMedicineCommand, bool>
    {
        private readonly IMedicineRepository _medicineRepository;

        public DeleteMedicineCommandHandler(IMedicineRepository medicineRepository)
        {
            _medicineRepository = medicineRepository;
        }

        public async Task<bool> Handle(DeleteMedicineCommand request, CancellationToken cancellationToken)
        {
            var medicineEntity = await _medicineRepository.GetByIdAsync(request.Id);
            if (medicineEntity == null)
            {
                return false;
            }

            await _medicineRepository.LogStockChangeAsync(
                medicineEntity.Id,
                -medicineEntity.Quantity,
                "Medicine tracking record removed entirely from active stock indexes."
            );

            await _medicineRepository.DeleteAsync(medicineEntity);
            return true;
        }
    }
}
