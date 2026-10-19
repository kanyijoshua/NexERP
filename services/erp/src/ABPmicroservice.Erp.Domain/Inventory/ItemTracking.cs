using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>How an item is tracked: by lot number, by serial number, or both.</summary>
public class ItemTrackingCode : CodeTableEntity
{
    public bool LotSpecific { get; private set; }
    public bool SerialSpecific { get; private set; }

    /// <summary>A lot or serial number coming in must carry an expiration date.</summary>
    public bool ExpirationDateRequired { get; private set; }

    protected ItemTrackingCode() { }

    public ItemTrackingCode(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetRules(bool lotSpecific, bool serialSpecific, bool expirationDateRequired)
    {
        LotSpecific = lotSpecific;
        SerialSpecific = serialSpecific;
        ExpirationDateRequired = expirationDateRequired;
    }
}

/// <summary>What is known about a lot: its expiry and whether it may be used.</summary>
public class LotNoInformation : CompanyEntity
{
    public string ItemNo { get; private set; }
    public string VariantCode { get; private set; }
    public string LotNo { get; private set; }
    public string Description { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public bool Blocked { get; private set; }

    protected LotNoInformation() { }

    public LotNoInformation(Guid id, string itemNo, string variantCode, string lotNo)
        : base(id)
    {
        ItemNo = Check.NotNullOrWhiteSpace(itemNo, nameof(itemNo), ErpDomainConsts.MaxNoLength);
        VariantCode = CodeTableEntity.NormalizeCode(Check.Length(variantCode, nameof(variantCode), ErpDomainConsts.MaxCodeLength));
        LotNo = Check.NotNullOrWhiteSpace(lotNo?.Trim(), nameof(lotNo), ErpDomainConsts.MaxLotNoLength);
    }

    public void SetDetails(string description, DateTime? expirationDate, bool blocked)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        ExpirationDate = expirationDate?.Date;
        Blocked = blocked;
    }
}

/// <summary>What is known about one serial number.</summary>
public class SerialNoInformation : CompanyEntity
{
    public string ItemNo { get; private set; }
    public string VariantCode { get; private set; }
    public string SerialNo { get; private set; }
    public string Description { get; private set; }
    public bool Blocked { get; private set; }

    protected SerialNoInformation() { }

    public SerialNoInformation(Guid id, string itemNo, string variantCode, string serialNo)
        : base(id)
    {
        ItemNo = Check.NotNullOrWhiteSpace(itemNo, nameof(itemNo), ErpDomainConsts.MaxNoLength);
        VariantCode = CodeTableEntity.NormalizeCode(Check.Length(variantCode, nameof(variantCode), ErpDomainConsts.MaxCodeLength));
        SerialNo = Check.NotNullOrWhiteSpace(serialNo?.Trim(), nameof(serialNo), ErpDomainConsts.MaxLotNoLength);
    }

    public void SetDetails(string description, bool blocked)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Blocked = blocked;
    }
}

/// <summary>
/// The lots and serial numbers of one document or journal line. Postings of the line take from its
/// tracking lines in order, so a line shipped in parts uses up its tracking lines in parts.
/// </summary>
public class ItemTrackingLine : CompanyEntity
{
    public ItemTrackingSourceType SourceType { get; private set; }

    /// <summary>The id of the line the tracking belongs to.</summary>
    public Guid SourceId { get; private set; }

    public string ItemNo { get; private set; }
    public string LotNo { get; private set; }
    public string SerialNo { get; private set; }

    /// <summary>In the item's base unit of measure.</summary>
    public decimal Quantity { get; private set; }

    public decimal QuantityHandled { get; private set; }

    public DateTime? ExpirationDate { get; private set; }

    public decimal QuantityRemaining => Quantity - QuantityHandled;

    protected ItemTrackingLine() { }

    public ItemTrackingLine(Guid id, ItemTrackingSourceType sourceType, Guid sourceId, string itemNo)
        : base(id)
    {
        SourceType = sourceType;
        SourceId = sourceId;
        ItemNo = Check.NotNullOrWhiteSpace(itemNo, nameof(itemNo), ErpDomainConsts.MaxNoLength);
    }

    public void SetTracking(string lotNo, string serialNo, decimal quantity, DateTime? expirationDate)
    {
        LotNo = lotNo.IsNullOrWhiteSpace() ? null : Check.Length(lotNo.Trim(), nameof(lotNo), ErpDomainConsts.MaxLotNoLength);
        SerialNo = serialNo.IsNullOrWhiteSpace() ? null : Check.Length(serialNo.Trim(), nameof(serialNo), ErpDomainConsts.MaxLotNoLength);
        Quantity = Math.Abs(quantity);
        ExpirationDate = expirationDate?.Date;
    }

    internal void Handle(decimal quantity)
    {
        QuantityHandled += quantity;
    }
}

/// <summary>One lot or serial number of a posting, with its share of the quantity.</summary>
public sealed record TrackingPart(string LotNo, string SerialNo, decimal Quantity, DateTime? ExpirationDate)
{
    public static readonly TrackingPart[] None = [];

    public string TrackingNo => SerialNo ?? LotNo;
}

/// <summary>
/// Splits a posting into its lots and serial numbers. An item without a tracking code posts in one
/// part; a tracked item needs tracking lines for the whole quantity, each serial number one unit.
/// </summary>
public class ItemTrackingManager : DomainService
{
    private readonly IRepository<ItemTrackingLine, Guid> _lineRepository;
    private readonly IRepository<ItemTrackingCode, Guid> _codeRepository;

    public ItemTrackingManager(IRepository<ItemTrackingLine, Guid> lineRepository, IRepository<ItemTrackingCode, Guid> codeRepository)
    {
        _lineRepository = lineRepository;
        _codeRepository = codeRepository;
    }

    public async Task<ItemTrackingCode> GetCodeAsync(Item item)
    {
        return item.ItemTrackingCode == null ? null : await _codeRepository.FirstOrDefaultAsync(c => c.Code == item.ItemTrackingCode);
    }

    /// <summary>
    /// The parts of <paramref name="quantity"/> base units of the line, taken from its tracking lines
    /// in order and marked as handled. Null for an item that is not tracked.
    /// </summary>
    public async Task<IReadOnlyList<TrackingPart>> TakeAsync(Item item, ItemTrackingSourceType sourceType, Guid sourceId, decimal quantity)
    {
        var code = await GetCodeAsync(item);
        if (code == null || !(code.LotSpecific || code.SerialSpecific))
        {
            return null;
        }

        var lines = (await _lineRepository.GetListAsync(l => l.SourceType == sourceType && l.SourceId == sourceId))
            .OrderBy(l => l.CreationTime)
            .ThenBy(l => l.Id)
            .ToList();

        var needed = Math.Abs(quantity);
        var tracked = lines.Sum(l => l.QuantityRemaining);
        if (tracked < needed)
        {
            throw new BusinessException(lines.Count == 0 ? ErpErrorCodes.Inventory.TrackingRequired : ErpErrorCodes.Inventory.TrackingQuantityMismatch)
                .WithData("itemNo", item.No)
                .WithData("quantity", needed)
                .WithData("tracked", tracked);
        }

        var parts = new List<TrackingPart>();
        foreach (var line in lines.Where(l => l.QuantityRemaining > 0m))
        {
            if (needed == 0m)
            {
                break;
            }

            Validate(item, code, line);
            var take = Math.Min(needed, line.QuantityRemaining);
            parts.Add(new TrackingPart(line.LotNo, line.SerialNo, take, line.ExpirationDate));
            line.Handle(take);
            await _lineRepository.UpdateAsync(line);
            needed -= take;
        }

        return parts;
    }

    /// <summary>The checks a tracking line has to pass for the item's tracking code.</summary>
    public static void Validate(Item item, ItemTrackingCode code, ItemTrackingLine line)
    {
        if ((code.LotSpecific && line.LotNo == null) || (code.SerialSpecific && line.SerialNo == null))
        {
            throw new BusinessException(ErpErrorCodes.Inventory.TrackingRequired)
                .WithData("itemNo", item.No)
                .WithData("quantity", line.Quantity);
        }
        if (code.SerialSpecific && line.Quantity != 1m)
        {
            throw new BusinessException(ErpErrorCodes.Inventory.SerialQuantityNotOne).WithData("itemNo", item.No);
        }
    }

    /// <summary>The tracking lines of a source line, to show or copy.</summary>
    public Task<List<ItemTrackingLine>> GetLinesAsync(ItemTrackingSourceType sourceType, Guid sourceId)
    {
        return _lineRepository.GetListAsync(l => l.SourceType == sourceType && l.SourceId == sourceId);
    }
}
