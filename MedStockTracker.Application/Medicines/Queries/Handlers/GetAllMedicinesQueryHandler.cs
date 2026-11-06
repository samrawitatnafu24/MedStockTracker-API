using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MedStockTracker.Application.Common;
using MedStockTracker.Application.DTOs.Medicine;
using MedStockTracker.Application.Interfaces;

namespace MedStockTracker.Application.Medicines.Queries.Handlers
{
    public class GetAllMedicinesQueryHandler :
        IRequestHandler<GetAllMedicinesQuery, PagedResult<MedicineDto>>
    {
        private readonly IMedicineRepository _repository;

        public GetAllMedicinesQueryHandler(
            IMedicineRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<MedicineDto>> Handle(
            GetAllMedicinesQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _repository.GetAllAsync(request.Filter);

            return new PagedResult<MedicineDto>
            {
                Items = result.Items.Select(m => new MedicineDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    BatchNumber = m.BatchNumber,
                    Quantity = m.Quantity,
                    Price = m.Price,
                    ExpiryDate = m.ExpiryDate,
                    HospitalBranch = m.HospitalBranch,
                    MinimumQuantity = m.MinimumQuantity,
                    ReservedQuantity = m.ReservedQuantity,
                    IsExpired = m.IsExpired,
                    CreatedAt = m.CreatedAt
                }).ToList(),

                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }
    }
}
