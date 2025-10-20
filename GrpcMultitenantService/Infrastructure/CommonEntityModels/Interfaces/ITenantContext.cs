namespace GrpcMultitenantService.Infrastructure.CommonEntityModels.Interfaces;

public interface ITenantContext
{
    string TenantId { get; set; }
    string CountryCode { get; set; }
}
