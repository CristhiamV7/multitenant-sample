using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcMultitenantService.Infrastructure.CommonEntityModels.Interfaces;
using GrpcMultitenantService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GrpcMultitenantService.Common;

public class MultitenancyInterceptor(ITenantContext tenantContext, AppDbContext dbContext) : Interceptor
{
    private const string TenantIdHeader = "tenant-id";
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly AppDbContext _dbContext = dbContext;

    private void SetTenantIdFromContext(ServerCallContext context)
    {
        string? headerTenantId = null;

        foreach (var entry in context.RequestHeaders)
        {
            if (string.Equals(entry.Key, TenantIdHeader, StringComparison.OrdinalIgnoreCase))
            {
                headerTenantId = entry.Value;
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(headerTenantId))
        {
            throw new RpcException(new Status(
                StatusCode.Unauthenticated,
                $"Required metadata '{TenantIdHeader}' is missing or empty."
            ));
        }

        var tenant = _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefault(t => t.TenantId == headerTenantId) ?? throw new RpcException(new Status(
                StatusCode.Unauthenticated,
                $"Invalid tenant ID '{headerTenantId}' provided."
            ));

        _tenantContext.TenantId = headerTenantId;
        _tenantContext.CountryCode = tenant.CountryCode;
    }
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        SetTenantIdFromContext(context);
        return await continuation(request, context);
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        SetTenantIdFromContext(context);
        await continuation(request, responseStream, context);
    }

    public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        SetTenantIdFromContext(context);
        return await continuation(requestStream, context);
    }

    public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(
        IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        SetTenantIdFromContext(context);
        await continuation(requestStream, responseStream, context);
    }
}
