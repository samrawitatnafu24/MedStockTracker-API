using MediatR;
using MedStockTracker.Application.Common;
using MedStockTracker.Application.DTOs.Medicine;

namespace MedStockTracker.Application.Medicines.Queries
{
    public class GetAllMedicinesQuery : IRequest<PagedResult<MedicineDto>>
    {
        public MedicineFilterDto Filter { get; set; }

        public GetAllMedicinesQuery(MedicineFilterDto filter)
        {
            Filter = filter;
        }
    }
}
