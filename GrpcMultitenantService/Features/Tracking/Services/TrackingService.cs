using Grpc.Core;
using GrpcMultitenantService.Features.ProviderClaim.Services;
using GrpcMultitenantService.Features.Tracking.Domain;
using GrpcMultitenantService.Infrastructure.CommonEntityModels.Interfaces;
using GrpcMultitenantService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GrpcMultitenantService.Features.Tracking.Services;

public class TrackingService(AppDbContext context, ILogger<TrackingService> logger, ITenantContext tenantContext) : TrackingEvent.TrackingEventBase
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<TrackingService> _logger = logger;
    private readonly ITenantContext _tenantContext = tenantContext;

    private TrackingEntity ToEntity(TrackingModel model)
    {
        return new TrackingEntity
        {
            Id = Guid.TryParse(model.Id, out var guid) ? guid : Guid.NewGuid(),
            Name = model.Name,
            TenantId = model.TenantId,
        };
    }

    private TrackingModel ToProto(TrackingEntity entity)
    {
        return new TrackingModel
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            TenantId = entity.TenantId,
        };
    }

    public override async Task<TrackingResponse> CreateTracking(CreateTrackingRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Creating new tracking: {Name}", request.Name);

        string tenantId = _tenantContext.TenantId;

        var trackingEntity = new TrackingEntity
        {
            Name = request.Name,
            TenantId = tenantId
        };

        await _context.Trackings.AddAsync(trackingEntity);
        await _context.SaveChangesAsync();

        var reply = new TrackingResponse
        {
            Tracking = ToProto(trackingEntity)
        };

        return reply;
    }

    public override async Task<TrackingResponse> GetTracking(TrackingIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Fetching tracking with ID: {Id}", request.Id);

        var trackingEntity = await _context.Trackings.FindAsync(request.Id);

        return trackingEntity == null
            ? throw new RpcException(new Status(StatusCode.NotFound, $"Tracking with ID {request.Id} not found."))
            : new TrackingResponse
            {
                Tracking = ToProto(trackingEntity)
            };
    }

    public override async Task ListTrackings(ListTrackingsRequest request, IServerStreamWriter<TrackingModel> responseStream, ServerCallContext context)
    {
        _logger.LogInformation("Fetching all trackings...");

        var trackings = await _context.Trackings.ToListAsync();

        foreach (var tracking in trackings)
        {
            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Client cancelled the stream.");
                return;
            }

            await responseStream.WriteAsync(ToProto(tracking));
        }
    }
    public override async Task<TrackingResponse> UpdateTracking(UpdateTrackingRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Updating tracking with ID: {Id}", request.Id);

        var trackingEntity = await _context.Trackings.FindAsync(request.Id) 
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Tracking with ID {request.Id} not found for update."));

        trackingEntity.Name = request.Name;
        trackingEntity.TenantId = request.TenantId;

        _context.Entry(trackingEntity).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return new TrackingResponse
        {
            Tracking = ToProto(trackingEntity)
        };
    }

    public override async Task<DeleteTrackingReply> DeleteTracking(TrackingIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Deleting tracking with ID: {Id}", request.Id);

        var trackingEntity = await _context.Trackings.FindAsync(request.Id);

        if (trackingEntity == null)
        {
            return new DeleteTrackingReply { Success = false, Message = $"Tracking with ID {request.Id} already deleted or not found." };
        }

        _context.Trackings.Remove(trackingEntity);
        await _context.SaveChangesAsync();

        return new DeleteTrackingReply { Success = true, Message = $"Tracking with ID {request.Id} successfully deleted." };
    }
}
