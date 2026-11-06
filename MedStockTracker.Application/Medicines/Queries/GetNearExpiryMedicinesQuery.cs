using System.Collections.Generic;
using MediatR;
using MedStockTracker.Application.DTOs.Medicine;

namespace MedStockTracker.Application.Medicines.Queries
{
    public class GetNearExpiryMedicinesQuery : IRequest<IEnumerable<MedicineDto>>
    {
        // No arguments needed to check global system expiration dates
    }
}
