using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIC.Shared.DTOs;
using SIC.Shared.Entities;

namespace SIC.Backend.Data;

public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<WhatsAppTemplate> WhatsAppTemplates { get; set; }
    public DbSet<EventType> EventTypes { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<PlanItem> PlanItems { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<TablesEvents> TablesEvents { get; set; }
    public DbSet<InvitationSendLog> InvitationSendLogs { get; set; }
    public DbSet<UsuarioWhatsAppConfig> UsuarioWhatsAppConfigs { get; set; }
    public DbSet<MassiveShippingProgress> MassiveShippingProgresses { get; set; }
    public DbSet<MessageKey> MessageKeys { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<InvitationEntry> InvitationEntries { get; set; }
    public DbSet<UserCredit> UserCredits { get; set; }
    public DbSet<UserCreditHistory> UserCreditHistories { get; set; }
    public DbSet<InvitationGuest> InvitationGuest { get; set; }
    public DbSet<EventImage> EventImages { get; set; }
    public DbSet<HistoryMessages> HistoryMessages { get; set; }
    public DbSet<ResponseFromWhatsApp> ResponseFromWhatsApps { get; set; }
    public DbSet<PhotoEvent> PhotoEvents { get; set; }
    public DbSet<PhotoEventImage> PhotoEventImages { get; set; }
    public DbSet<TemplateSent> TemplateSents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<EventType>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Event>().HasIndex(x => new { x.Name, x.SubTitle, x.Date }).IsUnique();
        modelBuilder.Entity<Item>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Plan>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<PlanItem>().HasIndex(x => new { x.PlanId, x.ItemId }).IsUnique();
        modelBuilder.Entity<InvitationEntry>().HasIndex(x => x.Code).IsUnique();
        DisableCascadingDelete(modelBuilder);

        modelBuilder.Entity<UserCredit>()
            .HasIndex(x => x.UserId)
            .IsUnique();

        modelBuilder.Entity<UserCredit>()
            .HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<UserCredit>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserCreditHistory>()
            .HasOne(h => h.UserCredit)
            .WithMany(c => c.CreditHistory)
            .HasForeignKey(h => h.UserCreditId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<User>()
            .HasOne(u => u.UserCredit)
            .WithOne(c => c.User)
            .HasForeignKey<UserCredit>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void DisableCascadingDelete(ModelBuilder modelBuilder)
    {
        var relationships = modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys());
        foreach (var relationship in relationships)
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}