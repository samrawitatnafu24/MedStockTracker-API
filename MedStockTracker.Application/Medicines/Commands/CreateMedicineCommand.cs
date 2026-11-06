using MediatR;
using System;
using MedStockTracker.Application.DTOs.Medicine;

namespace MedStockTracker.Application.Medicines.Commands
{
    public class CreateMedicineCommand : IRequest<MedicineDto>
    {
        public string Name { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string HospitalBranch { get; set; } = "4 Kilo";
        public int MinimumQuantity { get; set; } = 50;

        public CreateMedicineCommand(
            string name,
            string batchNumber,
            int quantity,
            decimal price,
            DateTime expiryDate,
            string hospitalBranch = "4 Kilo",
            int minimumQuantity = 50)
        {
            Name = name;
            BatchNumber = batchNumber;
            Quantity = quantity;
            Price = price;
            ExpiryDate = expiryDate;
            HospitalBranch = hospitalBranch;
            MinimumQuantity = minimumQuantity;
        }
    }
}
