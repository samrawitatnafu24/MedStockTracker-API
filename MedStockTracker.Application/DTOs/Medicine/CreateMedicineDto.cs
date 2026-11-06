using System;
using System.ComponentModel.DataAnnotations;

namespace MedStockTracker.Application.DTOs.Medicine
{
    public class CreateMedicineDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string BatchNumber { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public DateTime ExpiryDate { get; set; }

        [Required]
        public string HospitalBranch { get; set; } = "4 Kilo";

        public int MinimumQuantity { get; set; } = 50;
    }
}
