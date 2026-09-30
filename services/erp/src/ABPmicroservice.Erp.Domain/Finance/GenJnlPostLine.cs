using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// What one posting run shares: its register, its transaction number and its source codes.
/// Mirrors the state Business Central keeps in codeunit 12 between lines.
/// </summary>
public sealed class GLPostingContext
{
    public GLPostingContext(GLRegister register, string sourceCode = null, string reasonCode = null)
    {
        Register = Check.NotNull(register, nameof(register));
        SourceCode = sourceCode;
        ReasonCode = reasonCode;
    }

    public GLRegister Register { get; }

    public string SourceCode { get; }

    public string ReasonCode { get; }

    public long TransactionNo => Register.TransactionNo;
}

/// <summary>
/// Core General Journal Posting Engine.
/// Mirrors Business Central Codeunit 12 "Gen. Jnl.-Post Line".
/// <para>
/// A customer, vendor, bank or employee line writes two rows, not one: the subledger entry and the
/// G/L entry on the control account taken from the posting group. Without the second, a posted
/// journal would not balance and the trial balance would be wrong.
/// </para>
/// <para>
/// Subledger entries keep the line's currency and its LCY amount; the G/L is always in LCY. A G/L
/// account side with a posting type and VAT groups splits into its base and a VAT entry. A party
/// line with an Applies-to Doc. No. settles that open entry, and any LCY left over because the two
/// were booked at different rates goes to the currency's realized gain or loss account.
/// </para>
/// </summary>
public class GenJnlPostLine : DomainService
{
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly IRepository<GLEntry, Guid> _glEntryRepository;
    private readonly IRepository<CustomerLedgerEntry, Guid> _custLedgerEntryRepository;
    private readonly IRepository<VendorLedgerEntry, Guid> _vendorLedgerEntryRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly IRepository<CustomerPostingGroup, Guid> _customerPostingGroupRepository;
    private readonly IRepository<VendorPostingGroup, Guid> _vendorPostingGroupRepository;
    private readonly IEntryNoGenerator _entryNoGenerator;

    private IRepository<BankAccount, Guid> BankAccountRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<BankAccount, Guid>>();
    private IRepository<BankAccountLedgerEntry, Guid> BankLedgerEntryRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<BankAccountLedgerEntry, Guid>>();
    private IRepository<BankAccountPostingGroup, Guid> BankPostingGroupRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<BankAccountPostingGroup, Guid>>();
    private IRepository<Employee, Guid> EmployeeRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<Employee, Guid>>();
    private IRepository<EmployeeLedgerEntry, Guid> EmployeeLedgerEntryRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<EmployeeLedgerEntry, Guid>>();
    private IRepository<EmployeePostingGroup, Guid> EmployeePostingGroupRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<EmployeePostingGroup, Guid>>();
    private IRepository<PaymentTerms, Guid> PaymentTermsRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<PaymentTerms, Guid>>();
    private IRepository<VatEntry, Guid> VatEntryRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<VatEntry, Guid>>();
    private PostingSetupManager PostingSetupManager => LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>();
    private CurrencyExchangeRateManager CurrencyManager => LazyServiceProvider.LazyGetRequiredService<CurrencyExchangeRateManager>();

    public GenJnlPostLine(
        IRepository<GLAccount, Guid> glAccountRepository,
        IRepository<GLEntry, Guid> glEntryRepository,
        IRepository<CustomerLedgerEntry, Guid> custLedgerEntryRepository,
        IRepository<VendorLedgerEntry, Guid> vendorLedgerEntryRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Vendor, Guid> vendorRepository,
        IRepository<CustomerPostingGroup, Guid> customerPostingGroupRepository,
        IRepository<VendorPostingGroup, Guid> vendorPostingGroupRepository,
        IEntryNoGenerator entryNoGenerator
    )
    {
        _glAccountRepository = glAccountRepository;
        _glEntryRepository = glEntryRepository;
        _custLedgerEntryRepository = custLedgerEntryRepository;
        _vendorLedgerEntryRepository = vendorLedgerEntryRepository;
        _customerRepository = customerRepository;
        _vendorRepository = vendorRepository;
        _customerPostingGroupRepository = customerPostingGroupRepository;
        _vendorPostingGroupRepository = vendorPostingGroupRepository;
        _entryNoGenerator = entryNoGenerator;
    }

    /// <summary>What every entry of one journal line shares.</summary>
    private sealed record LineContext(
        GenJournalLine Line,
        GLPostingContext Context,
        DateTime? DueDate,
        string BillToPayToNo
    )
    {
        public DateTime PostingDate => Line.PostingDate;
        public DateTime DocumentDate => Line.DocumentDate;
        public GLEntryDocumentType DocumentType => Line.DocumentType;
        public string DocumentNo => Line.DocumentNo;
        public string Description => Line.Description;
        public Guid DimensionSetId => Line.DimensionSetId;
        public string CurrencyCode => Line.CurrencyCode;
    }

    /// <summary>One side of a journal line: the account or the balancing account.</summary>
    private sealed record Side(
        GenJournalAccountType AccountType,
        string AccountNo,
        decimal Amount,
        decimal AmountLcy,
        GeneralPostingType GenPostingType,
        string VatBusPostingGroup,
        string VatProdPostingGroup,
        string AppliesToDocNo
    );

    /// <summary>
    /// Posts one journal line: its account, then its balancing account with the opposite sign.
    /// <paramref name="dueDate"/> is the due date of a customer or vendor entry; without it the
    /// party's payment terms decide.
    /// </summary>
    public async Task PostLineAsync(GenJournalLine line, GLPostingContext context, DateTime? dueDate = null)
    {
        Check.NotNull(line, nameof(line));
        Check.NotNull(context, nameof(context));

        if (line.Amount == 0m && line.AmountLcy == 0m)
        {
            return;
        }

        var accountIsParty = IsParty(line.AccountType);
        var lineContext = new LineContext(line, context, dueDate, PartyNo(line));

        await PostSideAsync(
            lineContext,
            new Side(
                line.AccountType,
                line.AccountNo,
                line.Amount,
                line.AmountLcy,
                line.GenPostingType,
                line.VatBusPostingGroup,
                line.VatProdPostingGroup,
                accountIsParty ? line.AppliesToDocNo : null
            )
        );

        if (line.BalAccountNo != null)
        {
            await PostSideAsync(
                lineContext,
                new Side(
                    line.BalAccountType!.Value,
                    line.BalAccountNo,
                    -line.Amount,
                    -line.AmountLcy,
                    line.BalGenPostingType,
                    line.BalVatBusPostingGroup,
                    line.BalVatProdPostingGroup,
                    accountIsParty ? null : line.AppliesToDocNo
                )
            );
        }
    }

    /// <summary>
    /// Posts the VAT of a document or journal line: the VAT entry and its G/L amount on the VAT
    /// account of the setup. A reverse charge purchase also credits the reverse charge account with
    /// the same amount, so the buyer's self-assessed VAT nets to zero. <paramref name="base"/> and
    /// <paramref name="amount"/> are in LCY and carry the G/L sign (negative for a sale).
    /// </summary>
    public async Task<VatEntry> PostVatAsync(
        VatPostingSetup setup,
        VatEntryType type,
        decimal @base,
        decimal amount,
        DateTime postingDate,
        DateTime documentDate,
        GLEntryDocumentType documentType,
        string documentNo,
        string billToPayToNo,
        string description,
        GLPostingContext context
    )
    {
        var entry = new VatEntry(GuidGenerator.Create(), postingDate, documentDate, documentType, documentNo, type, @base, amount, setup, billToPayToNo)
        {
            EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.VatEntry),
            TransactionNo = context?.TransactionNo ?? 0,
            RegisterNo = context?.Register.No ?? 0,
        };

        context?.Register.NoteVatEntry(entry.EntryNo);
        await VatEntryRepository.InsertAsync(entry);

        if (amount == 0m)
        {
            return entry;
        }

        await PostGLDirectAsync(
            PostingSetupManager.GetVatAccount(setup, type),
            postingDate,
            documentType,
            documentNo,
            description,
            amount,
            billToPayToNo,
            context: context,
            documentDate: documentDate
        );

        if (setup.VatCalculationType == VatCalculationType.ReverseChargeVat && type == VatEntryType.Purchase)
        {
            await PostGLDirectAsync(
                PostingSetupManager.GetVatAccount(setup, type, reverseChargeSide: true),
                postingDate,
                documentType,
                documentNo,
                description,
                -amount,
                billToPayToNo,
                context: context,
                documentDate: documentDate
            );
        }

        return entry;
    }

    /// <summary>
    /// Writes one G/L entry straight to an account, in LCY. Used by the document posting engines,
    /// which have already worked out the accounts and amounts themselves.
    /// </summary>
    public async Task<GLEntry> PostGLDirectAsync(
        string glAccountNo,
        DateTime postingDate,
        GLEntryDocumentType documentType,
        string documentNo,
        string description,
        decimal amount,
        string sourceNo = null,
        Guid dimensionSetId = default,
        GLPostingContext context = null,
        DateTime? documentDate = null
    )
    {
        var account = await _glAccountRepository.FirstOrDefaultAsync(a => a.No == glAccountNo);
        if (account == null)
        {
            throw new BusinessException(ErpErrorCodes.GLAccounts.GLAccountNotFound).WithData("accountNo", glAccountNo);
        }

        if (account.Blocked)
        {
            throw new BusinessException(ErpErrorCodes.GLAccounts.AccountBlocked).WithData("accountNo", glAccountNo);
        }

        var entry = new GLEntry(
            GuidGenerator.Create(),
            account.Id,
            account.No,
            postingDate,
            documentType,
            documentNo,
            description,
            amount,
            sourceNo,
            dimensionSetId: dimensionSetId,
            documentDate: documentDate
        )
        {
            EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.GLEntry),
        };

        Stamp(entry, context);
        context?.Register.NoteGLEntry(entry.EntryNo);

        account.ApplyEntry(amount);
        await _glAccountRepository.UpdateAsync(account);
        await _glEntryRepository.InsertAsync(entry);

        return entry;
    }

    /// <summary>
    /// Revalues a foreign currency bank account: a bank ledger entry with no amount in the currency
    /// and <paramref name="amountLcy"/> in LCY, and the same on the bank's G/L account. The
    /// counterpart (the unrealized gain or loss) is the caller's.
    /// </summary>
    internal async Task PostBankAdjustmentAsync(
        BankAccount bankAccount,
        decimal amountLcy,
        DateTime postingDate,
        string documentNo,
        string description,
        GLPostingContext context
    )
    {
        var glAccountNo = await GetBankGLAccountNoAsync(bankAccount);

        var entry = new BankAccountLedgerEntry(
            GuidGenerator.Create(),
            bankAccount.Id,
            bankAccount.No,
            postingDate,
            GLEntryDocumentType.None,
            documentNo,
            description,
            0m,
            currencyCode: bankAccount.CurrencyCode,
            amountLcy: amountLcy
        )
        {
            EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.BankAccountLedgerEntry),
            TransactionNo = context.TransactionNo,
            RegisterNo = context.Register.No,
        };

        context.Register.NoteBankEntry(entry.EntryNo);
        await BankLedgerEntryRepository.InsertAsync(entry);

        bankAccount.ApplyBalance(0m, amountLcy);
        await BankAccountRepository.UpdateAsync(bankAccount);

        await PostGLDirectAsync(glAccountNo, postingDate, GLEntryDocumentType.None, documentNo, description, amountLcy, bankAccount.No, context: context);
    }

    private async Task PostSideAsync(LineContext line, Side side)
    {
        switch (side.AccountType)
        {
            case GenJournalAccountType.GLAccount:
                await PostGLAccountSideAsync(line, side);
                break;

            case GenJournalAccountType.Customer:
                await PostCustomerEntryAsync(line, side);
                break;

            case GenJournalAccountType.Vendor:
                await PostVendorEntryAsync(line, side);
                break;

            case GenJournalAccountType.BankAccount:
                await PostBankEntryAsync(line, side);
                break;

            case GenJournalAccountType.Employee:
                await PostEmployeeEntryAsync(line, side);
                break;
        }
    }

    /// <summary>A G/L account side: its base on the account, and the VAT beside it when it carries any.</summary>
    private async Task PostGLAccountSideAsync(LineContext line, Side side)
    {
        var hasVat = side.GenPostingType != GeneralPostingType.None && side.VatProdPostingGroup != null;
        if (!hasVat)
        {
            await PostGL(line, side.AccountNo, side.AmountLcy, sourceNo: null);
            return;
        }

        var setup = await PostingSetupManager.GetVatPostingSetupAsync(side.VatBusPostingGroup, side.VatProdPostingGroup);
        var vat = JournalVat.Calculate(side.AmountLcy, setup, side.GenPostingType);

        // A Full VAT line has no base: all of it is VAT.
        if (vat.Base != 0m && setup.VatCalculationType != VatCalculationType.FullVat)
        {
            await PostGL(line, side.AccountNo, vat.Base, sourceNo: null);
        }

        await PostVatAsync(
            setup,
            side.GenPostingType == GeneralPostingType.Purchase ? VatEntryType.Purchase : VatEntryType.Sale,
            setup.VatCalculationType == VatCalculationType.FullVat ? 0m : vat.Base,
            vat.Vat,
            line.PostingDate,
            line.DocumentDate,
            line.DocumentType,
            line.DocumentNo,
            line.BillToPayToNo,
            line.Description,
            line.Context
        );
    }

    private async Task PostCustomerEntryAsync(LineContext line, Side side)
    {
        var customer = await _customerRepository.FirstOrDefaultAsync(c => c.No == side.AccountNo);
        if (customer == null)
        {
            throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", side.AccountNo);
        }

        var receivablesAccountNo = await GetReceivablesAccountNoAsync(customer);

        var entry = new CustomerLedgerEntry(
            GuidGenerator.Create(),
            customer.Id,
            customer.No,
            line.PostingDate,
            line.DocumentType.ToString(),
            line.DocumentNo,
            line.Description,
            side.Amount,
            line.DueDate ?? await DueDateAsync(customer.PaymentTermsCode, line.DocumentDate, line.PostingDate),
            line.DimensionSetId,
            line.CurrencyCode,
            side.AmountLcy
        )
        {
            EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.CustomerLedgerEntry),
            TransactionNo = line.Context.TransactionNo,
            RegisterNo = line.Context.Register.No,
        };

        var realized = 0m;
        if (side.AppliesToDocNo != null)
        {
            var open = await FindOpenEntryAsync(_custLedgerEntryRepository, e => e.CustomerId == customer.Id && e.Open, side.AppliesToDocNo);
            realized = EntryApplication.Apply(entry, open);
            await _custLedgerEntryRepository.UpdateAsync(open);
        }

        line.Context.Register.NoteCustomerEntry(entry.EntryNo);
        await _custLedgerEntryRepository.InsertAsync(entry);

        customer.ApplyBalance(side.AmountLcy - realized);
        await _customerRepository.UpdateAsync(customer);

        // The receivables control account must move with the subledger, or the books do not balance.
        await PostGL(line, receivablesAccountNo, side.AmountLcy, customer.No);
        await PostRealizedAsync(line, receivablesAccountNo, entry.CurrencyCode, realized, customer.No);
    }

    private async Task PostVendorEntryAsync(LineContext line, Side side)
    {
        var vendor = await _vendorRepository.FirstOrDefaultAsync(v => v.No == side.AccountNo);
        if (vendor == null)
        {
            throw new BusinessException(ErpErrorCodes.Vendors.VendorNotFound).WithData("accountNo", side.AccountNo);
        }

        var payablesAccountNo = await GetPayablesAccountNoAsync(vendor);

        var entry = new VendorLedgerEntry(
            GuidGenerator.Create(),
            vendor.Id,
            vendor.No,
            line.PostingDate,
            line.DocumentType.ToString(),
            line.DocumentNo,
            line.Description,
            side.Amount,
            line.DueDate ?? await DueDateAsync(vendor.PaymentTermsCode, line.DocumentDate, line.PostingDate),
            line.DimensionSetId,
            line.CurrencyCode,
            side.AmountLcy
        )
        {
            EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.VendorLedgerEntry),
            TransactionNo = line.Context.TransactionNo,
            RegisterNo = line.Context.Register.No,
        };

        var realized = 0m;
        if (side.AppliesToDocNo != null)
        {
            var open = await FindOpenEntryAsync(_vendorLedgerEntryRepository, e => e.VendorId == vendor.Id && e.Open, side.AppliesToDocNo);
            realized = EntryApplication.Apply(entry, open);
            await _vendorLedgerEntryRepository.UpdateAsync(open);
        }

        line.Context.Register.NoteVendorEntry(entry.EntryNo);
        await _vendorLedgerEntryRepository.InsertAsync(entry);

        vendor.ApplyBalance(side.AmountLcy - realized);
        await _vendorRepository.UpdateAsync(vendor);

        await PostGL(line, payablesAccountNo, side.AmountLcy, vendor.No);
        await PostRealizedAsync(line, payablesAccountNo, entry.CurrencyCode, realized, vendor.No);
    }

    private async Task PostBankEntryAsync(LineContext line, Side side)
    {
        var bankAccount = await BankAccountRepository.FirstOrDefaultAsync(b => b.No == side.AccountNo)
            ?? throw new BusinessException(ErpErrorCodes.CashManagement.BankAccountNotFound).WithData("accountNo", side.AccountNo);

        var glAccountNo = await GetBankGLAccountNoAsync(bankAccount);

        var entry = new BankAccountLedgerEntry(
            GuidGenerator.Create(),
            bankAccount.Id,
            bankAccount.No,
            line.PostingDate,
            line.DocumentType,
            line.DocumentNo,
            line.Description,
            side.Amount,
            line.DimensionSetId,
            line.CurrencyCode,
            side.AmountLcy
        )
        {
            EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.BankAccountLedgerEntry),
            TransactionNo = line.Context.TransactionNo,
            RegisterNo = line.Context.Register.No,
        };

        line.Context.Register.NoteBankEntry(entry.EntryNo);
        await BankLedgerEntryRepository.InsertAsync(entry);

        bankAccount.ApplyBalance(side.Amount, side.AmountLcy);
        await BankAccountRepository.UpdateAsync(bankAccount);

        // The bank's G/L account moves with its ledger, as the receivables account does with a customer's.
        await PostGL(line, glAccountNo, side.AmountLcy, bankAccount.No);
    }

    /// <summary>
    /// An employee side: an expense claim (credit) or a payout (debit) on the employee's ledger,
    /// and the same on the employee posting group's payables account. Mirrors BC's employee lines.
    /// </summary>
    private async Task PostEmployeeEntryAsync(LineContext line, Side side)
    {
        var employee = await EmployeeRepository.FirstOrDefaultAsync(e => e.No == side.AccountNo)
            ?? throw new BusinessException(ErpErrorCodes.HumanResources.EmployeeNotFound).WithData("no", side.AccountNo);

        var group = employee.EmployeePostingGroup == null
            ? null
            : await EmployeePostingGroupRepository.FirstOrDefaultAsync(g => g.Code == employee.EmployeePostingGroup);
        var payablesAccountNo = group?.PayablesAccountNo
            ?? throw new BusinessException(ErpErrorCodes.HumanResources.PostingGroupNotFound)
                .WithData("employeeNo", employee.No)
                .WithData("postingGroup", employee.EmployeePostingGroup ?? "");

        var entry = new EmployeeLedgerEntry(
            GuidGenerator.Create(),
            employee.Id,
            employee.No,
            line.PostingDate,
            line.DocumentDate,
            line.DocumentType,
            line.DocumentNo,
            line.Description,
            side.AmountLcy,
            line.DimensionSetId
        )
        {
            EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.EmployeeLedgerEntry),
            TransactionNo = line.Context.TransactionNo,
            RegisterNo = line.Context.Register.No,
        };

        if (side.AppliesToDocNo != null)
        {
            var repository = EmployeeLedgerEntryRepository;
            var open = await FindOpenEntryAsync(repository, e => e.EmployeeId == employee.Id && e.Open, side.AppliesToDocNo);
            // Employees are paid in LCY, so the application leaves nothing behind to realize.
            EntryApplication.Apply(entry, open);
            await repository.UpdateAsync(open);
        }

        line.Context.Register.NoteEmployeeEntry(entry.EntryNo);
        await EmployeeLedgerEntryRepository.InsertAsync(entry);

        employee.ApplyBalance(side.AmountLcy);
        await EmployeeRepository.UpdateAsync(employee);

        await PostGL(line, payablesAccountNo, side.AmountLcy, employee.No);
    }

    /// <summary>
    /// Clears the realized difference out of the control account and into the currency's realized
    /// gain or loss account. Positive <paramref name="realized"/> is a loss.
    /// </summary>
    private async Task PostRealizedAsync(LineContext line, string controlAccountNo, string currencyCode, decimal realized, string sourceNo)
    {
        if (realized == 0m || currencyCode == null)
        {
            return;
        }

        var currency = await CurrencyManager.GetCurrencyAsync(currencyCode);
        var gainLossAccountNo = currency.GetGainLossAccount(gain: realized < 0m, realized: true);

        await PostGL(line, controlAccountNo, -realized, sourceNo);
        await PostGL(line, gainLossAccountNo, realized, sourceNo);
    }

    private async Task<TEntry> FindOpenEntryAsync<TEntry>(
        IRepository<TEntry, Guid> repository,
        System.Linq.Expressions.Expression<Func<TEntry, bool>> openOfParty,
        string documentNo
    )
        where TEntry : LedgerEntryBase, IApplicableLedgerEntry
    {
        var candidates = await repository.GetListAsync(openOfParty);
        return candidates.Where(e => e.DocumentNo == documentNo).OrderBy(e => e.EntryNo).FirstOrDefault()
            ?? throw new BusinessException(ErpErrorCodes.Journals.AppliesToEntryNotFound).WithData("documentNo", documentNo);
    }

    private Task<GLEntry> PostGL(LineContext line, string accountNo, decimal amountLcy, string sourceNo)
    {
        return PostGLDirectAsync(
            accountNo,
            line.PostingDate,
            line.DocumentType,
            line.DocumentNo,
            line.Description,
            amountLcy,
            sourceNo,
            line.DimensionSetId,
            line.Context,
            line.DocumentDate
        );
    }

    private static bool IsParty(GenJournalAccountType type) =>
        type is GenJournalAccountType.Customer or GenJournalAccountType.Vendor or GenJournalAccountType.Employee;

    /// <summary>The customer, vendor or employee a line concerns, stamped on its VAT entries.</summary>
    private static string PartyNo(GenJournalLine line)
    {
        if (IsParty(line.AccountType))
        {
            return line.AccountNo;
        }

        return line.BalAccountType.HasValue && IsParty(line.BalAccountType.Value) ? line.BalAccountNo : null;
    }

    private async Task<string> GetBankGLAccountNoAsync(BankAccount bankAccount)
    {
        var group = bankAccount.BankAccPostingGroup.IsNullOrWhiteSpace()
            ? null
            : await BankPostingGroupRepository.FirstOrDefaultAsync(g => g.Code == bankAccount.BankAccPostingGroup);

        return group?.GLAccountNo
            ?? throw new BusinessException(ErpErrorCodes.CashManagement.PostingGroupNotFound)
                .WithData("accountNo", bankAccount.No)
                .WithData("postingGroup", bankAccount.BankAccPostingGroup ?? "");
    }

    /// <summary>The due date the party's payment terms give a document of this date (BC "Due Date Calculation").</summary>
    private async Task<DateTime> DueDateAsync(string paymentTermsCode, DateTime documentDate, DateTime postingDate)
    {
        var baseDate = documentDate == default ? postingDate : documentDate;
        var code = CodeTableEntity.NormalizeCode(paymentTermsCode);
        var terms = code == null ? null : await PaymentTermsRepository.FirstOrDefaultAsync(t => t.Code == code);

        return terms?.CalculateDueDate(baseDate) ?? postingDate.AddDays(ErpDomainConsts.DefaultPaymentDueDays);
    }

    internal async Task<string> GetReceivablesAccountNoAsync(Customer customer)
    {
        var group = customer.CustomerPostingGroup.IsNullOrWhiteSpace()
            ? null
            : await _customerPostingGroupRepository.FirstOrDefaultAsync(g => g.Code == customer.CustomerPostingGroup);

        return group?.ReceivablesAccountNo
            ?? throw new BusinessException(ErpErrorCodes.Customers.PostingGroupNotFound)
                .WithData("customerNo", customer.No)
                .WithData("postingGroup", customer.CustomerPostingGroup);
    }

    internal async Task<string> GetPayablesAccountNoAsync(Vendor vendor)
    {
        var group = vendor.VendorPostingGroup.IsNullOrWhiteSpace()
            ? null
            : await _vendorPostingGroupRepository.FirstOrDefaultAsync(g => g.Code == vendor.VendorPostingGroup);

        return group?.PayablesAccountNo
            ?? throw new BusinessException(ErpErrorCodes.Vendors.PostingGroupNotFound)
                .WithData("vendorNo", vendor.No)
                .WithData("postingGroup", vendor.VendorPostingGroup);
    }

    private static void Stamp(GLEntry entry, GLPostingContext context)
    {
        if (context == null)
        {
            return;
        }

        entry.TransactionNo = context.TransactionNo;
        entry.RegisterNo = context.Register.No;
        entry.SourceCode = context.SourceCode;
        entry.ReasonCode = context.ReasonCode;
    }
}
