using Microsoft.EntityFrameworkCore;
using SIC.Shared.Entities;

namespace SIC.Backend.Data;
public class DataContext: DbContext
{
    public DataContext(DbContextOptions<DataContext> options ) : base(options)
    {

    }
    public DbSet<EventType> EventTypes { get; set; }
    public DbSet<Item> Items { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Item>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<EventType>().HasIndex(x => x.Name).IsUnique();
    }
}
