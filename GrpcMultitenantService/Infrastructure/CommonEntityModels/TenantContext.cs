using GrpcMultitenantService.Infrastructure.CommonEntityModels.Interfaces;

namespace GrpcMultitenantService.Infrastructure.CommonEntityModels;

public class TenantContext : ITenantContext
{
    private string _tenantId = string.Empty;
    private string _countryCode = string.Empty;

    public string TenantId
    {
        get => _tenantId;
        set => _tenantId = value ?? string.Empty;
    }

    public string CountryCode
    {
        get => _countryCode;
        set => _countryCode = value ?? string.Empty;
    }
}
