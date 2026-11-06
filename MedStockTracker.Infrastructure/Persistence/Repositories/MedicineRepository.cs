using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedStockTracker.Application.Interfaces;
using MedStockTracker.Application.DTOs.Medicine;
using MedStockTracker.Application.Common;
using MedStockTracker.Infrastructure.Persistence.Data;
using MedStockTracker.Domain.Entities;

namespace MedStockTracker.Infrastructure.Persistence.Repositories
{
    public class MedicineRepository : IMedicineRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicineRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Medicine?> GetByIdAsync(int id)
        {
            return await _context.Medicines.FindAsync(id);
        }

        public async Task<PagedResult<Medicine>> GetAllAsync(
            MedicineFilterDto filter)
        {
            var query = _context.Medicines.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(m =>
                    m.Name.Contains(filter.SearchTerm) ||
                    m.BatchNumber.Contains(filter.SearchTerm));
            }

            if (filter.IsLowStock == true)
            {
                query = query.Where(m => m.Quantity <= 10);
            }

            var totalCount = await query.CountAsync();

            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

            pageSize = Math.Min(pageSize, 100);

            var items = await query
                .OrderBy(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Medicine>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task AddAsync(Medicine medicine)
        {
            await _context.Medicines.AddAsync(medicine);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Medicine medicine)
        {
            _context.Medicines.Update(medicine);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Medicine medicine)
        {
            _context.Medicines.Remove(medicine);
            await _context.SaveChangesAsync();
        }

        public async Task LogStockChangeAsync(
            int medicineId,
            int changeQuantity,
            string actionType)
        {
            var auditTrail = new StockHistory
            {
                MedicineId = medicineId,
                ChangeQuantity = changeQuantity,
                ActionType = actionType
            };

            await _context.StockHistories.AddAsync(auditTrail);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<StockHistory>> GetStockHistoryByIdAsync(
            int medicineId)
        {
            return await _context.StockHistories
                .Where(h => h.MedicineId == medicineId)
                .OrderByDescending(h => h.Id)
                .ToListAsync();
        }
    }
}
