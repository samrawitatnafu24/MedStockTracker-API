using System;
using System.Collections.Generic;

namespace MedStockTracker.Domain.Entities
{
public class Medicine
{
public int Id { get; set; }


    public string Name { get; set; } = string.Empty;

    public string BatchNumber { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTime ExpiryDate { get; set; }

    public string HospitalBranch { get; set; } = "4 Kilo";

    public int MinimumQuantity { get; set; } = 50;

    public int ReservedQuantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsExpired => ExpiryDate <= DateTime.UtcNow;

    public ICollection<MedicineHold> Holds { get; set; } = new List<MedicineHold>();

    public Category? Category { get; set; }
}


}
