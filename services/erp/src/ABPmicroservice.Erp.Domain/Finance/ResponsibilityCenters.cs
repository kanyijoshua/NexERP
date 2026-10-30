using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Responsibility centers: a user whose User Setup names a sales (or purchase) responsibility
/// center works for that center only. New documents get it, lists show only its documents, and a
/// document of another center cannot be opened. A user without one sees every center.
/// </summary>
public class ResponsibilityCenterManager : DomainService
{
    private readonly IRepository<UserSetup, Guid> _userSetupRepository;

    private ICurrentUser CurrentUser => LazyServiceProvider.LazyGetRequiredService<ICurrentUser>();

    public ResponsibilityCenterManager(IRepository<UserSetup, Guid> userSetupRepository)
    {
        _userSetupRepository = userSetupRepository;
    }

    /// <summary>The center the current user works for on sales (or purchase) documents; null is every center.</summary>
    public async Task<string> GetUserCenterAsync(bool sales)
    {
        var userName = CurrentUser.UserName;
        if (userName.IsNullOrWhiteSpace())
        {
            return null;
        }

        var setup = (await _userSetupRepository.GetListAsync()).FirstOrDefault(s => string.Equals(s.UserId, userName, StringComparison.OrdinalIgnoreCase));
        return CodeTableEntity.NormalizeCode(sales ? setup?.SalesRespCtrFilter : setup?.PurchaseRespCtrFilter);
    }

    /// <summary>The center a new document gets: the one asked for, else the user's, else the customer's or vendor's.</summary>
    public async Task<string> DefaultCenterAsync(string requested, string partyCenter, bool sales) =>
        CodeTableEntity.NormalizeCode(requested) ?? await GetUserCenterAsync(sales) ?? CodeTableEntity.NormalizeCode(partyCenter);

    /// <summary>Refuses a document of another center than the one the user works for.</summary>
    public async Task EnsureAllowedAsync(string documentNo, string center, bool sales)
    {
        var userCenter = await GetUserCenterAsync(sales);
        if (userCenter != null && !string.Equals(userCenter, CodeTableEntity.NormalizeCode(center), StringComparison.Ordinal))
        {
            throw new BusinessException(ErpErrorCodes.ResponsibilityCenters.DocumentOfOtherCenter)
                .WithData("documentNo", documentNo)
                .WithData("center", center ?? "");
        }
    }
}
