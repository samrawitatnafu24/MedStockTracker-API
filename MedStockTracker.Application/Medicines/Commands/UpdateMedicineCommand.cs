using MediatR;
using System;

namespace MedStockTracker.Application.Medicines.Commands
{
        public class UpdateMedicineCommand : IRequest<bool>
        {
                public int Id { get; set; }

                public string Name { get; set; } = string.Empty;

                public string BatchNumber { get; set; } = string.Empty;

                public int Quantity { get; set; }

                public decimal Price { get; set; }

                public DateTime ExpiryDate { get; set; }

                public string HospitalBranch { get; set; } = "4 Kilo";

                public int MinimumQuantity { get; set; } = 50;

                public UpdateMedicineCommand(
                    int id,
                    string name,
                    string batchNumber,
                    int quantity,
                    decimal price,
                    DateTime expiryDate,
                    string hospitalBranch = "4 Kilo",
                    int minimumQuantity = 50)
                {
                        Id = id;
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
