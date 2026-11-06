using System;
using System.ComponentModel.DataAnnotations;

namespace MedStockTracker.Application.DTOs.Medicine;

public class MedicineHoldDto
{
    [Required, StringLength(160)] public string PatientName { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int Quantity { get; set; }
    public string? Note { get; set; }
    public int DurationHours { get; set; } = 2;
}
