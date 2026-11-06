using FluentValidation;
using System;
using MedStockTracker.Application.Medicines.Commands;

namespace MedStockTracker.Application.Validators
{
    public class UpdateMedicineCommandValidator : AbstractValidator<UpdateMedicineCommand>
    {
        public UpdateMedicineCommandValidator()
        {

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Medicine ID is required for updates.")
                .GreaterThan(0).WithMessage("Medicine ID must be a positive integer.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Medicine name cannot be empty.")
                .MaximumLength(150).WithMessage("Medicine name cannot exceed 150 characters.");

            RuleFor(x => x.BatchNumber)
                .NotEmpty().WithMessage("Batch number is required.");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.UtcNow).WithMessage("Expiry date must be a future date.");
        }
    }
}
