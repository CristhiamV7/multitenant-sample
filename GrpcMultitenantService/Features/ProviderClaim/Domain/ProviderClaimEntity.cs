namespace GrpcMultitenantService.Features.ProviderClaim.Domain;

public class ProviderClaimEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string ProviderName { get; set; }
    public required string CountryCode { get; set; }
}
