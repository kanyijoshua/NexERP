using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>What the G/L entries an application writes share: date, document and register.</summary>
public sealed record ApplicationPostingInfo(
    DateTime PostingDate,
    GLEntryDocumentType DocumentType,
    string DocumentNo,
    string Description,
    Guid DimensionSetId,
    GLPostingContext Context
);

/// <summary>What an application moved out of the control account besides the applied amounts.</summary>
public sealed record ApplicationOutcome(decimal RealizedLcy, decimal DiscountLcy)
{
    /// <summary>The LCY that left the party's balance with the application.</summary>
    public decimal BalanceChangeLcy => -(RealizedLcy + DiscountLcy);
}

/// <summary>One entry to apply, and how much of it (null: all that is open).</summary>
public sealed record EntryToApply(Guid EntryId, decimal? AmountToApply);

/// <summary>
/// Applies customer and vendor entries to each other and undoes applications. Every movement of an
/// entry's open amount is written as a detailed entry, so the sum of an entry's detailed entries is
/// always what is open on it, and an application can be undone exactly.
/// <para>
/// A payment made on or before an invoice's payment discount date settles the invoice in full when
/// it covers the invoice less the discount; the discount goes to the posting group's payment
/// discount account. An application between entries booked at different rates realizes the
/// difference on the currency's gain or loss account.
/// </para>
/// </summary>
public class EntryApplicationManager : DomainService
{
    private GenJnlPostLine GenJnlPostLine => LazyServiceProvider.LazyGetRequiredService<GenJnlPostLine>();
    private GLRegisterManager RegisterManager => LazyServiceProvider.LazyGetRequiredService<GLRegisterManager>();
    private IEntryNoGenerator EntryNoGenerator => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();
    private CurrencyExchangeRateManager CurrencyManager => LazyServiceProvider.LazyGetRequiredService<CurrencyExchangeRateManager>();
    private GeneralLedgerSetupManager GLSetupManager => LazyServiceProvider.LazyGetRequiredService<GeneralLedgerSetupManager>();

    private T Repo<T>() where T : class => LazyServiceProvider.LazyGetRequiredService<T>();

    /// <summary>
    /// Writes the initial detailed entry of a newly posted customer or vendor entry: what every later
    /// application starts from.
    /// </summary>
    public Task WriteInitialEntryAsync(PartyLedgerType type, IPartyLedgerEntry entry, GLEntryDocumentType documentType, GLPostingContext context)
    {
        return InsertDetailAsync(
            type,
            entry,
            DetailedCVLedgerEntryType.InitialEntry,
            entry.Amount,
            entry.AmountLcy,
            entry.PostingDate,
            documentType,
            entry.DocumentNo,
            appliedEntryNo: 0,
            applicationNo: 0,
            context
        );
    }

    /// <summary>
    /// Applies a new entry (a payment posted from a journal or document) to an open one while posting.
    /// The caller has not inserted <paramref name="newEntry"/> yet and moves the party balance itself.
    /// </summary>
    public async Task<ApplicationOutcome> ApplyOnPostingAsync(
        PartyLedgerType type,
        IPartyLedgerEntry newEntry,
        IPartyLedgerEntry openEntry,
        ApplicationPostingInfo info
    )
    {
        var adapter = Adapter(type);
        var outcome = await ApplyPairAsync(adapter, newEntry, openEntry, null, info, info.Context.TransactionNo);
        await adapter.UpdateAsync(openEntry);
        return outcome;
    }

    /// <summary>
    /// Applies a posted entry to other posted entries of the same party, in the order given, and posts
    /// what that realizes. Returns the application number, which <see cref="UnapplyAsync"/> undoes.
    /// </summary>
    public async Task<long> ApplyPostedEntriesAsync(
        PartyLedgerType type,
        Guid applyingEntryId,
        IReadOnlyList<EntryToApply> entries,
        DateTime postingDate,
        string documentNo = null
    )
    {
        Check.NotNull(entries, nameof(entries));
        var adapter = Adapter(type);
        var applying = await adapter.GetAsync(applyingEntryId);
        if (entries.Count == 0 || entries.Any(e => e.EntryId == applyingEntryId))
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.NothingToApply).WithData("entryNo", applying.EntryNo);
        }

        await GLSetupManager.CheckPostingDateAsync(postingDate);
        var date = postingDate.Date < applying.PostingDate ? applying.PostingDate : postingDate.Date;

        var register = await RegisterManager.OpenAsync(date, GLRegister.ApplicationSourceCode, documentNo ?? applying.DocumentNo);
        var context = new GLPostingContext(register, GLRegister.ApplicationSourceCode);
        var info = new ApplicationPostingInfo(
            date,
            GLEntryDocumentType.None,
            documentNo ?? applying.DocumentNo,
            null,
            applying.DimensionSetId,
            context
        );

        var balanceChange = 0m;
        foreach (var target in entries)
        {
            if (!applying.Open)
            {
                break;
            }

            var entry = await adapter.GetAsync(target.EntryId);
            if (entry.PartyId != applying.PartyId)
            {
                throw new BusinessException(ErpErrorCodes.FinanceOperations.EntriesNotSameParty)
                    .WithData("entryNo", entry.EntryNo)
                    .WithData("partyNo", applying.PartyNo);
            }

            var outcome = await ApplyPairAsync(adapter, applying, entry, target.AmountToApply, info, context.TransactionNo);
            balanceChange += outcome.BalanceChangeLcy;
            await adapter.UpdateAsync(entry);
        }

        await adapter.UpdateAsync(applying);
        await adapter.ApplyBalanceAsync(applying.PartyId, balanceChange);
        await RegisterManager.CloseAsync(register);

        return context.TransactionNo;
    }

    /// <summary>
    /// Undoes the last application <paramref name="entryId"/> took part in: every entry of it gets its
    /// open amount back, and the gains, losses and discounts it posted are reversed on
    /// <paramref name="postingDate"/>.
    /// </summary>
    public async Task<int> UnapplyAsync(PartyLedgerType type, Guid entryId, DateTime postingDate)
    {
        var adapter = Adapter(type);
        var entry = await adapter.GetAsync(entryId);

        var details = await adapter.GetDetailsAsync(d => !d.Unapplied && d.EntryType == DetailedCVLedgerEntryType.Application, entry.EntryNo);
        if (details.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.NothingToUnapply).WithData("entryNo", entry.EntryNo);
        }

        var applicationNo = details.Max(d => d.ApplicationNo);
        var application = await adapter.GetApplicationAsync(applicationNo);
        var appliedOn = application.Max(d => d.PostingDate);
        if (postingDate.Date < appliedOn)
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.UnapplyDateTooEarly)
                .WithData("date", appliedOn.ToString("yyyy-MM-dd"));
        }

        await GLSetupManager.CheckPostingDateAsync(postingDate);
        var register = await RegisterManager.OpenAsync(postingDate.Date, GLRegister.UnapplicationSourceCode, entry.DocumentNo);
        var context = new GLPostingContext(register, GLRegister.UnapplicationSourceCode);
        var entries = new Dictionary<long, IPartyLedgerEntry>();
        var balanceChange = 0m;

        foreach (var detail in application.OrderByDescending(d => d.EntryNo))
        {
            if (!entries.TryGetValue(detail.LedgerEntryNo, out var ledgerEntry))
            {
                ledgerEntry = await adapter.GetByEntryNoAsync(detail.LedgerEntryNo);
                entries[detail.LedgerEntryNo] = ledgerEntry;
            }

            var info = new ApplicationPostingInfo(postingDate.Date, GLEntryDocumentType.None, detail.DocumentNo, null, ledgerEntry.DimensionSetId, context);
            switch (detail.EntryType)
            {
                case DetailedCVLedgerEntryType.Application:
                    ledgerEntry.RestoreRemaining(-detail.Amount, -detail.AmountLcy);
                    break;

                case DetailedCVLedgerEntryType.PaymentDiscount:
                    ledgerEntry.RestoreRemaining(-detail.Amount, -detail.AmountLcy);
                    ledgerEntry.RestorePmtDiscount(-detail.Amount);
                    // The discount comes off the discount account and back onto the control account.
                    await PostDiscountAsync(adapter, ledgerEntry, -detail.AmountLcy, info);
                    balanceChange -= detail.AmountLcy;
                    break;

                case DetailedCVLedgerEntryType.RealizedGain:
                case DetailedCVLedgerEntryType.RealizedLoss:
                    await PostRealizedAsync(adapter, ledgerEntry, -detail.AmountLcy, info, gain: detail.EntryType == DetailedCVLedgerEntryType.RealizedGain);
                    balanceChange += detail.AmountLcy;
                    break;
            }

            var mirror = await InsertDetailAsync(
                type,
                ledgerEntry,
                detail.EntryType,
                -detail.Amount,
                -detail.AmountLcy,
                postingDate.Date,
                GLEntryDocumentType.None,
                detail.DocumentNo,
                detail.AppliedEntryNo,
                detail.ApplicationNo,
                context,
                unapplied: true
            );
            await adapter.MarkUnappliedAsync(detail.Id, mirror);
        }

        foreach (var ledgerEntry in entries.Values)
        {
            await adapter.UpdateAsync(ledgerEntry);
        }

        await adapter.ApplyBalanceAsync(entry.PartyId, balanceChange);
        await RegisterManager.CloseAsync(register);
        return application.Count;
    }

    /// <summary>
    /// Applies <paramref name="applying"/> to <paramref name="entry"/>: as much as both have open, or
    /// <paramref name="amountToApply"/> of the entry. Takes the payment discount when the entry is
    /// settled in full on or before its discount date.
    /// </summary>
    private async Task<ApplicationOutcome> ApplyPairAsync(
        PartyAdapter adapter,
        IPartyLedgerEntry applying,
        IPartyLedgerEntry entry,
        decimal? amountToApply,
        ApplicationPostingInfo info,
        long applicationNo
    )
    {
        if (
            !entry.Open
            || !applying.Open
            || !string.Equals(applying.CurrencyCode, entry.CurrencyCode, StringComparison.OrdinalIgnoreCase)
            || Math.Sign(applying.RemainingAmount) == Math.Sign(entry.RemainingAmount)
        )
        {
            throw new BusinessException(ErpErrorCodes.Journals.AppliesToEntryMismatch).WithData("documentNo", entry.DocumentNo);
        }

        var sign = Math.Sign(entry.RemainingAmount);
        var wanted = amountToApply.HasValue ? Math.Abs(amountToApply.Value) : Math.Abs(entry.RemainingAmount);
        if (wanted == 0m)
        {
            return new ApplicationOutcome(0m, 0m);
        }
        if (wanted > Math.Abs(entry.RemainingAmount))
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.AmountToApplyTooLarge)
                .WithData("entryNo", entry.EntryNo)
                .WithData("remaining", entry.RemainingAmount);
        }

        var available = Math.Abs(applying.RemainingAmount);
        var discount = 0m;
        if (
            entry.RemainingPmtDiscPossible != 0m
            && entry.PmtDiscountDate.HasValue
            && info.PostingDate <= entry.PmtDiscountDate.Value
            && wanted == Math.Abs(entry.RemainingAmount)
            && available + Math.Abs(entry.RemainingPmtDiscPossible) >= wanted
        )
        {
            discount = entry.RemainingPmtDiscPossible;
        }

        var applied = sign * Math.Min(wanted - Math.Abs(discount), available);
        var outcome = new ApplicationOutcome(0m, 0m);

        // The discount first: it is measured on the entry before the payment takes its share.
        var discountLcy = 0m;
        if (discount != 0m)
        {
            discountLcy = EntryApplication.Portion(entry, discount);
            entry.ReduceRemaining(discount, discountLcy, applying.EntryNo);
            entry.TakePmtDiscount();
            await InsertDetailAsync(adapter.Type, entry, DetailedCVLedgerEntryType.PaymentDiscount, -discount, -discountLcy, info.PostingDate, info.DocumentType, info.DocumentNo, applying.EntryNo, applicationNo, info.Context);
            await PostDiscountAsync(adapter, entry, -discountLcy, info);
        }

        var realized = 0m;
        if (applied != 0m)
        {
            var entryLcy = EntryApplication.Portion(entry, applied);
            var applyingLcy = EntryApplication.Portion(applying, -applied);
            entry.ReduceRemaining(applied, entryLcy, applying.EntryNo);
            applying.ReduceRemaining(-applied, applyingLcy, entry.EntryNo);
            realized = entryLcy + applyingLcy;

            await InsertDetailAsync(adapter.Type, entry, DetailedCVLedgerEntryType.Application, -applied, -entryLcy, info.PostingDate, info.DocumentType, info.DocumentNo, applying.EntryNo, applicationNo, info.Context);
            await InsertDetailAsync(adapter.Type, applying, DetailedCVLedgerEntryType.Application, applied, -applyingLcy, info.PostingDate, info.DocumentType, info.DocumentNo, entry.EntryNo, applicationNo, info.Context);

            if (realized != 0m)
            {
                await InsertDetailAsync(
                    adapter.Type,
                    entry,
                    realized > 0m ? DetailedCVLedgerEntryType.RealizedLoss : DetailedCVLedgerEntryType.RealizedGain,
                    0m,
                    realized,
                    info.PostingDate,
                    info.DocumentType,
                    info.DocumentNo,
                    applying.EntryNo,
                    applicationNo,
                    info.Context
                );
                await PostRealizedAsync(adapter, entry, realized, info);
            }
        }
        else if (discount == 0m)
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.NothingToApply).WithData("entryNo", applying.EntryNo);
        }

        // A payment that closed an entry with a discount still to be had loses the chance to take it.
        if (!entry.Open && entry.RemainingPmtDiscPossible != 0m)
        {
            entry.TakePmtDiscount();
        }

        return outcome with { RealizedLcy = realized, DiscountLcy = discountLcy };
    }

    /// <summary>
    /// Moves a payment discount between the control account and the discount account. A negative
    /// <paramref name="controlAmountLcy"/> takes it off the control account (the discount is granted
    /// or received); a positive one puts it back.
    /// </summary>
    private async Task PostDiscountAsync(PartyAdapter adapter, IPartyLedgerEntry entry, decimal controlAmountLcy, ApplicationPostingInfo info)
    {
        if (controlAmountLcy == 0m)
        {
            return;
        }

        var accounts = await adapter.GetAccountsAsync(entry.PartyId);
        // A discount granted to a customer is a cost, one received from a vendor an income.
        var discountAccount = adapter.Type == PartyLedgerType.Customer ? accounts.PmtDiscDebitAccountNo : accounts.PmtDiscCreditAccountNo;
        if (discountAccount.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.FinanceOperations.PaymentDiscountAccountMissing)
                .WithData("postingGroup", accounts.PostingGroup ?? "");
        }

        await PostGLAsync(accounts.ControlAccountNo, controlAmountLcy, entry.PartyNo, info);
        await PostGLAsync(discountAccount, -controlAmountLcy, entry.PartyNo, info);
    }

    /// <summary>Clears a realized difference out of the control account and onto the currency's gain or loss account.</summary>
    private async Task PostRealizedAsync(PartyAdapter adapter, IPartyLedgerEntry entry, decimal realized, ApplicationPostingInfo info, bool? gain = null)
    {
        if (realized == 0m || entry.CurrencyCode == null)
        {
            return;
        }

        var accounts = await adapter.GetAccountsAsync(entry.PartyId);
        var currency = await CurrencyManager.GetCurrencyAsync(entry.CurrencyCode);
        var gainLossAccountNo = currency.GetGainLossAccount(gain: gain ?? realized < 0m, realized: true);

        await PostGLAsync(accounts.ControlAccountNo, -realized, entry.PartyNo, info);
        await PostGLAsync(gainLossAccountNo, realized, entry.PartyNo, info);
    }

    private Task<GLEntry> PostGLAsync(string accountNo, decimal amountLcy, string sourceNo, ApplicationPostingInfo info)
    {
        return GenJnlPostLine.PostGLDirectAsync(
            accountNo,
            info.PostingDate,
            info.DocumentType,
            info.DocumentNo,
            info.Description,
            amountLcy,
            sourceNo,
            info.DimensionSetId,
            info.Context
        );
    }

    private async Task<long> InsertDetailAsync(
        PartyLedgerType type,
        IPartyLedgerEntry entry,
        DetailedCVLedgerEntryType entryType,
        decimal amount,
        decimal amountLcy,
        DateTime postingDate,
        GLEntryDocumentType documentType,
        string documentNo,
        long appliedEntryNo,
        long applicationNo,
        GLPostingContext context,
        bool unapplied = false
    )
    {
        var adapter = Adapter(type);
        var entryNo = await EntryNoGenerator.NextAsync(adapter.DetailSequence);
        var detail = new DetailRow(
            Guid.Empty,
            entryNo,
            entry.EntryNo,
            entryType,
            postingDate.Date,
            documentType,
            documentNo,
            amount,
            amountLcy,
            appliedEntryNo,
            applicationNo,
            unapplied
        );
        await adapter.InsertDetailAsync(detail, entry, context, GuidGenerator.Create());
        return entryNo;
    }

    private PartyAdapter Adapter(PartyLedgerType type) =>
        type == PartyLedgerType.Customer
            ? new CustomerAdapter(Repo<IRepository<CustomerLedgerEntry, Guid>>(), Repo<IRepository<DetailedCustLedgEntry, Guid>>(), Repo<IRepository<Customer, Guid>>(), Repo<IRepository<CustomerPostingGroup, Guid>>(), LazyServiceProvider.LazyGetRequiredService<Volo.Abp.Users.ICurrentUser>().UserName)
            : new VendorAdapter(Repo<IRepository<VendorLedgerEntry, Guid>>(), Repo<IRepository<DetailedVendorLedgEntry, Guid>>(), Repo<IRepository<Vendor, Guid>>(), Repo<IRepository<VendorPostingGroup, Guid>>(), LazyServiceProvider.LazyGetRequiredService<Volo.Abp.Users.ICurrentUser>().UserName);

    // --- the two subledgers behind one shape ----------------------------------------------------

    /// <summary>A detailed entry of either subledger, as the application logic reads it.</summary>
    internal sealed record DetailRow(
        Guid Id,
        long EntryNo,
        long LedgerEntryNo,
        DetailedCVLedgerEntryType EntryType,
        DateTime PostingDate,
        GLEntryDocumentType DocumentType,
        string DocumentNo,
        decimal Amount,
        decimal AmountLcy,
        long AppliedEntryNo,
        long ApplicationNo,
        bool Unapplied
    );

    internal sealed record PartyAccounts(string PostingGroup, string ControlAccountNo, string PmtDiscDebitAccountNo, string PmtDiscCreditAccountNo);

    internal abstract class PartyAdapter
    {
        public abstract PartyLedgerType Type { get; }

        public abstract string DetailSequence { get; }

        public abstract Task<IPartyLedgerEntry> GetAsync(Guid id);

        public abstract Task<IPartyLedgerEntry> GetByEntryNoAsync(long entryNo);

        public abstract Task UpdateAsync(IPartyLedgerEntry entry);

        public abstract Task ApplyBalanceAsync(Guid partyId, decimal amountLcy);

        public abstract Task<PartyAccounts> GetAccountsAsync(Guid partyId);

        public abstract Task InsertDetailAsync(DetailRow row, IPartyLedgerEntry entry, GLPostingContext context, Guid id);

        /// <summary>The details of one ledger entry that pass <paramref name="predicate"/>.</summary>
        public abstract Task<List<DetailRow>> GetDetailsAsync(Func<DetailRow, bool> predicate, long ledgerEntryNo);

        /// <summary>Every detail of an application that has not been undone yet.</summary>
        public abstract Task<List<DetailRow>> GetApplicationAsync(long applicationNo);

        public abstract Task MarkUnappliedAsync(Guid detailId, long unappliedByEntryNo);
    }

    private static NotSupportedException NotPartyEntry() => new("Not a ledger entry of this subledger.");

    internal sealed class CustomerAdapter(
        IRepository<CustomerLedgerEntry, Guid> entries,
        IRepository<DetailedCustLedgEntry, Guid> details,
        IRepository<Customer, Guid> customers,
        IRepository<CustomerPostingGroup, Guid> groups,
        string userName
    ) : PartyAdapter
    {
        public override PartyLedgerType Type => PartyLedgerType.Customer;

        public override string DetailSequence => ErpSequenceNames.DetailedCustLedgEntry;

        public override async Task<IPartyLedgerEntry> GetAsync(Guid id) => await entries.GetAsync(id);

        public override async Task<IPartyLedgerEntry> GetByEntryNoAsync(long entryNo) => await entries.FirstAsync(e => e.EntryNo == entryNo);

        public override Task UpdateAsync(IPartyLedgerEntry entry) => entries.UpdateAsync(entry as CustomerLedgerEntry ?? throw NotPartyEntry());

        public override async Task ApplyBalanceAsync(Guid partyId, decimal amountLcy)
        {
            if (amountLcy == 0m)
            {
                return;
            }

            var customer = await customers.GetAsync(partyId);
            customer.ApplyBalance(amountLcy);
            await customers.UpdateAsync(customer);
        }

        public override async Task<PartyAccounts> GetAccountsAsync(Guid partyId)
        {
            var customer = await customers.GetAsync(partyId);
            var group = customer.CustomerPostingGroup.IsNullOrWhiteSpace() ? null : await groups.FirstOrDefaultAsync(g => g.Code == customer.CustomerPostingGroup);
            if (group == null)
            {
                throw new BusinessException(ErpErrorCodes.Customers.PostingGroupNotFound)
                    .WithData("customerNo", customer.No)
                    .WithData("postingGroup", customer.CustomerPostingGroup ?? "");
            }

            return new PartyAccounts(group.Code, group.ReceivablesAccountNo, group.PaymentDiscDebitAccNo, group.PaymentDiscCreditAccNo);
        }

        public override async Task InsertDetailAsync(DetailRow row, IPartyLedgerEntry entry, GLPostingContext context, Guid id)
        {
            var detail = new DetailedCustLedgEntry(id)
            {
                EntryNo = row.EntryNo,
                CustLedgerEntryNo = row.LedgerEntryNo,
                EntryType = row.EntryType,
                PostingDate = row.PostingDate,
                DocumentType = row.DocumentType,
                DocumentNo = row.DocumentNo,
                Amount = row.Amount,
                AmountLcy = row.AmountLcy,
                CustomerNo = entry.PartyNo,
                CurrencyCode = entry.CurrencyCode,
                UserId = userName,
                SourceCode = context?.SourceCode,
                TransactionNo = context?.TransactionNo ?? 0,
                DebitAmount = row.Amount > 0m ? row.Amount : 0m,
                CreditAmount = row.Amount < 0m ? -row.Amount : 0m,
                DebitAmountLcy = row.AmountLcy > 0m ? row.AmountLcy : 0m,
                CreditAmountLcy = row.AmountLcy < 0m ? -row.AmountLcy : 0m,
                InitialEntryDueDate = entry.DueDate,
                AppliedCustLedgerEntryNo = row.AppliedEntryNo,
                Unapplied = row.Unapplied,
                ApplicationNo = (int)row.ApplicationNo,
                LedgerEntryAmount = row.EntryType == DetailedCVLedgerEntryType.InitialEntry,
            };
            await details.InsertAsync(detail);
        }

        public override async Task<List<DetailRow>> GetDetailsAsync(Func<DetailRow, bool> predicate, long ledgerEntryNo)
        {
            var rows = await details.GetListAsync(d => d.CustLedgerEntryNo == ledgerEntryNo);
            return rows.Select(Row).Where(predicate).ToList();
        }

        public override async Task<List<DetailRow>> GetApplicationAsync(long applicationNo)
        {
            var no = (int)applicationNo;
            var rows = await details.GetListAsync(d => d.ApplicationNo == no && !d.Unapplied && d.EntryType != DetailedCVLedgerEntryType.InitialEntry);
            return rows.Select(Row).ToList();
        }

        public override async Task MarkUnappliedAsync(Guid detailId, long unappliedByEntryNo)
        {
            var detail = await details.GetAsync(detailId);
            detail.Unapplied = true;
            detail.UnappliedByEntryNo = unappliedByEntryNo;
            await details.UpdateAsync(detail);
        }

        private static DetailRow Row(DetailedCustLedgEntry d) =>
            new(d.Id, d.EntryNo, d.CustLedgerEntryNo, d.EntryType, d.PostingDate ?? default, d.DocumentType, d.DocumentNo, d.Amount, d.AmountLcy, d.AppliedCustLedgerEntryNo, d.ApplicationNo, d.Unapplied);
    }

    internal sealed class VendorAdapter(
        IRepository<VendorLedgerEntry, Guid> entries,
        IRepository<DetailedVendorLedgEntry, Guid> details,
        IRepository<Vendor, Guid> vendors,
        IRepository<VendorPostingGroup, Guid> groups,
        string userName
    ) : PartyAdapter
    {
        public override PartyLedgerType Type => PartyLedgerType.Vendor;

        public override string DetailSequence => ErpSequenceNames.DetailedVendorLedgEntry;

        public override async Task<IPartyLedgerEntry> GetAsync(Guid id) => await entries.GetAsync(id);

        public override async Task<IPartyLedgerEntry> GetByEntryNoAsync(long entryNo) => await entries.FirstAsync(e => e.EntryNo == entryNo);

        public override Task UpdateAsync(IPartyLedgerEntry entry) => entries.UpdateAsync(entry as VendorLedgerEntry ?? throw NotPartyEntry());

        public override async Task ApplyBalanceAsync(Guid partyId, decimal amountLcy)
        {
            if (amountLcy == 0m)
            {
                return;
            }

            var vendor = await vendors.GetAsync(partyId);
            vendor.ApplyBalance(amountLcy);
            await vendors.UpdateAsync(vendor);
        }

        public override async Task<PartyAccounts> GetAccountsAsync(Guid partyId)
        {
            var vendor = await vendors.GetAsync(partyId);
            var group = vendor.VendorPostingGroup.IsNullOrWhiteSpace() ? null : await groups.FirstOrDefaultAsync(g => g.Code == vendor.VendorPostingGroup);
            if (group == null)
            {
                throw new BusinessException(ErpErrorCodes.Vendors.PostingGroupNotFound)
                    .WithData("vendorNo", vendor.No)
                    .WithData("postingGroup", vendor.VendorPostingGroup ?? "");
            }

            return new PartyAccounts(group.Code, group.PayablesAccountNo, group.PaymentDiscDebitAccNo, group.PaymentDiscCreditAccNo);
        }

        public override async Task InsertDetailAsync(DetailRow row, IPartyLedgerEntry entry, GLPostingContext context, Guid id)
        {
            var detail = new DetailedVendorLedgEntry(id)
            {
                EntryNo = row.EntryNo,
                VendorLedgerEntryNo = row.LedgerEntryNo,
                EntryType = row.EntryType,
                PostingDate = row.PostingDate,
                DocumentType = row.DocumentType,
                DocumentNo = row.DocumentNo,
                Amount = row.Amount,
                AmountLcy = row.AmountLcy,
                VendorNo = entry.PartyNo,
                CurrencyCode = entry.CurrencyCode,
                UserId = userName,
                SourceCode = context?.SourceCode,
                TransactionNo = context?.TransactionNo ?? 0,
                DebitAmount = row.Amount > 0m ? row.Amount : 0m,
                CreditAmount = row.Amount < 0m ? -row.Amount : 0m,
                DebitAmountLcy = row.AmountLcy > 0m ? row.AmountLcy : 0m,
                CreditAmountLcy = row.AmountLcy < 0m ? -row.AmountLcy : 0m,
                InitialEntryDueDate = entry.DueDate,
                AppliedVendLedgerEntryNo = row.AppliedEntryNo,
                Unapplied = row.Unapplied,
                ApplicationNo = (int)row.ApplicationNo,
                LedgerEntryAmount = row.EntryType == DetailedCVLedgerEntryType.InitialEntry,
            };
            await details.InsertAsync(detail);
        }

        public override async Task<List<DetailRow>> GetDetailsAsync(Func<DetailRow, bool> predicate, long ledgerEntryNo)
        {
            var rows = await details.GetListAsync(d => d.VendorLedgerEntryNo == ledgerEntryNo);
            return rows.Select(Row).Where(predicate).ToList();
        }

        public override async Task<List<DetailRow>> GetApplicationAsync(long applicationNo)
        {
            var no = (int)applicationNo;
            var rows = await details.GetListAsync(d => d.ApplicationNo == no && !d.Unapplied && d.EntryType != DetailedCVLedgerEntryType.InitialEntry);
            return rows.Select(Row).ToList();
        }

        public override async Task MarkUnappliedAsync(Guid detailId, long unappliedByEntryNo)
        {
            var detail = await details.GetAsync(detailId);
            detail.Unapplied = true;
            detail.UnappliedByEntryNo = unappliedByEntryNo;
            await details.UpdateAsync(detail);
        }

        private static DetailRow Row(DetailedVendorLedgEntry d) =>
            new(d.Id, d.EntryNo, d.VendorLedgerEntryNo, d.EntryType, d.PostingDate ?? default, d.DocumentType, d.DocumentNo, d.Amount, d.AmountLcy, d.AppliedVendLedgerEntryNo, d.ApplicationNo, d.Unapplied);
    }
}
