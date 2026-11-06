using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MedStockTracker.Application.DTOs.Medicine;
using MedStockTracker.Application.Interfaces;

namespace MedStockTracker.Application.Medicines.Queries.Handlers
{
    public class GetNearExpiryMedicinesQueryHandler :
        IRequestHandler<GetNearExpiryMedicinesQuery, IEnumerable<MedicineDto>>
    {
        private readonly IMedicineRepository _repository;

        public GetNearExpiryMedicinesQueryHandler(
            IMedicineRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MedicineDto>> Handle(
            GetNearExpiryMedicinesQuery request,
            CancellationToken cancellationToken)
        {
            var allMedicines = await _repository.GetAllAsync(
                new MedicineFilterDto
                {
                    Page = 1,
                    PageSize = 100
                });

            var now = DateTime.UtcNow;
            var criticalThreshold = now.AddDays(30);

            return allMedicines.Items
                .Where(m =>
                    m.ExpiryDate <= criticalThreshold &&
                    m.ExpiryDate > now)
                .Select(m => new MedicineDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    BatchNumber = m.BatchNumber,
                    Quantity = m.Quantity,
                    Price = m.Price,
                    ExpiryDate = m.ExpiryDate
                })
                .ToList();
        }
    }
}
