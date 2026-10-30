using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Quality;

/// <summary>
/// Quality Control Point: a test goods of an item (or of an item category, or any item) go through
/// when they are received or produced, either pass/fail or a measurement within limits.
/// </summary>
public class QualityControlPoint : CodeTableEntity
{
    public QualityOperation Operation { get; private set; }

    /// <summary>The item tested; blank tests every item of the category, or every item.</summary>
    public string ItemNo { get; private set; }

    public string ItemCategoryCode { get; private set; }
    public QualityTestType TestType { get; private set; }
    public decimal MinValue { get; private set; }
    public decimal MaxValue { get; private set; }
    public string UnitOfMeasure { get; private set; }
    public string Instructions { get; private set; }
    public bool Blocked { get; private set; }

    protected QualityControlPoint() { }

    public QualityControlPoint(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(QualityOperation operation, string itemNo, string itemCategoryCode, QualityTestType testType, decimal minValue, decimal maxValue, string unitOfMeasure, string instructions, bool blocked)
    {
        if (testType == QualityTestType.Measure && maxValue < minValue)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.InvalidDateRange);
        }

        Operation = operation;
        ItemNo = itemNo.IsNullOrWhiteSpace() ? null : Check.Length(itemNo.Trim(), nameof(itemNo), ErpDomainConsts.MaxNoLength);
        ItemCategoryCode = NormalizeCode(Check.Length(itemCategoryCode, nameof(itemCategoryCode), ErpDomainConsts.MaxCodeLength));
        TestType = testType;
        MinValue = minValue;
        MaxValue = maxValue;
        UnitOfMeasure = Check.Length(unitOfMeasure, nameof(unitOfMeasure), ErpDomainConsts.MaxCodeLength);
        Instructions = Check.Length(instructions, nameof(instructions), 2000);
        Blocked = blocked;
    }

    public bool Applies(Item item, QualityOperation operation) =>
        !Blocked && Operation == operation
        && (ItemNo == null || string.Equals(ItemNo, item.No, StringComparison.OrdinalIgnoreCase))
        && (ItemCategoryCode == null || ItemCategoryCode == item.ItemCategoryCode);
}

/// <summary>Quality Check: one test of goods at a control point, pending until it is recorded as passed or failed.</summary>
public class QualityCheck : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string ControlPointCode { get; private set; }
    public QualityOperation Operation { get; private set; }
    public string ItemNo { get; private set; }
    public string ItemDescription { get; private set; }
    public string SourceDocumentNo { get; private set; }
    public string LotNo { get; private set; }
    public decimal Quantity { get; private set; }
    public QualityTestType TestType { get; private set; }
    public decimal MinValue { get; private set; }
    public decimal MaxValue { get; private set; }
    public decimal? MeasuredValue { get; private set; }
    public QualityCheckResult Result { get; private set; }
    public string CheckedBy { get; private set; }
    public DateTime? CheckedDate { get; private set; }
    public string Note { get; private set; }

    protected QualityCheck() { }

    public QualityCheck(Guid id, string no, QualityControlPoint point, Item item, string sourceDocumentNo, string lotNo, decimal quantity)
        : base(id)
    {
        No = no;
        ControlPointCode = point.Code;
        Operation = point.Operation;
        ItemNo = item.No;
        ItemDescription = item.Description;
        SourceDocumentNo = sourceDocumentNo;
        LotNo = lotNo;
        Quantity = quantity;
        TestType = point.TestType;
        MinValue = point.MinValue;
        MaxValue = point.MaxValue;
        Result = QualityCheckResult.Pending;
    }

    /// <summary>
    /// Records the test: a measurement passes when it is within the limits; a pass/fail test takes
    /// the verdict given.
    /// </summary>
    public void Record(bool? passed, decimal? measuredValue, string note, string checkedBy, DateTime date)
    {
        if (Result != QualityCheckResult.Pending)
        {
            throw new BusinessException(ErpErrorCodes.Quality.CheckAlreadyRecorded).WithData("no", No).WithData("result", Result.ToString());
        }

        if (TestType == QualityTestType.Measure)
        {
            MeasuredValue = measuredValue ?? throw new BusinessException(ErpErrorCodes.Quality.MeasureRequired).WithData("no", No);
            Result = measuredValue >= MinValue && measuredValue <= MaxValue ? QualityCheckResult.Pass : QualityCheckResult.Fail;
        }
        else
        {
            Result = passed == true ? QualityCheckResult.Pass : QualityCheckResult.Fail;
        }

        Note = Check.Length(note, nameof(note), 2000);
        CheckedBy = checkedBy;
        CheckedDate = date.Date;
    }

    /// <summary>A recorded check is done again, e.g. after rework.</summary>
    public void Reset()
    {
        Result = QualityCheckResult.Pending;
        MeasuredValue = null;
        CheckedBy = null;
        CheckedDate = null;
    }
}

/// <summary>Makes the checks the control points ask for when goods are received or produced.</summary>
public class QualityManager : DomainService
{
    private readonly IRepository<QualityControlPoint, Guid> _points;
    private readonly IRepository<QualityCheck, Guid> _checks;
    private readonly IRepository<Item, Guid> _items;

    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public QualityManager(IRepository<QualityControlPoint, Guid> points, IRepository<QualityCheck, Guid> checks, IRepository<Item, Guid> items)
    {
        _points = points;
        _checks = checks;
        _items = items;
    }

    public async Task<string> NextNoAsync() => $"QC{await EntryNos.NextAsync(ErpSequenceNames.QualityCheckNo):D6}";

    /// <summary>A check per control point that applies to each item line a receipt brought in.</summary>
    public async Task<int> CreateForReceiptAsync(PostedPurchaseReceipt receipt)
    {
        if (receipt.Return)
        {
            return 0;
        }

        var made = 0;
        foreach (var line in receipt.Lines.Where(l => l.Type == DocumentLineType.Item && !l.Correction && l.Quantity > 0m))
        {
            made += await CreateAsync(QualityOperation.Receipt, line.No, receipt.No, null, line.Quantity * line.QtyPerUnitOfMeasure);
        }

        return made;
    }

    /// <summary>A check per control point that applies to output of a production order.</summary>
    public async Task<int> CreateForOutputAsync(string itemNo, string productionOrderNo, decimal quantity) =>
        await CreateAsync(QualityOperation.Production, itemNo, productionOrderNo, null, quantity);

    public async Task<int> CreateAsync(QualityOperation operation, string itemNo, string sourceDocumentNo, string lotNo, decimal quantity)
    {
        var item = await _items.FirstOrDefaultAsync(i => i.No == itemNo);
        if (item == null)
        {
            return 0;
        }

        var points = (await _points.GetListAsync(p => !p.Blocked && p.Operation == operation)).Where(p => p.Applies(item, operation)).OrderBy(p => p.Code).ToList();
        foreach (var point in points)
        {
            await _checks.InsertAsync(new QualityCheck(GuidGenerator.Create(), await NextNoAsync(), point, item, sourceDocumentNo, lotNo, quantity), autoSave: true);
        }

        return points.Count;
    }
}
