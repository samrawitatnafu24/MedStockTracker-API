using MediatR;

namespace MedStockTracker.Application.Medicines.Commands
{
    public class StockInCommand : IRequest<bool>
    {
        public int MedicineId { get; set; }

        public int Quantity { get; set; }

        public StockInCommand(int medicineId, int quantity)
        {
            MedicineId = medicineId;
            Quantity = quantity;
        }
    }
}
