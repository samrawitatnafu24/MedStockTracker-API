using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MedStockTracker.Application.DTOs.Medicine;
using MedStockTracker.Application.Interfaces;

namespace MedStockTracker.Application.Medicines.Queries.Handlers
{
    public class GetMedicineByIdQueryHandler :
        IRequestHandler<GetMedicineByIdQuery, MedicineDto?>
    {
        private readonly IMedicineRepository _repository;

        public GetMedicineByIdQueryHandler(IMedicineRepository repository)
        {
            _repository = repository;
        }

        public async Task<MedicineDto?> Handle(
            GetMedicineByIdQuery request,
            CancellationToken cancellationToken)
        {
            var medicine = await _repository.GetByIdAsync(request.Id);

            if (medicine == null)
            {
                return null;
            }

            return new MedicineDto
            {
                Id = medicine.Id,
                Name = medicine.Name,
                BatchNumber = medicine.BatchNumber,
                Quantity = medicine.Quantity,
                Price = medicine.Price,
                ExpiryDate = medicine.ExpiryDate,
                HospitalBranch = medicine.HospitalBranch,
                MinimumQuantity = medicine.MinimumQuantity,
                ReservedQuantity = medicine.ReservedQuantity,
                IsExpired = medicine.IsExpired,
                CreatedAt = medicine.CreatedAt
            };
        }
    }
}
