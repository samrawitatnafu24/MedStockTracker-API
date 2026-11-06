using MediatR;
using MedStockTracker.Application.DTOs.Medicine;

namespace MedStockTracker.Application.Medicines.Queries
{
    public class GetMedicineByIdQuery : IRequest<MedicineDto?>
    {
        public int Id { get; set; }
        public GetMedicineByIdQuery(int id) => Id = id;
    }
}
