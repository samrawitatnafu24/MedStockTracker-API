using System;

namespace MedStockTracker.Application.DTOs.Medicine
{
    public class MedicineDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string BatchNumber { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public DateTime ExpiryDate { get; set; }

        public string HospitalBranch { get; set; } = string.Empty;

        public int MinimumQuantity { get; set; }

        public int ReservedQuantity { get; set; }

        public bool IsExpired { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
