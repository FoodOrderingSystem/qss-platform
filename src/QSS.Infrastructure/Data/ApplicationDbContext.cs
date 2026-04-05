using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QSS.Domain.Entities;

namespace QSS.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<QssProcess> Processes => Set<QssProcess>();
    public DbSet<QssTask> Tasks => Set<QssTask>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<RoomInventoryItem> RoomInventoryItems => Set<RoomInventoryItem>();
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<Medication> Medications => Set<Medication>();
    public DbSet<MedicationUsageLog> MedicationUsageLogs => Set<MedicationUsageLog>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<AppNotification> Notifications => Set<AppNotification>();
    public DbSet<LearningMaterial> LearningMaterials => Set<LearningMaterial>();
    public DbSet<LearningProgress> LearningProgress => Set<LearningProgress>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // QssTask self-referencing
        builder.Entity<QssTask>()
            .HasOne(t => t.DependsOnTask)
            .WithMany(t => t.DependentTasks)
            .HasForeignKey(t => t.DependsOnTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // Task -> Assignee (restrict delete)
        builder.Entity<QssTask>()
            .HasOne(t => t.Assignee)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Task -> CreatedBy (no nav)
        builder.Entity<QssTask>()
            .HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Process -> CreatedBy
        builder.Entity<QssProcess>()
            .HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // TaskComment -> Author
        builder.Entity<TaskComment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // TaskComment -> MentionedUser
        builder.Entity<TaskComment>()
            .HasOne(c => c.MentionedUser)
            .WithMany()
            .HasForeignKey(c => c.MentionedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ChatMessage -> Sender
        builder.Entity<ChatMessage>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // ChatMessage -> Recipient
        builder.Entity<ChatMessage>()
            .HasOne(m => m.Recipient)
            .WithMany()
            .HasForeignKey(m => m.RecipientId)
            .OnDelete(DeleteBehavior.Restrict);

        // LearningMaterial -> UploadedBy
        builder.Entity<LearningMaterial>()
            .HasOne(l => l.UploadedBy)
            .WithMany()
            .HasForeignKey(l => l.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // MedicationUsageLog -> UsedBy
        builder.Entity<MedicationUsageLog>()
            .HasOne(m => m.UsedBy)
            .WithMany()
            .HasForeignKey(m => m.UsedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Soft delete global query filter example (apply per entity as needed)
        builder.Entity<QssProcess>().HasQueryFilter(p => !p.IsDeleted);
        builder.Entity<QssTask>().HasQueryFilter(t => !t.IsDeleted);
        builder.Entity<Room>().HasQueryFilter(r => !r.IsDeleted);
        builder.Entity<Device>().HasQueryFilter(d => !d.IsDeleted);
        builder.Entity<Material>().HasQueryFilter(m => !m.IsDeleted);
        builder.Entity<Medication>().HasQueryFilter(m => !m.IsDeleted);
        builder.Entity<LearningMaterial>().HasQueryFilter(l => !l.IsDeleted);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is QSS.Domain.Common.BaseEntity &&
                        e.State == EntityState.Modified);
        foreach (var entry in entries)
        {
            ((QSS.Domain.Common.BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }
}
