using System;

namespace MedStockTracker.Application.DTOs
{
    public class StockHistoryDto
    {
        public int Id { get; set; }
        public int MedicineId { get; set; }
        public int ChangeQuantity { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
