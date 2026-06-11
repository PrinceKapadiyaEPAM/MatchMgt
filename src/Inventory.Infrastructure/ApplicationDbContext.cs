using Inventory.Domain.Entities;
using Inventory.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Inventory.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Catalogue> Catalogues => Set<Catalogue>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<Design> Designs => Set<Design>();
    public DbSet<DesignPlate> DesignPlates => Set<DesignPlate>();
    public DbSet<DesignMatching> DesignMatchings => Set<DesignMatching>();
    public DbSet<Party> Party => Set<Party>();
    public DbSet<ProgramEntry> Program => Set<ProgramEntry>();
    public DbSet<ProgramMatching> ProgramMatchings => Set<ProgramMatching>();
    public DbSet<CompanyProfile> CompanyProfile => Set<CompanyProfile>();
    public DbSet<DispatchEntry> DispatchEntries => Set<DispatchEntry>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<DesignMatching>()
            .HasIndex(d => new { d.DesignPlateId, d.MatchingNo })
            .IsUnique();
    }
}