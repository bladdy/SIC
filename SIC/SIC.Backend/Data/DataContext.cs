using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIC.Shared.Entities;

namespace SIC.Backend.Data;

public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<EventType> EventTypes { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<PlanItem> PlanItems { get; set; }
    public DbSet<Event> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<EventType>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Event>().HasIndex(x => new { x.Name, x.SubTitle, x.Date }).IsUnique();
        modelBuilder.Entity<Item>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Plan>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<PlanItem>().HasIndex(x => new { x.PlanId, x.ItemId }).IsUnique();

        DisableCascadingDelete(modelBuilder);
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