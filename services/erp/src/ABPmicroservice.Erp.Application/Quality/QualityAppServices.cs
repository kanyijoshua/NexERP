using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Quality;

public class QualityControlPointAppService
    : CodeTableAppServiceBase<QualityControlPoint, QualityControlPointDto, CreateUpdateQualityControlPointDto>,
        IQualityControlPointAppService
{
    public QualityControlPointAppService(IRepository<QualityControlPoint, Guid> repository)
        : base(repository, ErpPermissions.Quality.Default) { }

    protected override QualityControlPoint NewEntity(Guid id, CreateUpdateQualityControlPointDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(QualityControlPoint entity, CreateUpdateQualityControlPointDto input)
    {
        var itemNo = input.ItemNo?.Trim();
        if (!itemNo.IsNullOrEmpty() && !await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().AnyAsync(i => i.No == itemNo))
        {
            throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", itemNo);
        }

        entity.Set(input.Operation, itemNo, input.ItemCategoryCode, input.TestType, input.MinValue, input.MaxValue, input.UnitOfMeasure, input.Instructions, input.Blocked);
    }
}

[Authorize(ErpPermissions.Quality.Default)]
public class QualityCheckAppService : ErpReadOnlyAppService<QualityCheck, QualityCheckDto, Guid, GetQualityCheckListInput>, IQualityCheckAppService
{
    private readonly IRepository<QualityCheck, Guid> _checks;

    private QualityManager Manager => LazyServiceProvider.LazyGetRequiredService<QualityManager>();

    public QualityCheckAppService(IRepository<QualityCheck, Guid> repository)
        : base(repository)
    {
        _checks = repository;
        GetPolicyName = ErpPermissions.Quality.Default;
        GetListPolicyName = ErpPermissions.Quality.Default;
    }

    [Authorize(ErpPermissions.Quality.Create)]
    public async Task<QualityCheckDto> CreateAsync(CreateQualityCheckDto input)
    {
        var code = CodeTableEntity.NormalizeCode(input.ControlPointCode);
        var point = await LazyServiceProvider.LazyGetRequiredService<IRepository<QualityControlPoint, Guid>>().FirstOrDefaultAsync(p => p.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.Quality.ControlPointNotFound).WithData("code", code ?? "");
        var itemNo = input.ItemNo?.Trim();
        var item = await LazyServiceProvider.LazyGetRequiredService<IRepository<Item, Guid>>().FirstOrDefaultAsync(i => i.No == itemNo)
            ?? throw new BusinessException(ErpErrorCodes.Items.ItemNotFound).WithData("no", itemNo ?? "");
        var check = new QualityCheck(GuidGenerator.Create(), await Manager.NextNoAsync(), point, item, input.SourceDocumentNo, input.LotNo, input.Quantity);
        await _checks.InsertAsync(check, autoSave: true);
        return await MapToGetOutputDtoAsync(check);
    }

    [Authorize(ErpPermissions.Quality.Record)]
    public async Task<QualityCheckDto> RecordAsync(Guid id, RecordQualityCheckInput input)
    {
        var check = await _checks.GetAsync(id);
        check.Record(input.Passed, input.MeasuredValue, input.Note, CurrentUser.UserName, Clock.Now);
        await _checks.UpdateAsync(check, autoSave: true);
        return await MapToGetOutputDtoAsync(check);
    }

    [Authorize(ErpPermissions.Quality.Record)]
    public async Task<QualityCheckDto> ResetAsync(Guid id)
    {
        var check = await _checks.GetAsync(id);
        check.Reset();
        await _checks.UpdateAsync(check, autoSave: true);
        return await MapToGetOutputDtoAsync(check);
    }

    [Authorize(ErpPermissions.Quality.Delete)]
    public async Task DeleteAsync(Guid id) => await _checks.DeleteAsync(id, autoSave: true);

    protected override async Task<IQueryable<QualityCheck>> CreateFilteredQueryAsync(GetQualityCheckListInput input)
    {
        var query = await ReadOnlyRepository.GetQueryableAsync();
        var document = input.SourceDocumentNo?.Trim();
        return query.WhereIf(!document.IsNullOrEmpty(), c => c.SourceDocumentNo == document).WhereIf(input.Result.HasValue, c => c.Result == input.Result.Value);
    }

    protected override IQueryable<QualityCheck> ApplyDefaultSorting(IQueryable<QualityCheck> query) => query.OrderByDescending(c => c.No);
}
