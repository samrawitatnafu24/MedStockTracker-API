using System;

namespace MedStockTracker.Domain.Entities
{
    public class Purchase
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public int SupplierId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public string Status { get; set; } = "Received";
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
        public Medicine? Medicine { get; set; }
        public Supplier? Supplier { get; set; }
    }
}
