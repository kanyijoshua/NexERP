using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.CostAccounting;

/// <summary>
/// Cost Accounting Setup: which dimensions are the cost centers and cost objects, and how far the
/// G/L has been transferred to cost accounting.
/// </summary>
public class CostAccountingSetup : CompanyEntity
{
    public string CostCenterDimension { get; private set; }
    public string CostObjectDimension { get; private set; }
    public long LastGLEntryNoTransferred { get; private set; }

    protected CostAccountingSetup() { }

    public CostAccountingSetup(Guid id)
        : base(id) { }

    public void Set(string costCenterDimension, string costObjectDimension)
    {
        CostCenterDimension = CodeTableEntity.NormalizeCode(Check.Length(costCenterDimension, nameof(costCenterDimension), ErpDomainConsts.MaxCodeLength));
        CostObjectDimension = CodeTableEntity.NormalizeCode(Check.Length(costObjectDimension, nameof(costObjectDimension), ErpDomainConsts.MaxCodeLength));
    }

    internal void RecordTransfer(long lastEntryNo) => LastGLEntryNoTransferred = Math.Max(LastGLEntryNoTransferred, lastEntryNo);
}

/// <summary>
/// Cost Type: a line of the cost chart. A cost type takes in the G/L accounts in its range; a
/// heading only titles; a total adds up the cost types in its totaling range.
/// </summary>
public class CostType : CompanyEntity, IHasNo
{
    public string No { get; private set; }
    public string Name { get; private set; }
    public CostTypeKind Type { get; private set; }

    /// <summary>The G/L accounts whose entries go to this cost type, e.g. "6000..6999|7100".</summary>
    public string GLAccountRange { get; private set; }

    /// <summary>A total: the cost types it adds up.</summary>
    public string Totaling { get; private set; }

    /// <summary>Used when the G/L entry has no cost center (or object) dimension.</summary>
    public string CostCenterCode { get; private set; }

    public string CostObjectCode { get; private set; }
    public bool Blocked { get; private set; }

    protected CostType() { }

    public CostType(Guid id, string no)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim();
    }

    public void Set(string name, CostTypeKind type, string glAccountRange, string totaling, string costCenterCode, string costObjectCode, bool blocked)
    {
        Name = Check.Length(name, nameof(name), ErpDomainConsts.MaxNameLength);
        Type = type;
        GLAccountRange = type == CostTypeKind.CostType ? Check.Length(glAccountRange?.Trim(), nameof(glAccountRange), 250) : null;
        Totaling = type == CostTypeKind.Total ? Check.Length(totaling?.Trim(), nameof(totaling), 250) : null;
        CostCenterCode = CodeTableEntity.NormalizeCode(Check.Length(costCenterCode, nameof(costCenterCode), ErpDomainConsts.MaxCodeLength));
        CostObjectCode = CodeTableEntity.NormalizeCode(Check.Length(costObjectCode, nameof(costObjectCode), ErpDomainConsts.MaxCodeLength));
        Blocked = blocked;
    }
}

/// <summary>Cost Center: a department or activity that costs are collected for.</summary>
public class CostCenter : CodeTableEntity
{
    public bool Blocked { get; private set; }

    protected CostCenter() { }

    public CostCenter(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetBlocked(bool blocked) => Blocked = blocked;
}

/// <summary>Cost Object: a product, product group or service that costs are collected for.</summary>
public class CostObject : CodeTableEntity
{
    public bool Blocked { get; private set; }

    protected CostObject() { }

    public CostObject(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetBlocked(bool blocked) => Blocked = blocked;
}

/// <summary>Cost Entry: one amount on a cost type, for a cost center or cost object.</summary>
public class CostEntry : LedgerEntryBase
{
    public string CostTypeNo { get; private set; }
    public string CostCenterCode { get; private set; }
    public string CostObjectCode { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string DocumentNo { get; private set; }
    public string Description { get; private set; }
    public decimal Amount { get; private set; }

    /// <summary>The G/L entry it was transferred from; 0 for an allocation.</summary>
    public long GLEntryNo { get; private set; }

    public string GLAccountNo { get; private set; }

    /// <summary>The allocation that posted it; null for a transferred entry.</summary>
    public string AllocationCode { get; private set; }

    public long RegisterNo { get; private set; }

    protected CostEntry() { }

    public CostEntry(
        Guid id,
        string costTypeNo,
        string costCenterCode,
        string costObjectCode,
        DateTime postingDate,
        string documentNo,
        string description,
        decimal amount,
        long glEntryNo,
        string glAccountNo,
        string allocationCode,
        long registerNo
    )
        : base(id)
    {
        CostTypeNo = costTypeNo;
        CostCenterCode = costCenterCode;
        CostObjectCode = costObjectCode;
        PostingDate = postingDate.Date;
        DocumentNo = Check.Length(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Amount = amount;
        GLEntryNo = glEntryNo;
        GLAccountNo = glAccountNo;
        AllocationCode = allocationCode;
        RegisterNo = registerNo;
    }
}

/// <summary>Cost Register: the cost entries one transfer or allocation run posted.</summary>
public class CostRegister : CompanyBasicEntity
{
    public long No { get; private set; }
    public CostRegisterSource Source { get; private set; }
    public DateTime PostingDate { get; private set; }
    public long FromEntryNo { get; private set; }
    public long ToEntryNo { get; private set; }
    public int EntryCount { get; private set; }
    public decimal DebitAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected CostRegister() { }

    public CostRegister(Guid id, long no, CostRegisterSource source, DateTime postingDate, DateTime createdAt)
        : base(id)
    {
        No = no;
        Source = source;
        PostingDate = postingDate.Date;
        CreatedAt = createdAt;
    }

    internal void Add(CostEntry entry)
    {
        FromEntryNo = FromEntryNo == 0 ? entry.EntryNo : Math.Min(FromEntryNo, entry.EntryNo);
        ToEntryNo = Math.Max(ToEntryNo, entry.EntryNo);
        EntryCount++;
        if (entry.Amount > 0m)
        {
            DebitAmount += entry.Amount;
        }
    }
}

/// <summary>
/// Cost Allocation Source: what one allocation takes away (the balance of a range of cost types on
/// a cost center or object) and the targets it shares it out to. Allocations run by level, so a
/// center allocated to at one level can be allocated on at the next.
/// </summary>
public class CostAllocationSource : CompanyAggregateRoot
{
    public string Code { get; private set; }
    public string Description { get; private set; }
    public int Level { get; private set; } = 1;
    public string CostTypeRange { get; private set; }
    public string CostCenterCode { get; private set; }
    public string CostObjectCode { get; private set; }

    /// <summary>The cost type the source is credited on.</summary>
    public string CreditToCostTypeNo { get; private set; }

    public bool Blocked { get; private set; }

    public Collection<CostAllocationTarget> Targets { get; private set; } = new();

    protected CostAllocationSource() { }

    public CostAllocationSource(Guid id, string code)
        : base(id)
    {
        Code = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxCodeLength));
    }

    public void Set(string description, int level, string costTypeRange, string costCenterCode, string costObjectCode, string creditToCostTypeNo, bool blocked)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Level = Math.Clamp(level, 1, 99);
        CostTypeRange = Check.NotNullOrWhiteSpace(costTypeRange, nameof(costTypeRange), 250).Trim();
        CostCenterCode = CodeTableEntity.NormalizeCode(Check.Length(costCenterCode, nameof(costCenterCode), ErpDomainConsts.MaxCodeLength));
        CostObjectCode = CodeTableEntity.NormalizeCode(Check.Length(costObjectCode, nameof(costObjectCode), ErpDomainConsts.MaxCodeLength));
        if (CostCenterCode == null && CostObjectCode == null)
        {
            throw new BusinessException(ErpErrorCodes.CostAccounting.AllocationSourceMissing).WithData("code", Code);
        }

        CreditToCostTypeNo = Check.NotNullOrWhiteSpace(creditToCostTypeNo, nameof(creditToCostTypeNo), ErpDomainConsts.MaxNoLength).Trim();
        Blocked = blocked;
    }

    public CostAllocationTarget AddTarget(Guid id, string costTypeNo, string costCenterCode, string costObjectCode, decimal share)
    {
        var target = new CostAllocationTarget(id, Id, (Targets.Count == 0 ? 0 : Targets.Max(t => t.LineNo)) + 10000);
        target.Set(costTypeNo, costCenterCode, costObjectCode, share);
        Targets.Add(target);
        return target;
    }
}

public class CostAllocationTarget : Entity<Guid>
{
    public Guid CostAllocationSourceId { get; private set; }
    public int LineNo { get; private set; }
    public string CostTypeNo { get; private set; }
    public string CostCenterCode { get; private set; }
    public string CostObjectCode { get; private set; }

    /// <summary>The target's share; the percentages are the shares over their total.</summary>
    public decimal Share { get; private set; }

    protected CostAllocationTarget() { }

    public CostAllocationTarget(Guid id, Guid sourceId, int lineNo)
        : base(id)
    {
        CostAllocationSourceId = sourceId;
        LineNo = lineNo;
    }

    public void Set(string costTypeNo, string costCenterCode, string costObjectCode, decimal share)
    {
        CostTypeNo = Check.NotNullOrWhiteSpace(costTypeNo, nameof(costTypeNo), ErpDomainConsts.MaxNoLength).Trim();
        CostCenterCode = CodeTableEntity.NormalizeCode(Check.Length(costCenterCode, nameof(costCenterCode), ErpDomainConsts.MaxCodeLength));
        CostObjectCode = CodeTableEntity.NormalizeCode(Check.Length(costObjectCode, nameof(costObjectCode), ErpDomainConsts.MaxCodeLength));
        Share = Math.Max(0m, share);
    }
}

/// <summary>A cost type's balance on one cost center or object over a period.</summary>
public sealed class CostBalance
{
    public string CostTypeNo { get; init; }
    public string CostTypeName { get; init; }
    public string CostCenterCode { get; init; }
    public string CostObjectCode { get; init; }
    public decimal Amount { get; init; }
}

/// <summary>
/// Cost accounting: transfers G/L entries to cost entries by cost type, cost center and cost object
/// (from the entries' dimensions), allocates the balances of cost centers and objects to others by
/// share, level by level, and reports the balances.
/// </summary>
public class CostAccountingManager : DomainService
{
    private readonly IRepository<CostAccountingSetup, Guid> _setupRepository;
    private readonly IRepository<CostType, Guid> _typeRepository;
    private readonly IRepository<CostEntry, Guid> _entryRepository;
    private readonly IRepository<CostRegister, Guid> _registerRepository;
    private readonly IRepository<CostAllocationSource, Guid> _allocationRepository;

    private IRepository<GLEntry, Guid> GLEntries => LazyServiceProvider.LazyGetRequiredService<IRepository<GLEntry, Guid>>();
    private IRepository<DimensionSetEntry, Guid> DimensionSetEntries => LazyServiceProvider.LazyGetRequiredService<IRepository<DimensionSetEntry, Guid>>();
    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public CostAccountingManager(
        IRepository<CostAccountingSetup, Guid> setupRepository,
        IRepository<CostType, Guid> typeRepository,
        IRepository<CostEntry, Guid> entryRepository,
        IRepository<CostRegister, Guid> registerRepository,
        IRepository<CostAllocationSource, Guid> allocationRepository
    )
    {
        _setupRepository = setupRepository;
        _typeRepository = typeRepository;
        _entryRepository = entryRepository;
        _registerRepository = registerRepository;
        _allocationRepository = allocationRepository;
    }

    public async Task<CostAccountingSetup> GetSetupAsync() =>
        await _setupRepository.FirstOrDefaultAsync() ?? await _setupRepository.InsertAsync(new CostAccountingSetup(GuidGenerator.Create()), autoSave: true);

    /// <summary>
    /// Transfers the G/L entries posted since the last transfer to the cost types whose G/L range
    /// takes their account; entries of accounts no cost type takes stay out. Returns the register,
    /// or null when there was nothing to transfer.
    /// </summary>
    public async Task<CostRegister> TransferGLEntriesAsync(DateTime runDate)
    {
        var setup = await GetSetupAsync();
        var last = setup.LastGLEntryNoTransferred;
        var glEntries = (await GLEntries.GetListAsync(e => e.EntryNo > last)).OrderBy(e => e.EntryNo).ToList();
        if (glEntries.Count == 0)
        {
            return null;
        }

        var types = (await _typeRepository.GetListAsync(t => t.Type == CostTypeKind.CostType && !t.Blocked && t.GLAccountRange != null))
            .Select(t => (Type: t, Range: AccountTotaling.Parse(t.GLAccountRange)))
            .OrderBy(t => t.Type.No)
            .ToList();
        var setIds = glEntries.Select(e => e.DimensionSetId).Where(id => id != Guid.Empty).Distinct().ToList();
        var dimensions = (await DimensionSetEntries.GetListAsync(d => setIds.Contains(d.DimensionSetId)))
            .GroupBy(d => d.DimensionSetId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(d => d.DimensionCode, d => d.DimensionValueCode));

        CostRegister register = null;
        foreach (var gl in glEntries)
        {
            var match = types.FirstOrDefault(t => t.Range.Matches(gl.GLAccountNo));
            if (match.Type == null)
            {
                continue;
            }

            var values = dimensions.GetValueOrDefault(gl.DimensionSetId);
            var center = Value(values, setup.CostCenterDimension) ?? match.Type.CostCenterCode;
            var costObject = Value(values, setup.CostObjectDimension) ?? match.Type.CostObjectCode;
            register ??= await NewRegisterAsync(CostRegisterSource.TransferFromGL, runDate);
            await InsertAsync(register, match.Type.No, center, costObject, gl.PostingDate, gl.DocumentNo, gl.Description, gl.Amount, gl.EntryNo, gl.GLAccountNo, null);
        }

        setup.RecordTransfer(glEntries[^1].EntryNo);
        await _setupRepository.UpdateAsync(setup, autoSave: true);
        if (register != null)
        {
            await _registerRepository.UpdateAsync(register, autoSave: true);
        }

        return register;
    }

    /// <summary>
    /// Runs the allocations of the levels from <paramref name="fromLevel"/> to <paramref name="toLevel"/>
    /// in order: each credits its source with the balance up to the posting date and debits its
    /// targets by share, the last target taking what rounding leaves.
    /// </summary>
    public async Task<CostRegister> AllocateAsync(int fromLevel, int toLevel, DateTime postingDate, string documentNo)
    {
        var query = await _allocationRepository.WithDetailsAsync(a => a.Targets);
        var sources = (await AsyncExecuter.ToListAsync(query.Where(a => !a.Blocked && a.Level >= fromLevel && a.Level <= toLevel)))
            .OrderBy(a => a.Level)
            .ThenBy(a => a.Code)
            .ToList();
        CostRegister register = null;

        foreach (var source in sources)
        {
            var targets = source.Targets.Where(t => t.Share > 0m).OrderBy(t => t.LineNo).ToList();
            var totalShare = targets.Sum(t => t.Share);
            if (totalShare == 0m)
            {
                throw new BusinessException(ErpErrorCodes.CostAccounting.AllocationSharesZero).WithData("code", source.Code);
            }

            var range = AccountTotaling.Parse(source.CostTypeRange);
            var date = postingDate.Date;
            var balance = (await _entryRepository.GetListAsync(e =>
                    e.PostingDate <= date
                    && (source.CostCenterCode == null || e.CostCenterCode == source.CostCenterCode)
                    && (source.CostObjectCode == null || e.CostObjectCode == source.CostObjectCode)))
                .Where(e => range.Matches(e.CostTypeNo))
                .Sum(e => e.Amount);
            if (balance == 0m)
            {
                continue;
            }

            register ??= await NewRegisterAsync(CostRegisterSource.Allocation, postingDate);
            var description = source.Description ?? source.Code;
            await InsertAsync(register, source.CreditToCostTypeNo, source.CostCenterCode, source.CostObjectCode, date, documentNo, description, -balance, 0, null, source.Code);

            var given = 0m;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                var amount = i == targets.Count - 1 ? balance - given : Math.Round(balance * target.Share / totalShare, 2, MidpointRounding.AwayFromZero);
                given += amount;
                await InsertAsync(register, target.CostTypeNo, target.CostCenterCode, target.CostObjectCode, date, documentNo, description, amount, 0, null, source.Code);
            }
        }

        if (register != null)
        {
            await _registerRepository.UpdateAsync(register, autoSave: true);
        }

        return register;
    }

    /// <summary>Balances by cost type and cost center or object over a period.</summary>
    public async Task<List<CostBalance>> GetBalancesAsync(DateTime fromDate, DateTime toDate, bool byCostObject)
    {
        var from = fromDate.Date;
        var to = toDate.Date;
        var names = (await _typeRepository.GetListAsync()).ToDictionary(t => t.No, t => t.Name);
        return (await _entryRepository.GetListAsync(e => e.PostingDate >= from && e.PostingDate <= to))
            .GroupBy(e => new { e.CostTypeNo, Code = byCostObject ? e.CostObjectCode : e.CostCenterCode })
            .Select(g => new CostBalance
            {
                CostTypeNo = g.Key.CostTypeNo,
                CostTypeName = names.GetValueOrDefault(g.Key.CostTypeNo),
                CostCenterCode = byCostObject ? null : g.Key.Code,
                CostObjectCode = byCostObject ? g.Key.Code : null,
                Amount = g.Sum(e => e.Amount),
            })
            .Where(b => b.Amount != 0m)
            .OrderBy(b => b.CostTypeNo)
            .ThenBy(b => b.CostCenterCode ?? b.CostObjectCode)
            .ToList();
    }

    private static string Value(Dictionary<string, string> values, string dimension) =>
        dimension == null || values == null ? null : values.GetValueOrDefault(dimension);

    private async Task<CostRegister> NewRegisterAsync(CostRegisterSource source, DateTime postingDate)
    {
        var register = new CostRegister(GuidGenerator.Create(), await EntryNos.NextAsync(ErpSequenceNames.CostRegister), source, postingDate, Clock.Now);
        return await _registerRepository.InsertAsync(register, autoSave: true);
    }

    private async Task InsertAsync(CostRegister register, string costTypeNo, string center, string costObject, DateTime date, string documentNo, string description, decimal amount, long glEntryNo, string glAccountNo, string allocationCode)
    {
        var entry = new CostEntry(GuidGenerator.Create(), costTypeNo, center, costObject, date, documentNo, description, amount, glEntryNo, glAccountNo, allocationCode, register.No)
        {
            EntryNo = await EntryNos.NextAsync(ErpSequenceNames.CostEntry),
        };
        await _entryRepository.InsertAsync(entry, autoSave: true);
        register.Add(entry);
    }
}
