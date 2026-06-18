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
    public DbSet<B2BUser> B2BUsers => Set<B2BUser>();
    public DbSet<CataloguePartyPrice> CataloguePartyPrices => Set<CataloguePartyPrice>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CatalogueCategory> CatalogueCategories => Set<CatalogueCategory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<DesignMatching>()
            .HasIndex(d => new { d.DesignPlateId, d.MatchingNo })
            .IsUnique();

        builder.Entity<B2BUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        builder.Entity<CataloguePartyPrice>()
            .HasIndex(p => new { p.CatalogueId, p.PartyId })
            .IsUnique();

        builder.Entity<CatalogueCategory>()
            .HasKey(cc => new { cc.CatalogueId, cc.CategoryId });

        // Configure CompanyProfile entity to use uuid Id and new properties
        builder.Entity<CompanyProfile>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Id).HasColumnType("uuid");
            b.Property(c => c.Name).IsRequired();
            b.Property(c => c.LogoFileName);
            b.Property(c => c.Address);
            b.Property(c => c.City);
            b.Property(c => c.State);
            b.Property(c => c.Pincode);
            b.Property(c => c.Phone);
            b.Property(c => c.Mobile);
            b.Property(c => c.Email);
            b.Property(c => c.Website);
            b.Property(c => c.GSTIN);
            b.Property(c => c.PAN);
            b.Property(c => c.CIN);
            b.Property(c => c.LetterheadHtml).HasColumnType("text");
            b.Property(c => c.ThemeColor);

            // New props
            b.Property(c => c.Tagline).HasMaxLength(255);
            b.Property(c => c.BrandmarkText).HasMaxLength(25);
        });
    }
}