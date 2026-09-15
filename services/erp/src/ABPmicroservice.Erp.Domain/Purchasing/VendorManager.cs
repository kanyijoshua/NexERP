using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Domain service for Vendor invariants (uniqueness).
/// </summary>
public class VendorManager : DomainService
{
    private readonly IRepository<Vendor, Guid> _vendorRepository;

    public VendorManager(IRepository<Vendor, Guid> vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public async Task<Vendor> CreateAsync(string no, string name)
    {
        await EnsureNoIsUniqueAsync(no);
        return new Vendor(GuidGenerator.Create(), no, name);
    }

    public async Task EnsureNoIsUniqueAsync(string no, Guid? excludeId = null)
    {
        var existing = await _vendorRepository.FirstOrDefaultAsync(x => x.No == no);
        if (existing != null && existing.Id != excludeId)
        {
            throw new BusinessException(ErpErrorCodes.Vendors.VendorAlreadyExists).WithData(
                "no",
                no
            );
        }
    }
}
