using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MedStockTracker.Application.Interfaces;

namespace MedStockTracker.Application.Medicines.Queries.Handlers
{

    public class GetStockHistoryQueryHandler : IRequestHandler<GetStockHistoryQuery, IEnumerable<MedStockTracker.Application.DTOs.StockHistoryDto>>
    {
        private readonly IMedicineRepository _repository;

        public GetStockHistoryQueryHandler(IMedicineRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MedStockTracker.Application.DTOs.StockHistoryDto>> Handle(GetStockHistoryQuery request, CancellationToken cancellationToken)
        {
            var auditRecords = await _repository.GetStockHistoryByIdAsync(request.MedicineId);

            if (auditRecords == null)
            {
                return Enumerable.Empty<MedStockTracker.Application.DTOs.StockHistoryDto>();
            }

            return auditRecords.Select(h => new MedStockTracker.Application.DTOs.StockHistoryDto
            {
                Id = h.Id,
                MedicineId = h.MedicineId,
                ChangeQuantity = h.ChangeQuantity,
                ActionType = h.ActionType,
                Timestamp = DateTime.UtcNow
            }).ToList();
        }
    }
}
