using System.ComponentModel.DataAnnotations;

namespace GrpcMultitenantService.Features.Tenant.Domain;

public class TenantEntity
{
    [Key]
    public required string TenantId { get; set; }
    public required string Name { get; set; }
    public required string CountryCode { get; set; }
    public required string CurrencyCode { get; set; }
}
