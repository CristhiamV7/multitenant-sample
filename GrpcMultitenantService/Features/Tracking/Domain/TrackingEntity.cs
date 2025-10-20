namespace GrpcMultitenantService.Features.Tracking.Domain;

public class TrackingEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string TenantId { get; set; }
}
