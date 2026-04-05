using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QSS.Domain.Entities;
using QSS.Domain.Enums;

namespace QSS.Infrastructure.Data;

public static class DbSeeder
{
    public static readonly string SuperadminId = "superadmin-user-id-fixed";
    public static readonly string AdminId = "admin-user-id-fixed";
    public static readonly string DentistId = "dentist-user-id-fixed";
    public static readonly string AssistantId = "assistant-user-id-fixed";
    public static readonly string TraineeId = "trainee-user-id-fixed";

    public static async Task SeedAsync(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        await context.Database.MigrateAsync();

        // Seed Roles
        string[] roles = { "Superadmin", "Admin", "Dentist", "DentalAssistant", "Trainee" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed Superadmin
        await EnsureUser(userManager, SuperadminId, "superadmin@qss.com", "Super", "Admin", "Admin@1234!", "Superadmin");
        await EnsureUser(userManager, AdminId, "admin@qss.com", "Practice", "Admin", "Admin@1234!", "Admin");
        await EnsureUser(userManager, DentistId, "dentist@qss.com", "Dr. Anna", "Kovač", "Admin@1234!", "Dentist");
        await EnsureUser(userManager, AssistantId, "assistant@qss.com", "Maria", "Steiner", "Admin@1234!", "DentalAssistant");
        await EnsureUser(userManager, TraineeId, "trainee@qss.com", "Tom", "Novak", "Admin@1234!", "Trainee");

        // Seed Rooms
        if (!context.Rooms.IgnoreQueryFilters().Any())
        {
            context.Rooms.AddRange(
                new Room { Name = "Treatment Room 1", Description = "Main treatment room", Location = "Ground Floor" },
                new Room { Name = "Treatment Room 2", Description = "Secondary treatment room", Location = "Ground Floor" },
                new Room { Name = "Sterilization Room", Description = "Instrument sterilization area", Location = "Back Hall" },
                new Room { Name = "Reception", Description = "Front desk and waiting area", Location = "Entrance" }
            );
            await context.SaveChangesAsync();
        }

        // Seed Devices
        if (!context.Devices.IgnoreQueryFilters().Any())
        {
            var room1 = context.Rooms.First();
            context.Devices.AddRange(
                new Device
                {
                    Name = "Dental Chair Unit #1",
                    SerialNumber = "DCU-2023-001",
                    Manufacturer = "Sirona",
                    Model = "Intego",
                    RoomId = room1.Id,
                    NextMaintenanceDue = DateTime.UtcNow.AddMonths(3)
                },
                new Device
                {
                    Name = "X-Ray Machine",
                    SerialNumber = "XR-2022-087",
                    Manufacturer = "Planmeca",
                    Model = "ProX",
                    RoomId = room1.Id,
                    NextMaintenanceDue = DateTime.UtcNow.AddMonths(6)
                },
                new Device
                {
                    Name = "Autoclave",
                    SerialNumber = "AC-2021-003",
                    Manufacturer = "W&H",
                    Model = "Lisa",
                    NextMaintenanceDue = DateTime.UtcNow.AddMonths(1)
                }
            );
            await context.SaveChangesAsync();
        }

        // Seed Processes
        if (!context.Processes.IgnoreQueryFilters().Any())
        {
            context.Processes.AddRange(
                new QssProcess { Name = "Daily Sterilization Protocol", Category = ProcessCategory.Hygiene, Description = "End-of-day sterilization procedures", CreatedByUserId = AdminId },
                new QssProcess { Name = "Equipment Maintenance", Category = ProcessCategory.Maintenance, Description = "Scheduled device maintenance checks", CreatedByUserId = AdminId },
                new QssProcess { Name = "Infection Control", Category = ProcessCategory.Safety, Description = "Standard infection prevention protocols", CreatedByUserId = AdminId }
            );
            await context.SaveChangesAsync();
        }

        // Seed Tasks
        if (!context.Tasks.IgnoreQueryFilters().Any())
        {
            var process1 = context.Processes.First();
            var process2 = context.Processes.Skip(1).First();
            context.Tasks.AddRange(
                new QssTask
                {
                    Name = "Clean and Autoclave Instruments",
                    Description = "Clean all instruments and run autoclave cycle",
                    Status = QssTaskStatus.Open,
                    Frequency = TaskFrequency.Daily,
                    ProcessId = process1.Id,
                    AssigneeId = AssistantId,
                    CreatedByUserId = AdminId,
                    DueDate = DateTime.UtcNow.AddDays(1)
                },
                new QssTask
                {
                    Name = "Dental Chair Inspection",
                    Description = "Check hydraulics, upholstery, and functionality",
                    Status = QssTaskStatus.Open,
                    Frequency = TaskFrequency.Monthly,
                    ProcessId = process2.Id,
                    AssigneeId = DentistId,
                    CreatedByUserId = AdminId,
                    DueDate = DateTime.UtcNow.AddDays(7)
                },
                new QssTask
                {
                    Name = "Surface Disinfection",
                    Description = "Disinfect all treatment surfaces before first patient",
                    Status = QssTaskStatus.Overdue,
                    Frequency = TaskFrequency.Daily,
                    ProcessId = process1.Id,
                    AssigneeId = AssistantId,
                    CreatedByUserId = AdminId,
                    DueDate = DateTime.UtcNow.AddDays(-1)
                }
            );
            await context.SaveChangesAsync();
        }

        // Seed Materials
        if (!context.Materials.IgnoreQueryFilters().Any())
        {
            context.Materials.AddRange(
                new Material { Name = "Disposable Gloves (L)", StockQuantity = 200, MinimumStockLevel = 50, Unit = MaterialUnit.Pieces, ExpirationDate = DateTime.UtcNow.AddYears(2) },
                new Material { Name = "Surgical Masks", StockQuantity = 150, MinimumStockLevel = 40, Unit = MaterialUnit.Pieces, ExpirationDate = DateTime.UtcNow.AddYears(3) },
                new Material { Name = "Disinfection Solution", StockQuantity = 5, MinimumStockLevel = 3, Unit = MaterialUnit.Liters, ExpirationDate = DateTime.UtcNow.AddMonths(18) },
                new Material { Name = "Cotton Rolls", StockQuantity = 3, MinimumStockLevel = 10, Unit = MaterialUnit.Boxes, ExpirationDate = DateTime.UtcNow.AddYears(2) }
            );
            await context.SaveChangesAsync();
        }

        // Seed Medications
        if (!context.Medications.IgnoreQueryFilters().Any())
        {
            context.Medications.AddRange(
                new Medication { Name = "Lidocaine 2%", Quantity = 20, MinimumQuantity = 5, ExpirationDate = DateTime.UtcNow.AddMonths(8), Dosage = "2% with epinephrine", ActiveIngredient = "Lidocaine HCl" },
                new Medication { Name = "Articaine 4%", Quantity = 1, MinimumQuantity = 4, ExpirationDate = DateTime.UtcNow.AddMonths(6), Dosage = "4% with epinephrine 1:100,000", ActiveIngredient = "Articaine HCl" },
                new Medication { Name = "Hydrogen Peroxide 3%", Quantity = 8, MinimumQuantity = 3, ExpirationDate = DateTime.UtcNow.AddMonths(12), Dosage = "3% solution" }
            );
            await context.SaveChangesAsync();
        }

        // Seed Learning Materials
        if (!context.LearningMaterials.IgnoreQueryFilters().Any())
        {
            context.LearningMaterials.AddRange(
                new LearningMaterial { Title = "Infection Control Protocol", Description = "Step-by-step guide for infection prevention", FileUrl = "/uploads/dummy.pdf", FileName = "infection_control.pdf", FileType = "pdf", FileSizeBytes = 102400, Category = "Safety", UploadedByUserId = AdminId },
                new LearningMaterial { Title = "Autoclave Operation Manual", Description = "How to operate the W&H Lisa autoclave", FileUrl = "/uploads/dummy.pdf", FileName = "autoclave_manual.pdf", FileType = "pdf", FileSizeBytes = 204800, Category = "Maintenance", UploadedByUserId = AdminId }
            );
            await context.SaveChangesAsync();
        }
    }

    private static async Task EnsureUser(UserManager<ApplicationUser> userManager,
        string id, string email, string firstName, string lastName, string password, string role)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser
            {
                Id = id,
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }
}
