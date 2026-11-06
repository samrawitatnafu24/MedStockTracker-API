using System.Collections.Generic;
using MediatR;
using MedStockTracker.Application.DTOs;

namespace MedStockTracker.Application.Medicines.Queries
{

    public class GetStockHistoryQuery : IRequest<IEnumerable<StockHistoryDto>>
    {
        public int MedicineId { get; set; }

        public GetStockHistoryQuery(int medicineId)
        {
            MedicineId = medicineId;
        }
    }
}
