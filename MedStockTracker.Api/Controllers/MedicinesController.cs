using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using MediatR;
using MedStockTracker.Application.DTOs.Medicine;
using MedStockTracker.Infrastructure.Persistence.Data;
using MedStockTracker.Domain.Entities;

using LocalUpdateCommand = MedStockTracker.Application.Medicines.Commands.UpdateMedicineCommand;
using LocalDeleteCommand = MedStockTracker.Application.Medicines.Commands.DeleteMedicineCommand;
using LocalCreateCommand = MedStockTracker.Application.Medicines.Commands.CreateMedicineCommand;

namespace MedStockTracker.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MedicinesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public MedicinesController(
            IMediator mediator,
            ApplicationDbContext context,
            IAuthorizationService authorizationService)
        {
            _mediator = mediator;
            _context = context;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] MedicineFilterDto filter)
        {
            var query = new MedStockTracker.Application.Medicines.Queries.GetAllMedicinesQuery(filter);
            var medicines = await _mediator.Send(query);
            return Ok(medicines);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new MedStockTracker.Application.Medicines.Queries.GetMedicineByIdQuery(id);
            var medicine = await _mediator.Send(query);

            if (medicine == null)
                return NotFound();

            return Ok(medicine);
        }

        [HttpGet("near-expiry")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNearExpiry()
        {
            var query = new MedStockTracker.Application.Medicines.Queries.GetNearExpiryMedicinesQuery();
            var medicines = await _mediator.Send(query);
            return Ok(medicines);
        }

        [HttpGet("{id}/stock-history")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStockHistory(int id)
        {
            var query = new MedStockTracker.Application.Medicines.Queries.GetStockHistoryQuery(id);
            var history = await _mediator.Send(query);
            return Ok(history);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateMedicineDto dto)
        {
            var command = new LocalCreateCommand(
                dto.Name,
                dto.BatchNumber,
                dto.Quantity,
                dto.Price,
                dto.ExpiryDate,
                dto.HospitalBranch,
                dto.MinimumQuantity);

            dynamic result = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CreateMedicineDto dto)
        {
            var medicine = await _context.Medicines.FindAsync(id);

            if (medicine == null)
                return NotFound();

            var authorization = await _authorizationService.AuthorizeAsync(
                User,
                medicine,
                "CanEditMedicine");

            if (!authorization.Succeeded)
                return Forbid();

            var command = new LocalUpdateCommand(
                id,
                dto.Name,
                dto.BatchNumber,
                dto.Quantity,
                dto.Price,
                dto.ExpiryDate,
                dto.HospitalBranch,
                dto.MinimumQuantity);

            var result = await _mediator.Send(command);

            bool updated = result is bool b
                ? b
                : Convert.ToBoolean(result);

            if (!updated)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new LocalDeleteCommand(id);

            object? result = await _mediator.Send(command);

            bool deleted = result is bool b
                ? b
                : Convert.ToBoolean(result);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpPost("import")]
        [AllowAnonymous]
        public async Task<IActionResult> ImportFromCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please upload a valid data CSV file spreadsheet."
                });
            }

            int importedRecordsCount = 0;

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                await reader.ReadLineAsync();

                string? line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var segments = line
                        .Split(',')
                        .Select(x => x.Trim().Trim('"'))
                        .ToArray();

                    if (segments.Length < 5)
                        continue;

                    string name = segments[0];
                    string batchNumber = segments[1];

                    if (!int.TryParse(
                            segments[2],
                            out var quantity) ||
                        !decimal.TryParse(
                            segments[3],
                            out var price) ||
                        !DateTime.TryParse(
                            segments[4],
                            out var expiryDate))
                    {
                        continue;
                    }

                    var hospitalBranch =
                        segments.Length > 5 &&
                        !string.IsNullOrWhiteSpace(segments[5])
                            ? segments[5]
                            : "4 Kilo";

                    var command = new LocalCreateCommand(
                        name,
                        batchNumber,
                        quantity,
                        price,
                        expiryDate,
                        hospitalBranch,
                        50);

                    await _mediator.Send(command);

                    importedRecordsCount++;
                }
            }

            return Ok(new
            {
                success = true,
                count = importedRecordsCount,
                message = "Bulk warehouse stock import completed cleanly!"
            });
        }

        [HttpGet("analytics")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnalytics(
            [FromQuery] string? hospitalBranch = null)
        {
            var medicines = _context.Medicines.AsQueryable();

            if (!string.IsNullOrWhiteSpace(hospitalBranch) &&
                hospitalBranch != "All branches")
            {
                medicines = medicines.Where(
                    x => x.HospitalBranch == hospitalBranch);
            }

            var rows = await medicines.ToListAsync();

            var expired = rows
                .Where(x => x.IsExpired)
                .ToList();

            var active = rows
                .Where(x => !x.IsExpired)
                .ToList();

            return Ok(new
            {
                expiredQuantity = expired.Sum(x => x.Quantity),
                expiredValue = expired.Sum(x => x.Quantity * x.Price),
                activeQuantity = active.Sum(x => x.Quantity),
                activeValue = active.Sum(x => x.Quantity * x.Price)
            });
        }

        [HttpPost("{id}/holds")]
        [AllowAnonymous]
        public async Task<IActionResult> Hold(
            int id,
            [FromBody] MedicineHoldDto dto)
        {
            var medicine = await _context.Medicines.FindAsync(id);

            if (medicine == null)
                return NotFound();

            if (dto.DurationHours < 1 ||
                dto.DurationHours > 2)
            {
                return BadRequest(new
                {
                    message = "A patient hold can last up to 2 hours."
                });
            }

            var availableQuantity =
                medicine.Quantity - medicine.ReservedQuantity;

            if (dto.Quantity > availableQuantity)
            {
                return BadRequest(new
                {
                    message = "Not enough available stock for this hold."
                });
            }

            var expiresAt =
                DateTime.UtcNow.AddHours(dto.DurationHours);

            medicine.ReservedQuantity += dto.Quantity;

            _context.MedicineHolds.Add(
                new MedicineHold
                {
                    MedicineId = id,
                    PatientName = dto.PatientName,
                    Quantity = dto.Quantity,
                    Note = dto.Note,
                    ExpiresAt = expiresAt
                });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Medicine held for the patient.",
                expiresAt
            });
        }

        // GLOBAL NETWORK SEARCH GATEWAY:
        // Allows patients to look up medicine across hospital branches.
        [HttpGet("search-by-name")]
        [AllowAnonymous]
        public IActionResult SearchMedicinePublic(
            [FromQuery] string medicineName)
        {
            if (string.IsNullOrWhiteSpace(medicineName))
            {
                return BadRequest(new
                {
                    message = "Please enter a medicine name to query."
                });
            }

            var crossHospitalStockLedger = new List<object>
            {
                new
                {
                    MedicineName = "Insulin Glargine",
                    Pharmacy = "Minilik Hospital Pharmacy",
                    Quantity = 120,
                    Price = 45.00,
                    Location = "4 Kilo, Addis Ababa",
                    Phone = "+251111XXXXXX"
                },
                new
                {
                    MedicineName = "Insulin Glargine",
                    Pharmacy = "Anwar Pharmacy",
                    Quantity = 15,
                    Price = 48.50,
                    Location = "Merkato, near Anwar Mosque",
                    Phone = "+251911XXXXXX"
                },
                new
                {
                    MedicineName = "Amoxicillin 500mg",
                    Pharmacy = "Black Lion Hospital Pharmacy",
                    Quantity = 0,
                    Price = 15.50,
                    Location = "6 Kilo, Addis Ababa",
                    Phone = "Out of Stock"
                },
                new
                {
                    MedicineName = "Amoxicillin 500mg",
                    Pharmacy = "Piazza Central Pharmacy",
                    Quantity = 45,
                    Price = 16.00,
                    Location = "Piazza, Opp. Cinema Ethiopia",
                    Phone = "+251912XXXXXX"
                }
            };

            var matchedResults = crossHospitalStockLedger
                .Where(m =>
                    ((string)m.GetType()
                        .GetProperty("MedicineName")!
                        .GetValue(m, null)!)
                    .Contains(
                        medicineName,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(matchedResults);
        }
    }
}
