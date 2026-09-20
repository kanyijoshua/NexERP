using System;
using System.Threading.Tasks;
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
/// A customer or vendor line writes two rows, not one: the subledger entry and the G/L entry on
/// the control account taken from the posting group. Without the second, a posted journal would
/// not balance and the trial balance would be wrong.
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

    /// <summary>
    /// Posts one journal line: its account, then its balancing account with the opposite sign.
    /// </summary>
    public async Task PostLineAsync(GenJournalLine line, GLPostingContext context)
    {
        Check.NotNull(line, nameof(line));
        Check.NotNull(context, nameof(context));

        if (line.Amount == 0m)
        {
            return;
        }

        await PostAccountEntryAsync(
            line.AccountType,
            line.AccountNo,
            line.PostingDate,
            line.DocumentDate,
            line.DocumentType,
            line.DocumentNo,
            line.Description,
            line.Amount,
            line.DimensionSetId,
            context
        );

        if (line.BalAccountNo != null)
        {
            await PostAccountEntryAsync(
                line.BalAccountType!.Value,
                line.BalAccountNo,
                line.PostingDate,
                line.DocumentDate,
                line.DocumentType,
                line.DocumentNo,
                line.Description,
                -line.Amount,
                line.DimensionSetId,
                context
            );
        }
    }

    /// <summary>
    /// Writes one G/L entry straight to an account. Used by the document posting engines, which
    /// have already worked out the accounts and amounts themselves.
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

    private async Task PostAccountEntryAsync(
        GenJournalAccountType accountType,
        string accountNo,
        DateTime postingDate,
        DateTime documentDate,
        GLEntryDocumentType documentType,
        string documentNo,
        string description,
        decimal amount,
        Guid dimensionSetId,
        GLPostingContext context
    )
    {
        switch (accountType)
        {
            case GenJournalAccountType.GLAccount:
                await PostGLDirectAsync(
                    accountNo,
                    postingDate,
                    documentType,
                    documentNo,
                    description,
                    amount,
                    sourceNo: null,
                    dimensionSetId: dimensionSetId,
                    context: context,
                    documentDate: documentDate
                );
                break;

            case GenJournalAccountType.Customer:
                await PostCustomerEntryAsync(
                    accountNo,
                    postingDate,
                    documentDate,
                    documentType,
                    documentNo,
                    description,
                    amount,
                    dimensionSetId,
                    context
                );
                break;

            case GenJournalAccountType.Vendor:
                await PostVendorEntryAsync(
                    accountNo,
                    postingDate,
                    documentDate,
                    documentType,
                    documentNo,
                    description,
                    amount,
                    dimensionSetId,
                    context
                );
                break;
        }
    }

    private async Task PostCustomerEntryAsync(
        string customerNo,
        DateTime postingDate,
        DateTime documentDate,
        GLEntryDocumentType documentType,
        string documentNo,
        string description,
        decimal amount,
        Guid dimensionSetId,
        GLPostingContext context
    )
    {
        var customer = await _customerRepository.FirstOrDefaultAsync(c => c.No == customerNo);
        if (customer == null)
        {
            throw new BusinessException(ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", customerNo);
        }

        var receivablesAccountNo = await GetReceivablesAccountNoAsync(customer);

        var entry = new CustomerLedgerEntry(
            GuidGenerator.Create(),
            customer.Id,
            customer.No,
            postingDate,
            documentType.ToString(),
            documentNo,
            description,
            amount,
            postingDate.AddDays(ErpDomainConsts.DefaultPaymentDueDays),
            dimensionSetId
        )
        {
            EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.CustomerLedgerEntry),
            TransactionNo = context?.TransactionNo ?? 0,
            RegisterNo = context?.Register.No ?? 0,
        };

        context?.Register.NoteCustomerEntry(entry.EntryNo);
        await _custLedgerEntryRepository.InsertAsync(entry);

        customer.ApplyBalance(amount);
        await _customerRepository.UpdateAsync(customer);

        // The receivables control account must move with the subledger, or the books do not balance.
        await PostGLDirectAsync(
            receivablesAccountNo,
            postingDate,
            documentType,
            documentNo,
            description,
            amount,
            sourceNo: customer.No,
            dimensionSetId: dimensionSetId,
            context: context,
            documentDate: documentDate
        );
    }

    private async Task PostVendorEntryAsync(
        string vendorNo,
        DateTime postingDate,
        DateTime documentDate,
        GLEntryDocumentType documentType,
        string documentNo,
        string description,
        decimal amount,
        Guid dimensionSetId,
        GLPostingContext context
    )
    {
        var vendor = await _vendorRepository.FirstOrDefaultAsync(v => v.No == vendorNo);
        if (vendor == null)
        {
            throw new BusinessException(ErpErrorCodes.Vendors.VendorNotFound).WithData("accountNo", vendorNo);
        }

        var payablesAccountNo = await GetPayablesAccountNoAsync(vendor);

        var entry = new VendorLedgerEntry(
            GuidGenerator.Create(),
            vendor.Id,
            vendor.No,
            postingDate,
            documentType.ToString(),
            documentNo,
            description,
            amount,
            postingDate.AddDays(ErpDomainConsts.DefaultPaymentDueDays),
            dimensionSetId
        )
        {
            EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.VendorLedgerEntry),
            TransactionNo = context?.TransactionNo ?? 0,
            RegisterNo = context?.Register.No ?? 0,
        };

        context?.Register.NoteVendorEntry(entry.EntryNo);
        await _vendorLedgerEntryRepository.InsertAsync(entry);

        vendor.ApplyBalance(amount);
        await _vendorRepository.UpdateAsync(vendor);

        await PostGLDirectAsync(
            payablesAccountNo,
            postingDate,
            documentType,
            documentNo,
            description,
            amount,
            sourceNo: vendor.No,
            dimensionSetId: dimensionSetId,
            context: context,
            documentDate: documentDate
        );
    }

    private async Task<string> GetReceivablesAccountNoAsync(Customer customer)
    {
        var group = customer.CustomerPostingGroup.IsNullOrWhiteSpace()
            ? null
            : await _customerPostingGroupRepository.FirstOrDefaultAsync(g => g.Code == customer.CustomerPostingGroup);

        return group?.ReceivablesAccountNo
            ?? throw new BusinessException(ErpErrorCodes.Customers.PostingGroupNotFound)
                .WithData("customerNo", customer.No)
                .WithData("postingGroup", customer.CustomerPostingGroup);
    }

    private async Task<string> GetPayablesAccountNoAsync(Vendor vendor)
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
