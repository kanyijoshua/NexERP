using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

public class VatSettlementRequest
{
    /// <summary>Blank settles everything still open up to <see cref="EndingDate"/>.</summary>
    public DateTime? StartingDate { get; set; }

    public DateTime EndingDate { get; set; }

    public DateTime PostingDate { get; set; }

    public string DocumentNo { get; set; }

    /// <summary>Where the net VAT goes: the liability to (or claim on) the tax authority.</summary>
    public string SettlementAccountNo { get; set; }
}

/// <summary>What one VAT posting setup and type settles.</summary>
public class VatSettlementLine
{
    public string VatBusPostingGroup { get; set; }
    public string VatProdPostingGroup { get; set; }
    public string VatIdentifier { get; set; }
    public VatCalculationType VatCalculationType { get; set; }
    public VatEntryType Type { get; set; }
    public decimal VatPercent { get; set; }
    public int EntryCount { get; set; }

    /// <summary>Base and VAT with the G/L sign: negative for sales.</summary>
    public decimal Base { get; set; }

    public decimal Amount { get; set; }
}

public class VatSettlementResult
{
    public List<VatSettlementLine> Lines { get; set; } = new();

    /// <summary>What the settlement account is credited with: positive when VAT is owed.</summary>
    public decimal NetVatPayable { get; set; }

    public bool Posted { get; set; }

    public long RegisterNo { get; set; }
}

/// <summary>
/// Calculates and posts the VAT settlement: the open VAT entries of the period are closed, each VAT account is cleared of
/// them, and the net lands on the settlement account as one amount owed to or reclaimable from
/// the tax authority.
/// </summary>
public class VatSettlementEngine : DomainService
{
    private readonly IRepository<VatEntry, Guid> _vatEntryRepository;
    private readonly IRepository<VatPostingSetup, Guid> _setupRepository;
    private readonly GenJnlPostLine _postLine;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;
    private readonly IEntryNoGenerator _entryNoGenerator;

    public VatSettlementEngine(
        IRepository<VatEntry, Guid> vatEntryRepository,
        IRepository<VatPostingSetup, Guid> setupRepository,
        GenJnlPostLine postLine,
        GLRegisterManager registerManager,
        GeneralLedgerSetupManager glSetupManager,
        IEntryNoGenerator entryNoGenerator
    )
    {
        _vatEntryRepository = vatEntryRepository;
        _setupRepository = setupRepository;
        _postLine = postLine;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
        _entryNoGenerator = entryNoGenerator;
    }

    /// <summary>What a settlement would close and post, without writing anything.</summary>
    public async Task<VatSettlementResult> CalculateAsync(VatSettlementRequest request)
    {
        var groups = await GetOpenGroupsAsync(request);
        return new VatSettlementResult
        {
            Lines = groups.Select(g => g.Line).ToList(),
            NetVatPayable = -groups.Sum(g => SettlementShare(g.Line)),
        };
    }

    public async Task<VatSettlementResult> PostAsync(VatSettlementRequest request)
    {
        if (request.SettlementAccountNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Journals.AccountNoRequired);
        }

        if (request.DocumentNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.NoSeries.NumberRequired);
        }

        await _glSetupManager.CheckPostingDateAsync(request.PostingDate);

        var groups = await GetOpenGroupsAsync(request);
        if (groups.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.NothingToSettle);
        }

        var documentNo = request.DocumentNo.Trim();
        var register = await _registerManager.OpenAsync(request.PostingDate, GLRegister.VatSettlementSourceCode, documentNo);
        var context = new GLPostingContext(register, GLRegister.VatSettlementSourceCode);
        var settlement = 0m;

        foreach (var group in groups)
        {
            var line = group.Line;
            var setup = await _setupRepository.FirstOrDefaultAsync(s =>
                s.VatBusPostingGroup == line.VatBusPostingGroup && s.VatProdPostingGroup == line.VatProdPostingGroup
            );

            if (line.Amount != 0m)
            {
                if (setup == null)
                {
                    throw new BusinessException(ErpErrorCodes.PostingSetup.VatPostingSetupMissing)
                        .WithData("vatBusPostingGroup", line.VatBusPostingGroup ?? "")
                        .WithData("vatProdPostingGroup", line.VatProdPostingGroup ?? "");
                }

                // Clear the VAT account of what the period put on it.
                await PostAsync(PostingSetupManager.GetVatAccount(setup, line.Type), -line.Amount, request, documentNo, context);

                if (line.VatCalculationType == VatCalculationType.ReverseChargeVat && line.Type == VatEntryType.Purchase)
                {
                    await PostAsync(PostingSetupManager.GetVatAccount(setup, line.Type, reverseChargeSide: true), line.Amount, request, documentNo, context);
                }

                settlement += SettlementShare(line);
            }

            var settlementEntry = new VatEntry(GuidGenerator.Create(), request.PostingDate, documentNo, group.Entries[0], -line.Base, -line.Amount)
            {
                EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.VatEntry),
                TransactionNo = context.TransactionNo,
                RegisterNo = register.No,
            };

            register.NoteVatEntry(settlementEntry.EntryNo);
            await _vatEntryRepository.InsertAsync(settlementEntry);

            // One by one: UpdateMany marks every column changed, which the append-only guard refuses.
            foreach (var entry in group.Entries)
            {
                entry.Closed = true;
                entry.ClosedByEntryNo = settlementEntry.EntryNo;
                await _vatEntryRepository.UpdateAsync(entry);
            }
        }

        if (settlement != 0m)
        {
            await PostAsync(request.SettlementAccountNo.Trim(), settlement, request, documentNo, context);
        }

        await _registerManager.CloseAsync(register);

        return new VatSettlementResult
        {
            Lines = groups.Select(g => g.Line).ToList(),
            NetVatPayable = -settlement,
            Posted = true,
            RegisterNo = register.No,
        };
    }

    /// <summary>
    /// What a group moves onto the settlement account: its VAT, except reverse charge purchases,
    /// whose input and self-assessed output VAT cancel and so leave nothing to pay.
    /// </summary>
    private static decimal SettlementShare(VatSettlementLine line) =>
        line.VatCalculationType == VatCalculationType.ReverseChargeVat && line.Type == VatEntryType.Purchase ? 0m : line.Amount;

    private Task<GLEntry> PostAsync(string accountNo, decimal amount, VatSettlementRequest request, string documentNo, GLPostingContext context)
    {
        return _postLine.PostGLDirectAsync(accountNo, request.PostingDate, GLEntryDocumentType.None, documentNo, "VAT settlement", amount, context: context);
    }

    private sealed record OpenGroup(VatSettlementLine Line, List<VatEntry> Entries);

    private async Task<List<OpenGroup>> GetOpenGroupsAsync(VatSettlementRequest request)
    {
        var start = request.StartingDate?.Date;
        var end = request.EndingDate.Date;
        if (start.HasValue && start.Value > end)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.InvalidDateRange);
        }

        var entries = await _vatEntryRepository.GetListAsync(e =>
            !e.Closed && e.Type != VatEntryType.Settlement && e.PostingDate <= end && (start == null || e.PostingDate >= start)
        );

        return entries
            .GroupBy(e => new { e.VatBusPostingGroup, e.VatProdPostingGroup, e.Type })
            .OrderBy(g => g.Key.VatBusPostingGroup)
            .ThenBy(g => g.Key.VatProdPostingGroup)
            .ThenBy(g => g.Key.Type)
            .Select(g =>
            {
                var list = g.OrderBy(e => e.EntryNo).ToList();
                return new OpenGroup(
                    new VatSettlementLine
                    {
                        VatBusPostingGroup = g.Key.VatBusPostingGroup,
                        VatProdPostingGroup = g.Key.VatProdPostingGroup,
                        Type = g.Key.Type,
                        VatIdentifier = list[0].VatIdentifier,
                        VatCalculationType = list[0].VatCalculationType,
                        VatPercent = list[0].VatPercent,
                        EntryCount = list.Count,
                        Base = list.Sum(e => e.Base),
                        Amount = list.Sum(e => e.Amount),
                    },
                    list
                );
            })
            .ToList();
    }
}

public class VatReturnRequest
{
    public DateTime StartingDate { get; set; }

    public DateTime EndingDate { get; set; }

    public VatEntrySelection Selection { get; set; } = VatEntrySelection.OpenAndClosed;
}

/// <summary>One rate of one side of the return, shown as positive amounts.</summary>
public class VatReturnLine
{
    public VatEntryType Type { get; set; }
    public string VatIdentifier { get; set; }
    public VatCalculationType VatCalculationType { get; set; }
    public decimal VatPercent { get; set; }
    public decimal Base { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// The VAT return boxes. Modelled on a standard VAT statement: output VAT (sales and the
/// self-assessed reverse charge), input VAT, the net due, and the sales and purchases before VAT.
/// </summary>
public class VatReturnResult
{
    public DateTime StartingDate { get; set; }
    public DateTime EndingDate { get; set; }
    public VatEntrySelection Selection { get; set; }
    public List<VatReturnLine> Lines { get; set; } = new();
    public decimal OutputVatOnSales { get; set; }
    public decimal OutputVatOnReverseCharge { get; set; }
    public decimal TotalOutputVat { get; set; }
    public decimal InputVat { get; set; }
    public decimal NetVatDue { get; set; }
    public decimal SalesExcludingVat { get; set; }
    public decimal PurchasesExcludingVat { get; set; }
}

/// <summary>
/// Calculates the VAT return from the VAT entries. A VAT Statement
/// with a fixed layout rather than user-defined statement lines.
/// </summary>
public class VatReturnEngine : DomainService
{
    private readonly IRepository<VatEntry, Guid> _vatEntryRepository;

    public VatReturnEngine(IRepository<VatEntry, Guid> vatEntryRepository)
    {
        _vatEntryRepository = vatEntryRepository;
    }

    public async Task<VatReturnResult> CalculateAsync(VatReturnRequest request)
    {
        var start = request.StartingDate.Date;
        var end = request.EndingDate.Date;
        if (start > end)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.InvalidDateRange);
        }

        var entries = (await _vatEntryRepository.GetListAsync(e =>
            e.Type != VatEntryType.Settlement && e.PostingDate >= start && e.PostingDate <= end
        ))
            .Where(e => request.Selection switch
            {
                VatEntrySelection.Open => !e.Closed,
                VatEntrySelection.Closed => e.Closed,
                _ => true,
            })
            .ToList();

        var sales = entries.Where(e => e.Type == VatEntryType.Sale).ToList();
        var purchases = entries.Where(e => e.Type == VatEntryType.Purchase).ToList();

        var result = new VatReturnResult
        {
            StartingDate = start,
            EndingDate = end,
            Selection = request.Selection,
            // Sales carry the G/L sign (negative); the return shows what is owed as positive.
            OutputVatOnSales = -sales.Sum(e => e.Amount),
            OutputVatOnReverseCharge = purchases.Where(e => e.VatCalculationType == VatCalculationType.ReverseChargeVat).Sum(e => e.Amount),
            InputVat = purchases.Sum(e => e.Amount),
            SalesExcludingVat = -sales.Sum(e => e.Base),
            PurchasesExcludingVat = purchases.Sum(e => e.Base),
            Lines = entries
                .GroupBy(e => new { e.Type, e.VatIdentifier, e.VatCalculationType, e.VatPercent })
                .OrderBy(g => g.Key.Type == VatEntryType.Sale ? 0 : 1)
                .ThenBy(g => g.Key.VatIdentifier)
                .ThenBy(g => g.Key.VatCalculationType)
                .Select(g =>
                {
                    var sign = g.Key.Type == VatEntryType.Sale ? -1m : 1m;
                    return new VatReturnLine
                    {
                        Type = g.Key.Type,
                        VatIdentifier = g.Key.VatIdentifier,
                        VatCalculationType = g.Key.VatCalculationType,
                        VatPercent = g.Key.VatPercent,
                        Base = sign * g.Sum(e => e.Base),
                        Amount = sign * g.Sum(e => e.Amount),
                    };
                })
                .ToList(),
        };

        result.TotalOutputVat = result.OutputVatOnSales + result.OutputVatOnReverseCharge;
        result.NetVatDue = result.TotalOutputVat - result.InputVat;
        return result;
    }
}
