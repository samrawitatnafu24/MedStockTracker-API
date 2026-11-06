using System;

namespace MedStockTracker.Domain.Entities
{
    public class StockHistory
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public int ChangeQuantity { get; set; }
        public string ActionType { get; set; } = string.Empty; // "STOCK_IN" or "STOCK_OUT"
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string ChangedBy { get; set; } = string.Empty;

        // Navigation property to link with the Medicine entity
        public Medicine? Medicine { get; set; }
    }
}
