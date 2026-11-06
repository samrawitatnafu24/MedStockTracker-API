using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedStockTracker.Domain.Entities;
using MedStockTracker.Infrastructure.Persistence.Data;

namespace MedStockTracker.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/inventory-data")]
    public class InventoryDataController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public InventoryDataController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // ==================== CATEGORIES ====================

        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories() =>
            Ok(await context.Categories.AsNoTracking()
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    MedicineCount = c.Medicines.Count,
                    c.CreatedAt
                })
                .OrderBy(c => c.Name)
                .ToListAsync());

        [HttpPost("categories")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] Category category)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.Name))
                return BadRequest(new { message = "Category name is required." });

            var name = category.Name.Trim();

            if (await context.Categories.AnyAsync(c =>
                c.Name.ToLower() == name.ToLower()))
                return Conflict(new { message = "A category with this name already exists." });

            category.Id = 0;
            category.Name = name;
            category.Description = category.Description?.Trim() ?? "";
            category.CreatedAt = DateTime.UtcNow;
            category.Medicines = new List<Medicine>();

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            return Ok(new
            {
                category.Id,
                category.Name,
                category.Description,
                MedicineCount = 0,
                category.CreatedAt
            });
        }

        [HttpPut("categories/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(
            int id, [FromBody] Category updated)
        {
            if (updated == null || string.IsNullOrWhiteSpace(updated.Name))
                return BadRequest(new { message = "Category name is required." });

            var category = await context.Categories.FindAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            var name = updated.Name.Trim();

            if (await context.Categories.AnyAsync(c =>
                c.Id != id && c.Name.ToLower() == name.ToLower()))
                return Conflict(new { message = "A category with this name already exists." });

            category.Name = name;
            category.Description = updated.Description?.Trim() ?? "";

            await context.SaveChangesAsync();

            return Ok(new
            {
                category.Id,
                category.Name,
                category.Description,
                MedicineCount = await context.Medicines.CountAsync(
                    m => m.CategoryId == id),
                category.CreatedAt
            });
        }

        [HttpDelete("categories/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await context.Categories.FindAsync(id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            var hasMedicines = await context.Medicines.AnyAsync(
                m => m.CategoryId == id);

            if (hasMedicines)
                return Conflict(new
                {
                    message = "This category cannot be deleted because medicines are assigned to it."
                });

            context.Categories.Remove(category);
            await context.SaveChangesAsync();

            return NoContent();
        }

        // ==================== SUPPLIERS ====================

        [HttpGet("suppliers")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSuppliers() =>
            Ok(await context.Suppliers.AsNoTracking()
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.ContactPerson,
                    s.Phone,
                    s.Email,
                    s.Address,
                    PurchaseCount = s.Purchases.Count,
                    s.CreatedAt
                })
                .OrderBy(s => s.Name)
                .ToListAsync());

        [HttpPost("suppliers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSupplier([FromBody] Supplier supplier)
        {
            if (supplier == null || string.IsNullOrWhiteSpace(supplier.Name))
                return BadRequest(new { message = "Supplier name is required." });

            supplier.Id = 0;
            supplier.Name = supplier.Name.Trim();
            supplier.ContactPerson = supplier.ContactPerson?.Trim() ?? "";
            supplier.Phone = supplier.Phone?.Trim() ?? "";
            supplier.Email = supplier.Email?.Trim() ?? "";
            supplier.Address = supplier.Address?.Trim() ?? "";
            supplier.CreatedAt = DateTime.UtcNow;

            context.Suppliers.Add(supplier);
            await context.SaveChangesAsync();

            return Ok(new
            {
                supplier.Id,
                supplier.Name,
                supplier.ContactPerson,
                supplier.Phone,
                supplier.Email,
                supplier.Address,
                PurchaseCount = 0,
                supplier.CreatedAt
            });
        }

        [HttpPut("suppliers/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSupplier(
            int id, [FromBody] Supplier updated)
        {
            if (updated == null || string.IsNullOrWhiteSpace(updated.Name))
                return BadRequest(new { message = "Supplier name is required." });

            var supplier = await context.Suppliers.FindAsync(id);

            if (supplier == null)
                return NotFound(new { message = "Supplier not found." });

            supplier.Name = updated.Name.Trim();
            supplier.ContactPerson = updated.ContactPerson?.Trim() ?? "";
            supplier.Phone = updated.Phone?.Trim() ?? "";
            supplier.Email = updated.Email?.Trim() ?? "";
            supplier.Address = updated.Address?.Trim() ?? "";

            await context.SaveChangesAsync();

            return Ok(new
            {
                supplier.Id,
                supplier.Name,
                supplier.ContactPerson,
                supplier.Phone,
                supplier.Email,
                supplier.Address,
                PurchaseCount = await context.Purchases.CountAsync(
                    p => p.SupplierId == id),
                supplier.CreatedAt
            });
        }

        [HttpDelete("suppliers/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await context.Suppliers.FindAsync(id);

            if (supplier == null)
                return NotFound(new { message = "Supplier not found." });

            var hasPurchases = await context.Purchases.AnyAsync(
                p => p.SupplierId == id);

            if (hasPurchases)
                return Conflict(new
                {
                    message = "This supplier cannot be deleted because purchase history exists."
                });

            context.Suppliers.Remove(supplier);
            await context.SaveChangesAsync();

            return NoContent();
        }

        // ==================== PURCHASES ====================

        [HttpGet("purchases")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPurchases() =>
            Ok(await context.Purchases.AsNoTracking()
                .Select(p => new
                {
                    p.Id,
                    p.MedicineId,
                    MedicineName = p.Medicine != null
                        ? p.Medicine.Name : "Unknown",
                    p.SupplierId,
                    SupplierName = p.Supplier != null
                        ? p.Supplier.Name : "Direct Supply",
                    p.Quantity,
                    p.UnitCost,
                    p.TotalCost,
                    p.Status,
                    p.PurchasedAt
                })
                .OrderByDescending(p => p.PurchasedAt)
                .ToListAsync());

        [HttpPost("purchases")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePurchase(
            [FromBody] Purchase purchase)
        {
            if (purchase == null || purchase.MedicineId <= 0 ||
                purchase.SupplierId <= 0 || purchase.Quantity <= 0 ||
                purchase.UnitCost < 0)
                return BadRequest(new
                {
                    message = "Please provide valid purchase details."
                });

            var medicine = await context.Medicines.FindAsync(
                purchase.MedicineId);

            if (medicine == null)
                return NotFound(new { message = "Medicine not found." });

            var supplier = await context.Suppliers.FindAsync(
                purchase.SupplierId);

            if (supplier == null)
                return NotFound(new { message = "Supplier not found." });

            purchase.Id = 0;
            purchase.TotalCost = purchase.Quantity * purchase.UnitCost;
            purchase.PurchasedAt = DateTime.UtcNow;
            purchase.Status = string.IsNullOrWhiteSpace(purchase.Status)
                ? "Received" : purchase.Status.Trim();

            context.Purchases.Add(purchase);

            if (string.Equals(
                purchase.Status, "Received",
                StringComparison.OrdinalIgnoreCase))
            {
                medicine.Quantity += purchase.Quantity;

                context.StockHistories.Add(new StockHistory
                {
                    MedicineId = medicine.Id,
                    ChangeQuantity = purchase.Quantity,
                    ActionType = "PURCHASE_RECEIVED",
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = User.Identity?.Name ?? "System"
                });
            }

            await context.SaveChangesAsync();

            return Ok(new
            {
                purchase.Id,
                purchase.MedicineId,
                MedicineName = medicine.Name,
                purchase.SupplierId,
                SupplierName = supplier.Name,
                purchase.Quantity,
                purchase.UnitCost,
                purchase.TotalCost,
                purchase.Status,
                purchase.PurchasedAt
            });
        }

        [HttpPut("purchases/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePurchase(
            int id, [FromBody] Purchase updated)
        {
            if (updated == null || updated.Quantity <= 0 ||
                updated.UnitCost < 0 || updated.MedicineId <= 0 ||
                updated.SupplierId <= 0)
                return BadRequest(new
                {
                    message = "Please provide valid purchase details."
                });

            var purchase = await context.Purchases.FindAsync(id);

            if (purchase == null)
                return NotFound(new { message = "Purchase not found." });

            if (purchase.MedicineId != updated.MedicineId)
                return BadRequest(new
                {
                    message = "Changing the medicine of an existing purchase is not allowed."
                });

            var medicine = await context.Medicines.FindAsync(
                purchase.MedicineId);

            var supplier = await context.Suppliers.FindAsync(
                updated.SupplierId);

            if (medicine == null || supplier == null)
                return NotFound(new
                {
                    message = "Medicine or supplier not found."
                });

            var wasReceived = string.Equals(
                purchase.Status, "Received",
                StringComparison.OrdinalIgnoreCase);

            var newStatus = string.IsNullOrWhiteSpace(updated.Status)
                ? purchase.Status : updated.Status.Trim();

            var willBeReceived = string.Equals(
                newStatus, "Received",
                StringComparison.OrdinalIgnoreCase);

            var oldStockEffect = wasReceived ? purchase.Quantity : 0;
            var newStockEffect = willBeReceived ? updated.Quantity : 0;
            var stockDifference = newStockEffect - oldStockEffect;

            if (stockDifference < 0 &&
                medicine.Quantity - medicine.ReservedQuantity < -stockDifference)
                return Conflict(new
                {
                    message = "Not enough available stock to reduce this purchase."
                });

            medicine.Quantity += stockDifference;

            if (stockDifference != 0)
            {
                context.StockHistories.Add(new StockHistory
                {
                    MedicineId = medicine.Id,
                    ChangeQuantity = stockDifference,
                    ActionType = "PURCHASE_UPDATED",
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = User.Identity?.Name ?? "System"
                });
            }

            purchase.SupplierId = updated.SupplierId;
            purchase.Quantity = updated.Quantity;
            purchase.UnitCost = updated.UnitCost;
            purchase.TotalCost = updated.Quantity * updated.UnitCost;
            purchase.Status = newStatus;

            await context.SaveChangesAsync();

            return Ok(new
            {
                purchase.Id,
                purchase.MedicineId,
                MedicineName = medicine.Name,
                purchase.SupplierId,
                SupplierName = supplier.Name,
                purchase.Quantity,
                purchase.UnitCost,
                purchase.TotalCost,
                purchase.Status,
                purchase.PurchasedAt
            });
        }

        [HttpDelete("purchases/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            var purchase = await context.Purchases.FindAsync(id);

            if (purchase == null)
                return NotFound(new { message = "Purchase not found." });

            if (string.Equals(
                purchase.Status, "Received",
                StringComparison.OrdinalIgnoreCase))
            {
                var medicine = await context.Medicines.FindAsync(
                    purchase.MedicineId);

                if (medicine == null)
                    return NotFound(new { message = "Medicine not found." });

                if (medicine.Quantity - medicine.ReservedQuantity <
                    purchase.Quantity)
                    return Conflict(new
                    {
                        message = "Cannot delete this purchase because some of its stock is already reserved or unavailable."
                    });

                medicine.Quantity -= purchase.Quantity;

                context.StockHistories.Add(new StockHistory
                {
                    MedicineId = medicine.Id,
                    ChangeQuantity = -purchase.Quantity,
                    ActionType = "PURCHASE_DELETED",
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = User.Identity?.Name ?? "System"
                });
            }

            context.Purchases.Remove(purchase);
            await context.SaveChangesAsync();

            return NoContent();
        }

        // ==================== SALES ====================

        [HttpGet("sales")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSales() =>
            Ok(await context.Sales.AsNoTracking()
                .Select(s => new
                {
                    s.Id,
                    s.MedicineId,
                    MedicineName = s.Medicine != null
                        ? s.Medicine.Name : "Unknown",
                    s.Quantity,
                    s.UnitPrice,
                    s.TotalAmount,
                    s.CustomerName,
                    s.HospitalBranch,
                    s.SoldAt
                })
                .OrderByDescending(s => s.SoldAt)
                .ToListAsync());

        [HttpPost("sales")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSale([FromBody] Sale sale)
        {
            if (sale == null || sale.MedicineId <= 0 ||
                sale.Quantity <= 0 || sale.UnitPrice < 0)
                return BadRequest(new
                {
                    message = "Please provide valid sale details."
                });

            var medicine = await context.Medicines.FindAsync(
                sale.MedicineId);

            if (medicine == null)
                return NotFound(new { message = "Medicine not found." });

            var availableStock =
                medicine.Quantity - medicine.ReservedQuantity;

            if (availableStock < sale.Quantity)
                return Conflict(new
                {
                    message = "Not enough available stock."
                });

            sale.Id = 0;
            sale.TotalAmount = sale.Quantity * sale.UnitPrice;
            sale.SoldAt = DateTime.UtcNow;
            sale.CustomerName = sale.CustomerName?.Trim() ?? "";
            sale.HospitalBranch = string.IsNullOrWhiteSpace(
                sale.HospitalBranch)
                ? medicine.HospitalBranch
                : sale.HospitalBranch.Trim();

            medicine.Quantity -= sale.Quantity;

            context.Sales.Add(sale);

            context.StockHistories.Add(new StockHistory
            {
                MedicineId = medicine.Id,
                ChangeQuantity = -sale.Quantity,
                ActionType = "SALE_CREATED",
                ChangedAt = DateTime.UtcNow,
                ChangedBy = User.Identity?.Name ?? "System"
            });

            await context.SaveChangesAsync();

            return Ok(new
            {
                sale.Id,
                sale.MedicineId,
                MedicineName = medicine.Name,
                sale.Quantity,
                sale.UnitPrice,
                sale.TotalAmount,
                sale.CustomerName,
                sale.HospitalBranch,
                sale.SoldAt
            });
        }

        [HttpPut("sales/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSale(
            int id, [FromBody] Sale updated)
        {
            if (updated == null || updated.MedicineId <= 0 ||
                updated.Quantity <= 0 || updated.UnitPrice < 0)
                return BadRequest(new
                {
                    message = "Please provide valid sale details."
                });

            var sale = await context.Sales.FindAsync(id);

            if (sale == null)
                return NotFound(new { message = "Sale not found." });

            if (sale.MedicineId != updated.MedicineId)
                return BadRequest(new
                {
                    message = "Changing the medicine of an existing sale is not allowed."
                });

            var medicine = await context.Medicines.FindAsync(
                sale.MedicineId);

            if (medicine == null)
                return NotFound(new { message = "Medicine not found." });

            var availableStockAfterRestore =
                medicine.Quantity - medicine.ReservedQuantity + sale.Quantity;

            if (availableStockAfterRestore < updated.Quantity)
                return Conflict(new
                {
                    message = "Not enough available stock for this sale."
                });

            var stockDifference = sale.Quantity - updated.Quantity;

            medicine.Quantity += stockDifference;

            if (stockDifference != 0)
            {
                context.StockHistories.Add(new StockHistory
                {
                    MedicineId = medicine.Id,
                    ChangeQuantity = stockDifference,
                    ActionType = "SALE_UPDATED",
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = User.Identity?.Name ?? "System"
                });
            }

            sale.Quantity = updated.Quantity;
            sale.UnitPrice = updated.UnitPrice;
            sale.TotalAmount = updated.Quantity * updated.UnitPrice;
            sale.CustomerName = updated.CustomerName?.Trim() ?? "";
            sale.HospitalBranch = string.IsNullOrWhiteSpace(
                updated.HospitalBranch)
                ? medicine.HospitalBranch
                : updated.HospitalBranch.Trim();

            await context.SaveChangesAsync();

            return Ok(new
            {
                sale.Id,
                sale.MedicineId,
                MedicineName = medicine.Name,
                sale.Quantity,
                sale.UnitPrice,
                sale.TotalAmount,
                sale.CustomerName,
                sale.HospitalBranch,
                sale.SoldAt
            });
        }

        [HttpDelete("sales/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            var sale = await context.Sales.FindAsync(id);

            if (sale == null)
                return NotFound(new { message = "Sale not found." });

            var medicine = await context.Medicines.FindAsync(
                sale.MedicineId);

            if (medicine == null)
                return NotFound(new { message = "Medicine not found." });

            medicine.Quantity += sale.Quantity;

            context.StockHistories.Add(new StockHistory
            {
                MedicineId = medicine.Id,
                ChangeQuantity = sale.Quantity,
                ActionType = "SALE_DELETED",
                ChangedAt = DateTime.UtcNow,
                ChangedBy = User.Identity?.Name ?? "System"
            });

            context.Sales.Remove(sale);

            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
