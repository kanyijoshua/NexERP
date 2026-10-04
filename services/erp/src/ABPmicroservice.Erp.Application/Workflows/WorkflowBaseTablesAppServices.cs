using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>Workflow User Groups.</summary>
public class WorkflowUserGroupAppService : CodeTableAppServiceBase<WorkflowUserGroup, CodeTableDto, CreateUpdateCodeTableDto>, IWorkflowUserGroupAppService
{
    public WorkflowUserGroupAppService(IRepository<WorkflowUserGroup, Guid> repository)
        : base(repository, ErpPermissions.WorkflowUserGroups.Default) { }

    protected override WorkflowUserGroup NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Workflow User Group Members.</summary>
public class WorkflowUserGroupMemberAppService : ErpTableAppService<WorkflowUserGroupMember, WorkflowUserGroupMemberDto, GetWorkflowUserGroupMemberListInput, CreateUpdateWorkflowUserGroupMemberDto>, IWorkflowUserGroupMemberAppService
{
    public WorkflowUserGroupMemberAppService(IRepository<WorkflowUserGroupMember, Guid> repository)
        : base(repository, ErpPermissions.WorkflowUserGroups.Default) { }

    public override async Task<WorkflowUserGroupMemberDto> CreateAsync(CreateUpdateWorkflowUserGroupMemberDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.WorkflowUserGroupCode), CodeTableEntity.NormalizeCode(input.UserName), null);

        var entity = new WorkflowUserGroupMember(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.WorkflowUserGroupCode), CodeTableEntity.NormalizeCode(input.UserName));
        entity.Set(input.SequenceNo);

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<WorkflowUserGroupMemberDto> UpdateAsync(Guid id, CreateUpdateWorkflowUserGroupMemberDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.WorkflowUserGroupCode), CodeTableEntity.NormalizeCode(input.UserName), id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.WorkflowUserGroupCode), CodeTableEntity.NormalizeCode(input.UserName));
        entity.Set(input.SequenceNo);

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<WorkflowUserGroupMember>> CreateFilteredQueryAsync(GetWorkflowUserGroupMemberListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var workflowUserGroupCode = input.WorkflowUserGroupCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!workflowUserGroupCode.IsNullOrEmpty(), x => x.WorkflowUserGroupCode == workflowUserGroupCode)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.WorkflowUserGroupCode.ToLower().Contains(filter)
                    || x.UserName.ToLower().Contains(filter)
            );
    }

    protected override IQueryable<WorkflowUserGroupMember> ApplyDefaultSorting(IQueryable<WorkflowUserGroupMember> query) =>
        query.OrderBy(x => x.WorkflowUserGroupCode).ThenBy(x => x.UserName);

    private async Task ValidateAsync(CreateUpdateWorkflowUserGroupMemberDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<WorkflowUserGroup>(input.WorkflowUserGroupCode);
    }

    private async Task EnsureKeyIsUniqueAsync(string workflowUserGroupCode, string userName, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.WorkflowUserGroupCode == workflowUserGroupCode && x.UserName == userName && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Workflow User Group Member").WithData("key", workflowUserGroupCode + " " + userName);
        }
    }
}

/// <summary>Approval Comment Lines.</summary>
public class ApprovalCommentLineAppService : ErpTableAppService<ApprovalCommentLine, ApprovalCommentLineDto, GetApprovalCommentLineListInput, CreateUpdateApprovalCommentLineDto>, IApprovalCommentLineAppService
{
    public ApprovalCommentLineAppService(IRepository<ApprovalCommentLine, Guid> repository)
        : base(repository, ErpPermissions.ApprovalComments.Default) { }

    public override async Task<ApprovalCommentLineDto> CreateAsync(CreateUpdateApprovalCommentLineDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var entryNo = input.EntryNo > 0 ? input.EntryNo : await NextEntryNoAsync(input);
        await EnsureKeyIsUniqueAsync(entryNo, null);

        var entity = new ApprovalCommentLine(GuidGenerator.Create(), entryNo);
        entity.Set(
            input.TableId,
            input.DocumentType,
            input.DocumentNo,
            input.UserId,
            input.DateAndTime,
            input.Comment
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<ApprovalCommentLineDto> UpdateAsync(Guid id, CreateUpdateApprovalCommentLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var entryNo = input.EntryNo > 0 ? input.EntryNo : entity.EntryNo;
        await EnsureKeyIsUniqueAsync(entryNo, id);

        entity.SetKey(entryNo);
        entity.Set(
            input.TableId,
            input.DocumentType,
            input.DocumentNo,
            input.UserId,
            input.DateAndTime,
            input.Comment
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<ApprovalCommentLine>> CreateFilteredQueryAsync(GetApprovalCommentLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var documentNo = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!documentNo.IsNullOrEmpty(), x => x.DocumentNo == documentNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => (x.DocumentNo != null && x.DocumentNo.ToLower().Contains(filter))
                    || (x.UserId != null && x.UserId.ToLower().Contains(filter))
                    || (x.Comment != null && x.Comment.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<ApprovalCommentLine> ApplyDefaultSorting(IQueryable<ApprovalCommentLine> query) =>
        query.OrderBy(x => x.EntryNo);

    private static Task ValidateAsync(CreateUpdateApprovalCommentLineDto input) => Task.CompletedTask;

    private async Task EnsureKeyIsUniqueAsync(long entryNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.EntryNo == entryNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Approval Comment Line").WithData("key", entryNo.ToString());
        }
    }

    /// <summary>The next Entry No. of the table: 1 above the last.</summary>
    private async Task<long> NextEntryNoAsync(CreateUpdateApprovalCommentLineDto input)
    {
        var query = await Repository.GetQueryableAsync();
        var last = await AsyncExecuter.FirstOrDefaultAsync(query.OrderByDescending(x => x.EntryNo));

        return (last?.EntryNo ?? 0) + 1;
    }
}
