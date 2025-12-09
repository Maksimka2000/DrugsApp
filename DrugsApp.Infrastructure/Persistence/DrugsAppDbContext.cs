using DrugsApp.Domain.TestFeature.Entities;
using Microsoft.EntityFrameworkCore;
namespace DrugsApp.Infrastructure.Persistence;

public sealed class DrugsAppDbContext : DbContext
{
    public DrugsAppDbContext(DbContextOptions<DrugsAppDbContext> options)
        : base(options) { }

    public DbSet<Test> Tests { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DrugsAppDbContext).Assembly);
    }
}