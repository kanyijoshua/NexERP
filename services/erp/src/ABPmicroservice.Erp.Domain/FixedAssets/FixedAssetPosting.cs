using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.FixedAssets;

/// <summary>FA Journal Batch: a set of fixed asset journal lines posted together, for one depreciation book.</summary>
public class FAJournalBatch : CompanyAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }

    /// <summary>The book the lines post to unless a line names another; blank means the setup's default book.</summary>
    public string DepreciationBookCode { get; private set; }

    protected FAJournalBatch() { }

    public FAJournalBatch(Guid id, string name, string description, string depreciationBookCode)
        : base(id)
    {
        SetName(name);
        Set(description, depreciationBookCode);
    }

    public void SetName(string name) => Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxJournalTemplateNameLength).Trim().ToUpperInvariant();

    public void Set(string description, string depreciationBookCode)
    {
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        DepreciationBookCode = CodeTableEntity.NormalizeCode(Check.Length(depreciationBookCode, nameof(depreciationBookCode), 10));
    }
}

/// <summary>
/// FA Journal Line: an acquisition, depreciation, write-down, appreciation or disposal of one fixed
/// asset. Acquisitions and disposals may name the account that pays or receives the money.
/// </summary>
public class FAJournalLine : CompanyEntity
{
    public Guid FAJournalBatchId { get; private set; }
    public int LineNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public string DocumentNo { get; private set; }
    public string FANo { get; private set; }
    public string DepreciationBookCode { get; private set; }
    public FAJournalPostingType FAPostingType { get; private set; }
    public string Description { get; private set; }

    /// <summary>
    /// Positive for an acquisition or appreciation; negative for depreciation and write-down; the
    /// proceeds (positive) for a disposal.
    /// </summary>
    public decimal Amount { get; private set; }

    public GenJournalAccountType? BalAccountType { get; private set; }
    public string BalAccountNo { get; private set; }

    /// <summary>The days a calculated depreciation covers.</summary>
    public int NoOfDepreciationDays { get; private set; }

    protected FAJournalLine() { }

    public FAJournalLine(Guid id, Guid batchId, int lineNo, DateTime postingDate, string documentNo, string faNo, FAJournalPostingType type, decimal amount)
        : base(id)
    {
        FAJournalBatchId = batchId;
        LineNo = lineNo;
        Update(postingDate, documentNo, faNo, type, amount, null);
    }

    public void Update(DateTime postingDate, string documentNo, string faNo, FAJournalPostingType type, decimal amount, string description)
    {
        PostingDate = postingDate.Date;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength).Trim();
        FANo = Check.NotNullOrWhiteSpace(faNo, nameof(faNo), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        FAPostingType = type;
        Amount = amount;
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }

    public void SetBook(string depreciationBookCode) =>
        DepreciationBookCode = CodeTableEntity.NormalizeCode(Check.Length(depreciationBookCode, nameof(depreciationBookCode), 10));

    public void SetBalancing(GenJournalAccountType? balAccountType, string balAccountNo)
    {
        BalAccountNo = balAccountNo.IsNullOrWhiteSpace() ? null : Check.Length(balAccountNo.Trim(), nameof(balAccountNo), ErpDomainConsts.MaxNoLength);
        BalAccountType = BalAccountNo == null ? null : balAccountType ?? GenJournalAccountType.GLAccount;
    }

    internal void SetDepreciationDays(int days) => NoOfDepreciationDays = days;
}

/// <summary>What to post to a fixed asset.</summary>
public sealed class FAPostingRequest
{
    public string FANo { get; init; }
    public string DepreciationBookCode { get; init; }
    public FAJournalPostingType PostingType { get; init; }
    public DateTime PostingDate { get; init; }
    public GLEntryDocumentType DocumentType { get; init; }
    public string DocumentNo { get; init; }
    public string Description { get; init; }

    /// <summary>In LCY, with the sign the FA ledger takes (see <see cref="FAJournalLine.Amount"/>).</summary>
    public decimal Amount { get; init; }

    public GenJournalAccountType? BalAccountType { get; init; }
    public string BalAccountNo { get; init; }
    public int NoOfDepreciationDays { get; init; }

    /// <summary>
    /// False when a document already posts the G/L side itself (a purchase invoice acquiring the
    /// asset): only the FA ledger is written then.
    /// </summary>
    public bool PostGL { get; init; } = true;

    public string SourceNo { get; init; }
}

/// <summary>What an asset is carried at in one book.</summary>
public sealed record FABookValues(decimal AcquisitionCost, decimal Depreciation, decimal WriteDown, decimal Appreciation, decimal BookValue, decimal Proceeds, decimal GainLoss)
{
    public decimal DepreciableBasis => AcquisitionCost;
}

/// <summary>The depreciation of one asset for one period.</summary>
public sealed record DepreciationResult(decimal Amount, int Days);

/// <summary>Everything the depreciation of a period depends on.</summary>
public sealed record DepreciationInput(
    FADepreciationMethod Method,
    DateTime StartingDate,
    DateTime? EndingDate,
    decimal NoOfDepreciationYears,
    decimal StraightLinePct,
    decimal FixedDeprAmount,
    decimal DecliningBalancePct,
    decimal DepreciableBasis,
    decimal BookValue,
    decimal EndingBookValue,
    decimal FinalRoundingAmount,
    bool UseHalfYearConvention,
    bool FiscalYear365Days,
    DateTime? LastDepreciationDate
);

/// <summary>
/// Depreciation of one period, pure so it can be tested and projected. Days are counted on a
/// 360-day year (30 days a month) unless the book uses a 365-day year. Straight-line spreads what
/// is left above the ending book value over what is left of the life, so the last period takes the
/// rounding; declining balance takes a percentage of the book value; DB1/SL takes the larger of the
/// two. The half-year convention starts the life in the middle of the year of acquisition. The book
/// value never goes below the ending book value, and a remainder under the final rounding amount is
/// depreciated at once.
/// </summary>
public static class DepreciationCalculator
{
    public static DepreciationResult Calculate(DepreciationInput input, DateTime untilDate)
    {
        if (input.Method is FADepreciationMethod.Manual or FADepreciationMethod.UserDefined)
        {
            return new DepreciationResult(0m, 0);
        }

        var start = input.UseHalfYearConvention ? new DateTime(input.StartingDate.Year, 7, 1) : input.StartingDate.Date;
        var ending = input.EndingDate?.Date ?? LifeEnd(input, start);
        var from = input.LastDepreciationDate.HasValue && input.LastDepreciationDate.Value.Date >= start ? input.LastDepreciationDate.Value.Date.AddDays(1) : start;
        var to = untilDate.Date;
        if (ending.HasValue && to > ending.Value)
        {
            to = ending.Value;
        }

        var depreciable = input.BookValue - input.EndingBookValue;
        if (to < from || depreciable <= 0m)
        {
            return new DepreciationResult(0m, 0);
        }

        var days = Days(from, to, input.FiscalYear365Days);
        var yearDays = input.FiscalYear365Days ? 365m : 360m;

        decimal StraightLine()
        {
            if (input.NoOfDepreciationYears > 0m && ending.HasValue)
            {
                var remaining = Days(from, ending.Value, input.FiscalYear365Days);
                return remaining <= 0 ? depreciable : depreciable * Math.Min(days, remaining) / remaining;
            }
            if (input.StraightLinePct > 0m)
            {
                return input.DepreciableBasis * input.StraightLinePct / 100m * days / yearDays;
            }
            return input.FixedDeprAmount * days / yearDays;
        }

        decimal DecliningBalance() => input.BookValue * input.DecliningBalancePct / 100m * days / yearDays;

        var amount = input.Method switch
        {
            FADepreciationMethod.DecliningBalance1 or FADepreciationMethod.DecliningBalance2 => DecliningBalance(),
            FADepreciationMethod.Db1SL or FADepreciationMethod.Db2SL => Math.Max(DecliningBalance(), StraightLine()),
            _ => StraightLine(),
        };

        amount = Math.Round(Math.Min(amount, depreciable), 2, MidpointRounding.AwayFromZero);
        if (depreciable - amount < input.FinalRoundingAmount || (ending.HasValue && to == ending.Value && input.NoOfDepreciationYears > 0m))
        {
            amount = depreciable;
        }

        return new DepreciationResult(-amount, days);
    }

    /// <summary>The last day of the asset's life, from its years of depreciation.</summary>
    public static DateTime? LifeEnd(DepreciationInput input, DateTime start)
    {
        if (input.NoOfDepreciationYears <= 0m)
        {
            return null;
        }

        var months = (int)Math.Round(input.NoOfDepreciationYears * 12m, MidpointRounding.AwayFromZero);
        return start.AddMonths(months).AddDays(-1);
    }

    /// <summary>The days from one date to another, both included, on a 30/360 or an actual calendar.</summary>
    public static int Days(DateTime from, DateTime to, bool actual)
    {
        if (to < from)
        {
            return 0;
        }

        if (actual)
        {
            return (to.Date - from.Date).Days + 1;
        }

        static int Day(DateTime d) => d.Day == 31 || (d.Month == 2 && d.Day == DateTime.DaysInMonth(d.Year, 2)) ? 30 : d.Day;
        return (to.Year - from.Year) * 360 + (to.Month - from.Month) * 30 + (Day(to) - Day(from)) + 1;
    }
}

/// <summary>
/// Posts to fixed assets: the FA ledger entry of the book and, where the book integrates that kind
/// of posting with the G/L, the G/L entries on the FA posting group's accounts. Disposal reverses
/// the acquisition cost and depreciation onto the disposal accounts and books the gain or loss, so
/// the asset's book value ends at zero.
/// </summary>
public class FAJnlPostLine : DomainService
{
    private readonly IRepository<FixedAsset, Guid> _assetRepository;
    private readonly IRepository<FADepreciationBook, Guid> _assetBookRepository;
    private readonly IRepository<FALedgerEntry, Guid> _entryRepository;

    private IRepository<DepreciationBook, Guid> BookRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<DepreciationBook, Guid>>();
    private IRepository<FAPostingGroup, Guid> GroupRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<FAPostingGroup, Guid>>();
    private FASetupManager SetupManager => LazyServiceProvider.LazyGetRequiredService<FASetupManager>();
    private GenJnlPostLine GenJnlPostLine => LazyServiceProvider.LazyGetRequiredService<GenJnlPostLine>();
    private IEntryNoGenerator EntryNoGenerator => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public FAJnlPostLine(
        IRepository<FixedAsset, Guid> assetRepository,
        IRepository<FADepreciationBook, Guid> assetBookRepository,
        IRepository<FALedgerEntry, Guid> entryRepository
    )
    {
        _assetRepository = assetRepository;
        _assetBookRepository = assetBookRepository;
        _entryRepository = entryRepository;
    }

    /// <summary>The book the setup posts to when nothing else is named.</summary>
    public async Task<string> DefaultBookAsync() =>
        (await SetupManager.GetAsync())?.DefaultDeprBook
        ?? throw new BusinessException(ErpErrorCodes.FixedAssets.DepreciationBookNotFound).WithData("code", "");

    public async Task<FixedAsset> GetAssetAsync(string faNo)
    {
        var no = faNo?.Trim().ToUpperInvariant();
        return await _assetRepository.FirstOrDefaultAsync(a => a.No == no)
            ?? throw new BusinessException(ErpErrorCodes.FixedAssets.FixedAssetNotFound).WithData("no", no ?? "");
    }

    public async Task<FADepreciationBook> GetAssetBookAsync(string faNo, string bookCode)
    {
        var no = faNo?.Trim().ToUpperInvariant();
        var code = CodeTableEntity.NormalizeCode(bookCode) ?? await DefaultBookAsync();
        return await _assetBookRepository.FirstOrDefaultAsync(b => b.FANo == no && b.DepreciationBookCode == code)
            ?? throw new BusinessException(ErpErrorCodes.FixedAssets.AssetNotInBook).WithData("no", no ?? "").WithData("book", code);
    }

    /// <summary>Acquisition cost, depreciation, write-downs and appreciation of an asset in a book, and its book value.</summary>
    public async Task<FABookValues> GetValuesAsync(string faNo, string bookCode, DateTime? until = null)
    {
        var no = faNo?.Trim().ToUpperInvariant();
        var code = CodeTableEntity.NormalizeCode(bookCode) ?? await DefaultBookAsync();
        var entries = await _entryRepository.GetListAsync(e => e.FANo == no && e.DepreciationBookCode == code);
        if (until.HasValue)
        {
            entries = entries.Where(e => e.FAPostingDate <= until.Value.Date).ToList();
        }

        decimal Sum(FALedgerEntryFAPostingType type) => entries.Where(e => e.FAPostingType == type).Sum(e => e.Amount);
        return new FABookValues(
            Sum(FALedgerEntryFAPostingType.AcquisitionCost),
            Sum(FALedgerEntryFAPostingType.Depreciation),
            Sum(FALedgerEntryFAPostingType.WriteDown),
            Sum(FALedgerEntryFAPostingType.Appreciation),
            entries.Where(e => e.PartOfBookValue).Sum(e => e.Amount),
            Sum(FALedgerEntryFAPostingType.ProceedsOnDisposal),
            Sum(FALedgerEntryFAPostingType.GainLoss)
        );
    }

    /// <summary>Posts one acquisition, depreciation, write-down, appreciation or disposal.</summary>
    public async Task<List<FALedgerEntry>> PostAsync(FAPostingRequest request, GLPostingContext context)
    {
        Check.NotNull(request, nameof(request));
        var asset = await GetAssetAsync(request.FANo);
        if (asset.Blocked || asset.Inactive)
        {
            throw new BusinessException(ErpErrorCodes.FixedAssets.FixedAssetBlocked).WithData("no", asset.No);
        }

        var assetBook = await GetAssetBookAsync(asset.No, request.DepreciationBookCode);
        if (assetBook.DisposalDate.HasValue)
        {
            throw new BusinessException(ErpErrorCodes.FixedAssets.AlreadyDisposed).WithData("no", asset.No).WithData("book", assetBook.DepreciationBookCode);
        }

        var book = await BookRepository.FirstOrDefaultAsync(b => b.Code == assetBook.DepreciationBookCode)
            ?? throw new BusinessException(ErpErrorCodes.FixedAssets.DepreciationBookNotFound).WithData("code", assetBook.DepreciationBookCode);
        var group = await GroupAsync(asset, assetBook);
        var date = request.PostingDate.Date;
        var entries = new List<FALedgerEntry>();

        if (request.PostingType == FAJournalPostingType.Disposal)
        {
            entries.AddRange(await DisposeAsync(request, asset, assetBook, book, group, context));
        }
        else
        {
            if (request.Amount == 0m)
            {
                return entries;
            }

            var type = request.PostingType switch
            {
                FAJournalPostingType.Depreciation => FALedgerEntryFAPostingType.Depreciation,
                FAJournalPostingType.WriteDown => FALedgerEntryFAPostingType.WriteDown,
                FAJournalPostingType.Appreciation => FALedgerEntryFAPostingType.Appreciation,
                _ => FALedgerEntryFAPostingType.AcquisitionCost,
            };
            if (type == FALedgerEntryFAPostingType.AcquisitionCost && !assetBook.AcquisitionDate.HasValue && request.Amount < 0m)
            {
                throw new BusinessException(ErpErrorCodes.FixedAssets.NotAcquired).WithData("no", asset.No);
            }
            if (type != FALedgerEntryFAPostingType.AcquisitionCost && !assetBook.AcquisitionDate.HasValue)
            {
                throw new BusinessException(ErpErrorCodes.FixedAssets.NotAcquired).WithData("no", asset.No);
            }

            entries.Add(await InsertAsync(asset, assetBook, request, type, FALedgerEntryFAPostingCategory.None, request.Amount, partOfBookValue: true, context));

            if (request.PostGL && Integrated(book, type))
            {
                var (assetAccount, counterAccount) = type switch
                {
                    FALedgerEntryFAPostingType.Depreciation => (group.AccumDepreciationAccount, group.DepreciationExpenseAcc),
                    FALedgerEntryFAPostingType.WriteDown => (group.WriteDownAccount, group.WriteDownExpenseAcc),
                    FALedgerEntryFAPostingType.Appreciation => (group.AppreciationAccount, group.AppreciationBalAccount),
                    _ => (group.AcquisitionCostAccount, group.AcquisitionCostBalAcc),
                };

                await GLAsync(Required(assetAccount, group, type.ToString()), request.Amount, request, asset, context);
                await BalanceAsync(request, asset, -request.Amount, counterAccount, group, type.ToString(), context);
            }

            switch (type)
            {
                case FALedgerEntryFAPostingType.AcquisitionCost:
                    assetBook.AcquisitionDate ??= date;
                    if (request.PostGL && Integrated(book, type))
                    {
                        assetBook.GLAcquisitionDate ??= date;
                    }
                    assetBook.LastAcquisitionCostDate = date;
                    assetBook.StartDepreciation(date);
                    break;
                case FALedgerEntryFAPostingType.Depreciation:
                    assetBook.LastDepreciationDate = assetBook.LastDepreciationDate.HasValue && assetBook.LastDepreciationDate > date ? assetBook.LastDepreciationDate : date;
                    break;
                case FALedgerEntryFAPostingType.WriteDown:
                    assetBook.LastWriteDownDate = date;
                    break;
                case FALedgerEntryFAPostingType.Appreciation:
                    assetBook.LastAppreciationDate = date;
                    break;
            }
        }

        await _assetBookRepository.UpdateAsync(assetBook);
        return entries;
    }

    /// <summary>
    /// Disposes of the asset in the book: proceeds, the reversal of everything that makes up its
    /// book value, and the gain or loss. On the G/L the reversals and the gain or loss together
    /// credit exactly the proceeds, which the balancing account (or the document that sold the
    /// asset) debits.
    /// </summary>
    private async Task<List<FALedgerEntry>> DisposeAsync(FAPostingRequest request, FixedAsset asset, FADepreciationBook assetBook, DepreciationBook book, FAPostingGroup group, GLPostingContext context)
    {
        if (!assetBook.AcquisitionDate.HasValue)
        {
            throw new BusinessException(ErpErrorCodes.FixedAssets.NotAcquired).WithData("no", asset.No);
        }

        var values = await GetValuesAsync(asset.No, assetBook.DepreciationBookCode);
        var proceeds = Math.Abs(request.Amount);
        var gainLoss = values.BookValue - proceeds; // negative is a gain
        var entries = new List<FALedgerEntry>
        {
            await InsertAsync(asset, assetBook, request, FALedgerEntryFAPostingType.ProceedsOnDisposal, FALedgerEntryFAPostingCategory.None, -proceeds, partOfBookValue: false, context),
        };

        foreach (var (type, amount) in new[]
        {
            (FALedgerEntryFAPostingType.AcquisitionCost, values.AcquisitionCost),
            (FALedgerEntryFAPostingType.Depreciation, values.Depreciation),
            (FALedgerEntryFAPostingType.WriteDown, values.WriteDown),
            (FALedgerEntryFAPostingType.Appreciation, values.Appreciation),
        })
        {
            if (amount != 0m)
            {
                entries.Add(await InsertAsync(asset, assetBook, request, type, FALedgerEntryFAPostingCategory.Disposal, -amount, partOfBookValue: true, context));
            }
        }

        if (gainLoss != 0m)
        {
            entries.Add(await InsertAsync(asset, assetBook, request, FALedgerEntryFAPostingType.GainLoss, FALedgerEntryFAPostingCategory.None, gainLoss, partOfBookValue: false, context));
        }

        if (request.PostGL && book.GLIntegrationDisposal)
        {
            await GLAsync(Required(group.AcqCostAccOnDisposal ?? group.AcquisitionCostAccount, group, "AcqCostAccOnDisposal"), -values.AcquisitionCost, request, asset, context);
            await GLAsync(Required(group.AccumDeprAccOnDisposal ?? group.AccumDepreciationAccount, group, "AccumDeprAccOnDisposal"), -values.Depreciation, request, asset, context);
            await GLAsync(group.WriteDownAccOnDisposal ?? group.WriteDownAccount, -values.WriteDown, request, asset, context);
            await GLAsync(group.AppreciationAccOnDisposal ?? group.AppreciationAccount, -values.Appreciation, request, asset, context);
            await GLAsync(Required(gainLoss < 0m ? group.GainsAccOnDisposal : group.LossesAccOnDisposal, group, gainLoss < 0m ? "GainsAccOnDisposal" : "LossesAccOnDisposal"), gainLoss, request, asset, context);
            await BalanceAsync(request, asset, proceeds, group.SalesBalAcc, group, null, context);
        }

        assetBook.DisposalDate = request.PostingDate.Date;
        return entries;
    }

    private async Task<FALedgerEntry> InsertAsync(
        FixedAsset asset,
        FADepreciationBook assetBook,
        FAPostingRequest request,
        FALedgerEntryFAPostingType type,
        FALedgerEntryFAPostingCategory category,
        decimal amount,
        bool partOfBookValue,
        GLPostingContext context
    )
    {
        var entry = new FALedgerEntry(GuidGenerator.Create())
        {
            EntryNo = await EntryNoGenerator.NextAsync(ErpSequenceNames.FALedgerEntry),
            FANo = asset.No,
            FAPostingDate = request.PostingDate.Date,
            PostingDate = request.PostingDate.Date,
            DocumentType = request.DocumentType,
            DocumentDate = request.PostingDate.Date,
            DocumentNo = request.DocumentNo,
            Description = request.Description ?? asset.Description,
            DepreciationBookCode = assetBook.DepreciationBookCode,
            FAPostingCategory = category,
            FAPostingType = type,
            Amount = amount,
            DebitAmount = amount > 0m ? amount : 0m,
            CreditAmount = amount < 0m ? -amount : 0m,
            PartOfBookValue = partOfBookValue,
            PartOfDepreciableBasis = type == FALedgerEntryFAPostingType.AcquisitionCost,
            NoOfDepreciationDays = type == FALedgerEntryFAPostingType.Depreciation ? request.NoOfDepreciationDays : 0,
            Quantity = 0m,
            FASubclassCode = asset.FASubclassCode,
            FAClassCode = asset.FAClassCode,
            FALocationCode = asset.FALocationCode,
            FAPostingGroup = assetBook.FAPostingGroup ?? asset.FAPostingGroup,
            GlobalDimension1Code = asset.GlobalDimension1Code,
            GlobalDimension2Code = asset.GlobalDimension2Code,
            LocationCode = asset.LocationCode,
            UserId = LazyServiceProvider.LazyGetRequiredService<Volo.Abp.Users.ICurrentUser>().UserName,
            DepreciationMethod = assetBook.DepreciationMethod,
            DepreciationStartingDate = assetBook.DepreciationStartingDate,
            StraightLinePct = assetBook.StraightLinePct,
            NoOfDepreciationYears = assetBook.NoOfDepreciationYears,
            DepreciationEndingDate = assetBook.DepreciationEndingDate,
            SourceCode = context?.SourceCode,
            TransactionNo = context?.TransactionNo ?? 0,
            BalAccountType = request.BalAccountType ?? GenJournalAccountType.GLAccount,
            BalAccountNo = request.BalAccountNo,
        };
        await _entryRepository.InsertAsync(entry, autoSave: true);
        return entry;
    }

    private Task GLAsync(string accountNo, decimal amount, FAPostingRequest request, FixedAsset asset, GLPostingContext context)
    {
        if (amount == 0m || accountNo.IsNullOrWhiteSpace())
        {
            return Task.CompletedTask;
        }

        return GenJnlPostLine.PostGLDirectAsync(accountNo, request.PostingDate, request.DocumentType, request.DocumentNo, request.Description ?? asset.Description, amount, request.SourceNo ?? asset.No, context: context);
    }

    /// <summary>The other side: the request's balancing account when it names one, else the group's default.</summary>
    private async Task BalanceAsync(FAPostingRequest request, FixedAsset asset, decimal amount, string defaultAccount, FAPostingGroup group, string what, GLPostingContext context)
    {
        if (amount == 0m)
        {
            return;
        }

        if (!request.BalAccountNo.IsNullOrWhiteSpace())
        {
            var line = new GenJournalLine(
                GuidGenerator.Create(),
                Guid.Empty,
                1,
                request.PostingDate,
                request.DocumentType,
                request.DocumentNo,
                request.BalAccountType ?? GenJournalAccountType.GLAccount,
                request.BalAccountNo,
                request.Description ?? asset.Description,
                amount
            );
            await GenJnlPostLine.PostLineAsync(line, context);
            return;
        }

        if (what == null)
        {
            // A disposal without a balancing account is a sale on a document, which posts the other side.
            return;
        }

        await GLAsync(Required(defaultAccount, group, what + "Bal"), amount, request, asset, context);
    }

    /// <summary>The account an acquisition of the asset debits: what a purchase line for the asset posts to.</summary>
    public async Task<string> GetAcquisitionAccountAsync(string faNo, string bookCode = null)
    {
        var asset = await GetAssetAsync(faNo);
        var group = await GroupAsync(asset, await GetAssetBookAsync(asset.No, bookCode));
        return Required(group.AcquisitionCostAccount, group, "AcquisitionCostAccount");
    }

    private static string Required(string accountNo, FAPostingGroup group, string what) =>
        accountNo.IsNullOrWhiteSpace()
            ? throw new BusinessException(ErpErrorCodes.FixedAssets.PostingGroupAccountMissing).WithData("postingGroup", group.Code).WithData("account", what)
            : accountNo;

    private static bool Integrated(DepreciationBook book, FALedgerEntryFAPostingType type) =>
        type switch
        {
            FALedgerEntryFAPostingType.AcquisitionCost => book.GLIntegrationAcqCost,
            FALedgerEntryFAPostingType.Depreciation => book.GLIntegrationDepreciation,
            FALedgerEntryFAPostingType.WriteDown => book.GLIntegrationWriteDown,
            FALedgerEntryFAPostingType.Appreciation => book.GLIntegrationAppreciation,
            _ => book.GLIntegrationDisposal,
        };

    private async Task<FAPostingGroup> GroupAsync(FixedAsset asset, FADepreciationBook assetBook)
    {
        var code = assetBook.FAPostingGroup ?? asset.FAPostingGroup;
        return (code == null ? null : await GroupRepository.FirstOrDefaultAsync(g => g.Code == code))
            ?? throw new BusinessException(ErpErrorCodes.FixedAssets.PostingGroupNotFound).WithData("no", asset.No).WithData("postingGroup", code ?? "");
    }
}

/// <summary>
/// Posts FA journal batches, and fills them with the depreciation of every asset up to a date.
/// </summary>
public class FAJnlPostBatch : DomainService
{
    public const string SourceCode = "FAJNL";

    private readonly IRepository<FAJournalBatch, Guid> _batchRepository;
    private readonly IRepository<FAJournalLine, Guid> _lineRepository;
    private readonly FAJnlPostLine _postLine;

    private GLRegisterManager RegisterManager => LazyServiceProvider.LazyGetRequiredService<GLRegisterManager>();
    private GeneralLedgerSetupManager GLSetupManager => LazyServiceProvider.LazyGetRequiredService<GeneralLedgerSetupManager>();

    public FAJnlPostBatch(IRepository<FAJournalBatch, Guid> batchRepository, IRepository<FAJournalLine, Guid> lineRepository, FAJnlPostLine postLine)
    {
        _batchRepository = batchRepository;
        _lineRepository = lineRepository;
        _postLine = postLine;
    }

    /// <summary>Posts every line of the batch in one register and empties the batch. Returns the lines posted.</summary>
    public async Task<int> PostAsync(Guid batchId)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        var lines = (await _lineRepository.GetListAsync(l => l.FAJournalBatchId == batchId)).OrderBy(l => l.LineNo).ToList();
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.FixedAssets.NothingToPost).WithData("batch", batch.Name);
        }

        foreach (var date in lines.Select(l => l.PostingDate).Distinct())
        {
            await GLSetupManager.CheckPostingDateAsync(date);
        }

        var register = await RegisterManager.OpenAsync(lines[0].PostingDate, SourceCode, batch.Name);
        var context = new GLPostingContext(register, SourceCode);
        foreach (var line in lines)
        {
            await _postLine.PostAsync(
                new FAPostingRequest
                {
                    FANo = line.FANo,
                    DepreciationBookCode = line.DepreciationBookCode ?? batch.DepreciationBookCode,
                    PostingType = line.FAPostingType,
                    PostingDate = line.PostingDate,
                    DocumentType = GLEntryDocumentType.None,
                    DocumentNo = line.DocumentNo,
                    Description = line.Description,
                    Amount = line.Amount,
                    BalAccountType = line.BalAccountType,
                    BalAccountNo = line.BalAccountNo,
                    NoOfDepreciationDays = line.NoOfDepreciationDays,
                },
                context
            );
            await _lineRepository.DeleteAsync(line);
        }

        await RegisterManager.CloseAsync(register);
        return lines.Count;
    }

    /// <summary>
    /// Adds a depreciation line for every acquired, undisposed asset of the batch's book (or the one
    /// asset given) for the days after its last depreciation up to <paramref name="untilDate"/>.
    /// An asset already depreciated up to that date gets no line. Returns the lines added.
    /// </summary>
    public async Task<int> CalculateDepreciationAsync(Guid batchId, DateTime untilDate, DateTime postingDate, string documentNo, string faNo = null)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        var bookCode = batch.DepreciationBookCode ?? await _postLine.DefaultBookAsync();
        var book = await LazyServiceProvider.LazyGetRequiredService<IRepository<DepreciationBook, Guid>>().FirstOrDefaultAsync(b => b.Code == bookCode)
            ?? throw new BusinessException(ErpErrorCodes.FixedAssets.DepreciationBookNotFound).WithData("code", bookCode);

        var no = faNo?.Trim().ToUpperInvariant();
        var assetBooks = await LazyServiceProvider.LazyGetRequiredService<IRepository<FADepreciationBook, Guid>>()
            .GetListAsync(b => b.DepreciationBookCode == bookCode && b.DisposalDate == null && b.AcquisitionDate != null);
        var existing = await _lineRepository.GetListAsync(l => l.FAJournalBatchId == batchId);
        var lineNo = existing.Count == 0 ? 0 : existing.Max(l => l.LineNo);
        var added = 0;

        foreach (var assetBook in assetBooks.Where(b => no == null || b.FANo == no).OrderBy(b => b.FANo))
        {
            // A line not posted yet for the asset would be counted twice.
            foreach (var stale in existing.Where(l => l.FANo == assetBook.FANo && l.FAPostingType == FAJournalPostingType.Depreciation))
            {
                await _lineRepository.DeleteAsync(stale);
            }

            var values = await _postLine.GetValuesAsync(assetBook.FANo, bookCode);
            var result = DepreciationCalculator.Calculate(InputOf(assetBook, book, values), untilDate);
            if (result.Amount == 0m)
            {
                continue;
            }

            lineNo += 10000;
            var line = new FAJournalLine(GuidGenerator.Create(), batchId, lineNo, postingDate, documentNo, assetBook.FANo, FAJournalPostingType.Depreciation, result.Amount);
            line.Update(postingDate, documentNo, assetBook.FANo, FAJournalPostingType.Depreciation, result.Amount, $"Depreciation to {untilDate:yyyy-MM-dd}");
            line.SetBook(bookCode);
            line.SetDepreciationDays(result.Days);
            await _lineRepository.InsertAsync(line);
            added++;
        }

        return added;
    }

    /// <summary>What the calculator needs of an asset in a book.</summary>
    public static DepreciationInput InputOf(FADepreciationBook assetBook, DepreciationBook book, FABookValues values) =>
        new(
            assetBook.DepreciationMethod,
            assetBook.DepreciationStartingDate ?? assetBook.AcquisitionDate ?? DateTime.Today,
            assetBook.DepreciationEndingDate,
            assetBook.NoOfDepreciationYears,
            assetBook.StraightLinePct,
            assetBook.FixedDeprAmount,
            assetBook.DecliningBalancePct,
            values.DepreciableBasis,
            values.BookValue,
            assetBook.EndingBookValue != 0m ? assetBook.EndingBookValue : book.DefaultEndingBookValue,
            assetBook.FinalRoundingAmount != 0m ? assetBook.FinalRoundingAmount : book.DefaultFinalRoundingAmount,
            assetBook.UseHalfYearConvention,
            book.FiscalYear365Days,
            assetBook.LastDepreciationDate
        );
}
