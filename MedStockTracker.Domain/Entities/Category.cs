using System;
using System.Collections.Generic;

namespace MedStockTracker.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
    }
}
