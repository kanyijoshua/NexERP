using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Workflows;

[Authorize(ErpPermissions.ApprovalUserSetup.Default)]
public class ApprovalUserSetupAppService
    : CrudAppService<
        ApprovalUserSetup,
        ApprovalUserSetupDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateApprovalUserSetupDto,
        CreateUpdateApprovalUserSetupDto
    >,
        IApprovalUserSetupAppService
{
    public ApprovalUserSetupAppService(IRepository<ApprovalUserSetup, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.ApprovalUserSetup.Default;
        GetListPolicyName = ErpPermissions.ApprovalUserSetup.Default;
        CreatePolicyName = ErpPermissions.ApprovalUserSetup.Create;
        UpdatePolicyName = ErpPermissions.ApprovalUserSetup.Update;
        DeletePolicyName = ErpPermissions.ApprovalUserSetup.Delete;
    }

    public override async Task<ApprovalUserSetupDto> CreateAsync(CreateUpdateApprovalUserSetupDto input)
    {
        await CheckCreatePolicyAsync();

        if (await Repository.AnyAsync(s => s.UserId == input.UserId))
        {
            throw new BusinessException(ErpErrorCodes.Approvals.UserSetupAlreadyExists).WithData("user", input.UserName);
        }

        await EnsureUsersAreSetUpAsync(input.ApproverUserId, input.SubstituteUserId);

        var setup = new ApprovalUserSetup(GuidGenerator.Create(), input.UserId, input.UserName);
        Apply(setup, input);

        await Repository.InsertAsync(setup, autoSave: true);
        return (await WithNamesAsync(new[] { setup })).Single();
    }

    public override async Task<ApprovalUserSetupDto> UpdateAsync(Guid id, CreateUpdateApprovalUserSetupDto input)
    {
        await CheckUpdatePolicyAsync();

        var setup = await GetEntityByIdAsync(id);
        await EnsureUsersAreSetUpAsync(input.ApproverUserId, input.SubstituteUserId);
        Apply(setup, input);

        await Repository.UpdateAsync(setup, autoSave: true);
        return (await WithNamesAsync(new[] { setup })).Single();
    }

    public override async Task<ApprovalUserSetupDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        return (await WithNamesAsync(new[] { await GetEntityByIdAsync(id) })).Single();
    }

    public override async Task<PagedResultDto<ApprovalUserSetupDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await CheckGetListPolicyAsync();

        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);
        var setups = await AsyncExecuter.ToListAsync(ApplyPaging(ApplySorting(query, input), input));

        return new PagedResultDto<ApprovalUserSetupDto>(totalCount, await WithNamesAsync(setups));
    }

    protected override IQueryable<ApprovalUserSetup> ApplyDefaultSorting(IQueryable<ApprovalUserSetup> query)
    {
        return query.OrderBy(x => x.UserName);
    }

    private static void Apply(ApprovalUserSetup setup, CreateUpdateApprovalUserSetupDto input)
    {
        setup.Update(
            input.UserName,
            input.ApproverUserId,
            input.SubstituteUserId,
            input.SalesAmountApprovalLimit,
            input.UnlimitedSalesApproval,
            input.PurchaseAmountApprovalLimit,
            input.UnlimitedPurchaseApproval,
            input.IsApprovalAdministrator
        );
    }

    // An approver without a setup row would break the chain the first time a request is sent.
    private async Task EnsureUsersAreSetUpAsync(params Guid?[] userIds)
    {
        foreach (var userId in userIds.Where(u => u.HasValue).Select(u => u.Value).Distinct())
        {
            if (!await Repository.AnyAsync(s => s.UserId == userId))
            {
                throw new BusinessException(ErpErrorCodes.Approvals.UserNotInApprovalSetup).WithData("user", userId);
            }
        }
    }

    private async Task<List<ApprovalUserSetupDto>> WithNamesAsync(IReadOnlyCollection<ApprovalUserSetup> setups)
    {
        var referenced = setups
            .SelectMany(s => new[] { s.ApproverUserId, s.SubstituteUserId })
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .Distinct()
            .ToList();

        var names = referenced.Count == 0
            ? new Dictionary<Guid, string>()
            : (await Repository.GetListAsync(s => referenced.Contains(s.UserId))).ToDictionary(s => s.UserId, s => s.UserName);

        return setups
            .Select(s =>
            {
                var dto = ObjectMapper.Map<ApprovalUserSetup, ApprovalUserSetupDto>(s);
                dto.ApproverUserName = s.ApproverUserId.HasValue ? names.GetValueOrDefault(s.ApproverUserId.Value) : null;
                dto.SubstituteUserName = s.SubstituteUserId.HasValue ? names.GetValueOrDefault(s.SubstituteUserId.Value) : null;
                return dto;
            })
            .ToList();
    }
}
