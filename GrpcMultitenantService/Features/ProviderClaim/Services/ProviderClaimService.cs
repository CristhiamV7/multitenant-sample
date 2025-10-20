using Grpc.Core;
using GrpcMultitenantService.Features.ProviderClaim.Domain;
using GrpcMultitenantService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GrpcMultitenantService.Features.ProviderClaim.Services;

public class ProviderClaimService(AppDbContext context, ILogger<ProviderClaimService> logger) : Claim.ClaimBase
{
    private readonly AppDbContext _context = context;
private readonly ILogger<ProviderClaimService> _logger = logger;

private ProviderClaimEntity ToEntity(ClaimModel model)
{
    return new ProviderClaimEntity
    {
        Id = Guid.TryParse(model.Id, out var guid) ? guid : Guid.NewGuid(),
        ProviderName = model.ProviderName,
        CountryCode = model.CountryCode
    };
}

private ClaimModel ToProto(ProviderClaimEntity entity)
{
    return new ClaimModel
    {
        Id = entity.Id.ToString(),
        ProviderName = entity.ProviderName,
        CountryCode = entity.CountryCode
    };
}

public override async Task GetAllClaims(
        GetAllClaimsRequest request,
        IServerStreamWriter<GetAllClaimsReply> responseStream,
        ServerCallContext context)
{
    _logger.LogInformation("Streaming all Claim Providers filtered by CountryCode: {CountryCode}", _context.CurrentCountryCode);

    var providers = await _context.ProviderClaims.ToListAsync();

    if (providers == null || providers.Count == 0)
    {
        _logger.LogInformation("No claim providers found for the current CountryCode.");
        return;
    }

    foreach (var provider in providers)
    {
        if (context.CancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Client cancelled the Claim Provider stream.");
            return;
        }

        var reply = new GetAllClaimsReply();

        reply.Claims.Add(ToProto(provider));

        await responseStream.WriteAsync(reply);
    }
}
}
