using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Companies;

[Serializable]
public class CompanyCacheItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public bool IsDefault { get; set; }
}

[Serializable]
public class TenantCompaniesCacheItem
{
    public const string CacheKey = "all";

    public List<CompanyCacheItem> Companies { get; set; } = new();
}

/// <summary>
/// Decides which company a request runs in. Business Central picks the company at sign-in;
/// here the client names it per request and the tenant's default company is the fallback.
/// </summary>
public class CompanyResolver : DomainService
{
    private readonly IRepository<Company, Guid> _companyRepository;
    private readonly IDistributedCache<TenantCompaniesCacheItem> _cache;

    public CompanyResolver(
        IRepository<Company, Guid> companyRepository,
        IDistributedCache<TenantCompaniesCacheItem> cache
    )
    {
        _companyRepository = companyRepository;
        _cache = cache;
    }

    /// <summary>
    /// Returns the requested company if it belongs to the current tenant, the tenant's default
    /// company when none is requested, or null when the tenant has no company yet.
    /// </summary>
    public async Task<BasicCompanyInfo> ResolveAsync(Guid? requestedCompanyId)
    {
        var companies = await GetTenantCompaniesAsync();

        if (requestedCompanyId.HasValue)
        {
            var requested = companies.FirstOrDefault(c => c.Id == requestedCompanyId.Value);
            if (requested == null)
            {
                // Unknown, or another tenant's: the tenant filter hides it either way.
                throw new BusinessException(ErpErrorCodes.Companies.CompanyNotFound).WithData(
                    "CompanyId",
                    requestedCompanyId.Value
                );
            }

            return new BasicCompanyInfo(requested.Id, requested.Name);
        }

        var fallback = companies.FirstOrDefault(c => c.IsDefault) ?? companies.FirstOrDefault();
        return fallback == null ? null : new BasicCompanyInfo(fallback.Id, fallback.Name);
    }

    /// <summary>Call after creating, renaming or deleting a company.</summary>
    public Task InvalidateAsync()
    {
        return _cache.RemoveAsync(TenantCompaniesCacheItem.CacheKey);
    }

    // The cache is tenant-aware: ABP prefixes the key with the current tenant id.
    private async Task<List<CompanyCacheItem>> GetTenantCompaniesAsync()
    {
        var item = await _cache.GetOrAddAsync(
            TenantCompaniesCacheItem.CacheKey,
            async () =>
            {
                var companies = await _companyRepository.GetListAsync();
                return new TenantCompaniesCacheItem
                {
                    Companies = companies
                        .OrderBy(c => c.CreationTime)
                        .Select(c => new CompanyCacheItem
                        {
                            Id = c.Id,
                            Name = c.Name,
                            IsDefault = c.IsDefault,
                        })
                        .ToList(),
                };
            },
            () => new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }
        );

        return item.Companies;
    }
}
