using GrpcMultitenantService.Features.ProviderClaim.Domain;
using GrpcMultitenantService.Features.Tenant.Domain;
using GrpcMultitenantService.Features.Tracking.Domain;
using GrpcMultitenantService.Infrastructure.CommonEntityModels.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GrpcMultitenantService.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext) : DbContext(options)
{
    private readonly ITenantContext _tenantContext = tenantContext;

public string CurrentTenantId => _tenantContext.TenantId;
public string CurrentCountryCode => _tenantContext.CountryCode;

public DbSet<TenantEntity> Tenants { get; set; }
public DbSet<TrackingEntity> Trackings { get; set; }
public DbSet<ProviderClaimEntity> ProviderClaims { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<TrackingEntity>().HasQueryFilter(
        t => t.TenantId == this.CurrentTenantId
    );

    modelBuilder.Entity<ProviderClaimEntity>().HasQueryFilter(
        pc => pc.CountryCode == this.CurrentCountryCode
    );
}
}
