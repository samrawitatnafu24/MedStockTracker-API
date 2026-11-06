using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MedStockTracker.Domain.Entities;
using MedStockTracker.Infrastructure.Identity;

namespace MedStockTracker.Infrastructure.Persistence.Data
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            var adminEmail = Environment.GetEnvironmentVariable("MEDSTOCK_ADMIN_EMAIL");
            var adminPassword = Environment.GetEnvironmentVariable("MEDSTOCK_ADMIN_PASSWORD");
            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "Admin email and password must be configured in environment variables.");
            }
            var adminUser = await context.Users
                .SingleOrDefaultAsync(user => user.Email == adminEmail);

            if (adminUser == null)
            {
                adminUser = new MedStockUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = adminEmail,
                    Email = adminEmail,
                    NormalizedUserName = adminEmail.ToUpperInvariant(),
                    NormalizedEmail = adminEmail.ToUpperInvariant(),
                    FirstName = "Samrawit",
                    LastName = "Atnafu",
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                adminUser.PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        adminPassword,
                        workFactor: 12);

                adminUser.LockoutEnd = null;
                adminUser.AccessFailedCount = 0;

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }
            else
            {
                adminUser.LockoutEnd = null;
                adminUser.AccessFailedCount = 0;

                if (string.IsNullOrWhiteSpace(adminUser.PasswordHash))
                {
                    adminUser.PasswordHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            adminPassword,
                            workFactor: 12);
                }

                await context.SaveChangesAsync();
            }

            const string adminRole = "Admin";

            var role = await context.Roles
                .SingleOrDefaultAsync(r => r.Name == adminRole);

            if (role == null)
            {
                role = new IdentityRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = adminRole,
                    NormalizedName = adminRole.ToUpperInvariant(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };

                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }

            var hasRole = await context.UserRoles
                .AnyAsync(
                    ur => ur.UserId == adminUser.Id &&
                          ur.RoleId == role.Id);

            if (!hasRole)
            {
                context.UserRoles.Add(
                    new IdentityUserRole<string>
                    {
                        UserId = adminUser.Id,
                        RoleId = role.Id
                    });

                await context.SaveChangesAsync();
            }

            if (!await context.Categories.AnyAsync())
            {
                await context.Categories.AddRangeAsync(
                    new Category
                    {
                        Name = "Analgesics",
                        Description =
                            "Pain relief and fever management medicines."
                    },
                    new Category
                    {
                        Name = "Antibiotics",
                        Description =
                            "Medicines used to treat bacterial infections."
                    },
                    new Category
                    {
                        Name = "Diabetes Care",
                        Description =
                            "Insulin and blood-glucose management supplies."
                    },
                    new Category
                    {
                        Name = "Emergency Care",
                        Description =
                            "Essential medicines for urgent patient care."
                    });

                await context.SaveChangesAsync();
            }

            if (!await context.Suppliers.AnyAsync())
            {
                await context.Suppliers.AddRangeAsync(
                    new Supplier
                    {
                        Name = "MedSupply Ethiopia",
                        ContactPerson = "Dawit Kebede",
                        Phone = "+251 911 234 567",
                        Email = "orders@medsupply.et",
                        Address = "Bole, Addis Ababa"
                    },
                    new Supplier
                    {
                        Name = "PharmaCare Distribution",
                        ContactPerson = "Hana Tesfaye",
                        Phone = "+251 922 345 678",
                        Email = "sales@pharmacare.et",
                        Address = "Kazanchis, Addis Ababa"
                    },
                    new Supplier
                    {
                        Name = "Global Medical Supplies",
                        ContactPerson = "Samuel Bekele",
                        Phone = "+251 933 456 789",
                        Email = "contact@globalmed.et",
                        Address = "Merkato, Addis Ababa"
                    });

                await context.SaveChangesAsync();
            }

            if (!await context.Medicines.AnyAsync())
            {
                var createdAt = DateTime.UtcNow;

                var medicines = new[]
                {
                    new Medicine
                    {
                        Name = "Paracetamol 500mg",
                        BatchNumber = "PCM-2026-001",
                        Quantity = 250,
                        Price = 0.25m,
                        ExpiryDate =
                            new DateTime(
                                2027, 12, 31, 0, 0, 0,
                                DateTimeKind.Utc),
                        HospitalBranch = "4 Kilo",
                        MinimumQuantity = 50,
                        CreatedAt = createdAt
                    },

                    new Medicine
                    {
                        Name = "Amoxicillin 500mg",
                        BatchNumber = "AMX-2026-002",
                        Quantity = 120,
                        Price = 1.10m,
                        ExpiryDate =
                            new DateTime(
                                2027, 8, 31, 0, 0, 0,
                                DateTimeKind.Utc),
                        HospitalBranch = "4 Kilo",
                        MinimumQuantity = 50,
                        CreatedAt = createdAt
                    },

                    new Medicine
                    {
                        Name = "Ibuprofen 200mg",
                        BatchNumber = "IBU-2026-003",
                        Quantity = 180,
                        Price = 0.40m,
                        ExpiryDate =
                            new DateTime(
                                2028, 3, 31, 0, 0, 0,
                                DateTimeKind.Utc),
                        HospitalBranch = "4 Kilo",
                        MinimumQuantity = 50,
                        CreatedAt = createdAt
                    },

                    new Medicine
                    {
                        Name = "Oral Rehydration Salts",
                        BatchNumber = "ORS-2026-004",
                        Quantity = 90,
                        Price = 0.75m,
                        ExpiryDate =
                            new DateTime(
                                2027, 6, 30, 0, 0, 0,
                                DateTimeKind.Utc),
                        HospitalBranch = "4 Kilo",
                        MinimumQuantity = 50,
                        CreatedAt = createdAt
                    }
                };

                await context.Medicines.AddRangeAsync(medicines);
                await context.SaveChangesAsync();
            }

            var categories =
                await context.Categories
                    .ToDictionaryAsync(category => category.Name);

            var categoryByMedicine =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ["Paracetamol 500mg"] = "Analgesics",
                    ["Ibuprofen 200mg"] = "Analgesics",
                    ["Amoxicillin 500mg"] = "Antibiotics",
                    ["Oral Rehydration Salts"] = "Emergency Care"
                };

            foreach (
                var medicine
                in await context.Medicines.ToListAsync())
            {
                if (
                    medicine.CategoryId == null &&
                    categoryByMedicine.TryGetValue(
                        medicine.Name,
                        out var categoryName))
                {
                    medicine.CategoryId =
                        categories[categoryName].Id;
                }
            }

            await context.SaveChangesAsync();

            if (!await context.Purchases.AnyAsync())
            {
                var medicines =
                    await context.Medicines
                        .OrderBy(medicine => medicine.Id)
                        .Take(4)
                        .ToListAsync();

                var suppliers =
                    await context.Suppliers
                        .OrderBy(supplier => supplier.Id)
                        .ToListAsync();

                if (medicines.Count < 4 || suppliers.Count < 3)
                {
                    return;
                }

                var purchases = new[]
                {
                    new Purchase
                    {
                        MedicineId = medicines[0].Id,
                        SupplierId = suppliers[0].Id,
                        Quantity = 500,
                        UnitCost = 0.18m,
                        TotalCost = 90m,
                        Status = "Received",
                        PurchasedAt =
                            DateTime.UtcNow.AddDays(-18)
                    },

                    new Purchase
                    {
                        MedicineId = medicines[1].Id,
                        SupplierId = suppliers[1].Id,
                        Quantity = 300,
                        UnitCost = 0.82m,
                        TotalCost = 246m,
                        Status = "Received",
                        PurchasedAt =
                            DateTime.UtcNow.AddDays(-12)
                    },

                    new Purchase
                    {
                        MedicineId = medicines[2].Id,
                        SupplierId = suppliers[0].Id,
                        Quantity = 240,
                        UnitCost = 0.29m,
                        TotalCost = 69.60m,
                        Status = "Received",
                        PurchasedAt =
                            DateTime.UtcNow.AddDays(-7)
                    },

                    new Purchase
                    {
                        MedicineId = medicines[3].Id,
                        SupplierId = suppliers[2].Id,
                        Quantity = 150,
                        UnitCost = 0.55m,
                        TotalCost = 82.50m,
                        Status = "Pending",
                        PurchasedAt =
                            DateTime.UtcNow.AddDays(-2)
                    }
                };

                await context.Purchases.AddRangeAsync(purchases);
                await context.SaveChangesAsync();
            }

            if (!await context.Sales.AnyAsync())
            {
                var medicines =
                    await context.Medicines
                        .OrderBy(medicine => medicine.Id)
                        .Take(4)
                        .ToListAsync();

                if (medicines.Count < 4)
                {
                    return;
                }

                var sales = new[]
                {
                    new Sale
                    {
                        MedicineId = medicines[0].Id,
                        Quantity = 75,
                        UnitPrice = 0.25m,
                        TotalAmount = 18.75m,
                        CustomerName = "BlackLion Hospital",
                        HospitalBranch = "4 Kilo",
                        SoldAt =
                            DateTime.UtcNow.AddDays(-6)
                    },

                    new Sale
                    {
                        MedicineId = medicines[1].Id,
                        Quantity = 42,
                        UnitPrice = 1.10m,
                        TotalAmount = 46.20m,
                        CustomerName = "Minilik Hospital",
                        HospitalBranch = "Minilik",
                        SoldAt =
                            DateTime.UtcNow.AddDays(-4)
                    },

                    new Sale
                    {
                        MedicineId = medicines[2].Id,
                        Quantity = 60,
                        UnitPrice = 0.40m,
                        TotalAmount = 24m,
                        CustomerName = "St. Peter Hospital",
                        HospitalBranch = "St. Peter",
                        SoldAt =
                            DateTime.UtcNow.AddDays(-2)
                    },

                    new Sale
                    {
                        MedicineId = medicines[3].Id,
                        Quantity = 30,
                        UnitPrice = 0.75m,
                        TotalAmount = 22.50m,
                        CustomerName = "City Care Pharmacy",
                        HospitalBranch = "4 Kilo",
                        SoldAt =
                            DateTime.UtcNow.AddDays(-1)
                    }
                };

                await context.Sales.AddRangeAsync(sales);
                await context.SaveChangesAsync();
            }
        }
    }
}
