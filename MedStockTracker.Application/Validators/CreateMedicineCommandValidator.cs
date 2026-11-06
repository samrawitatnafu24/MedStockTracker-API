using FluentValidation;
using MedStockTracker.Application.Medicines.Commands;

namespace MedStockTracker.Application.Validators
{
    public class CreateMedicineCommandValidator : AbstractValidator<CreateMedicineCommand>
    {
        public CreateMedicineCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Medicine name is required.")
                .MaximumLength(100).WithMessage("Medicine name cannot exceed 100 characters.");

            RuleFor(x => x.BatchNumber)
                .NotEmpty().WithMessage("Batch number is required.")
                .Matches(@"^[A-Z0-9-]+$").WithMessage("Batch number must contain only uppercase letters, numbers, and hyphens.");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Initial quantity cannot be negative.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("Cannot register expired medicine. Expiry date must be in the future.");
        }
    }
}
