namespace MedStockTracker.Application.DTOs.Medicine
{
    public class MedicineFilterDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsLowStock { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
