using System;

namespace MedStockTracker.Domain.Entities
{
    public class Sale
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string HospitalBranch { get; set; } = string.Empty;
        public DateTime SoldAt { get; set; } = DateTime.UtcNow;
        public Medicine? Medicine { get; set; }
    }
}
