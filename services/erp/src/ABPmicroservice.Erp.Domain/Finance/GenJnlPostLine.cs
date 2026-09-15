using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Purchasing;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Core General Journal Posting Engine.
/// Mirrors Business Central Codeunit 12 "Gen. Jnl.-Post Line".
/// Handles atomic posting of General Journal lines to G/L Entries, Customer Subledger, and Vendor Subledger.
/// </summary>
public class GenJnlPostLine : DomainService
{
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly IRepository<GLEntry, Guid> _glEntryRepository;
    private readonly IRepository<CustomerLedgerEntry, Guid> _custLedgerEntryRepository;
    private readonly IRepository<VendorLedgerEntry, Guid> _vendorLedgerEntryRepository;

    public GenJnlPostLine(
        IRepository<GLAccount, Guid> glAccountRepository,
        IRepository<GLEntry, Guid> glEntryRepository,
        IRepository<CustomerLedgerEntry, Guid> custLedgerEntryRepository,
        IRepository<VendorLedgerEntry, Guid> vendorLedgerEntryRepository
    )
    {
        _glAccountRepository = glAccountRepository;
        _glEntryRepository = glEntryRepository;
        _custLedgerEntryRepository = custLedgerEntryRepository;
        _vendorLedgerEntryRepository = vendorLedgerEntryRepository;
    }

    public async Task PostLineAsync(GenJournalLine line)
    {
        Check.NotNull(line, nameof(line));
        if (line.Amount == 0m)
        {
            return;
        }

        // Post main account entry
        await PostAccountEntryAsync(
            line.AccountType,
            line.AccountNo,
            line.PostingDate,
            line.DocumentType,
            line.DocumentNo,
            line.Description,
            line.Amount,
            line.DimensionSetId
        );

        // Post balancing account entry (with inverted sign) if specified
        if (!string.IsNullOrWhiteSpace(line.BalAccountNo))
        {
            await PostAccountEntryAsync(
                line.BalAccountType ?? "G/L Account",
                line.BalAccountNo,
                line.PostingDate,
                line.DocumentType,
                line.DocumentNo,
                line.Description,
                -line.Amount,
                line.DimensionSetId
            );
        }
    }

    public async Task PostGLDirectAsync(
        string glAccountNo,
        DateTime postingDate,
        GLEntryDocumentType documentType,
        string documentNo,
        string description,
        decimal amount,
        string sourceNo = null,
        Guid dimensionSetId = default
    )
    {
        var account = await _glAccountRepository.FirstOrDefaultAsync(a => a.No == glAccountNo);
        if (account == null)
        {
            throw new UserFriendlyException($"G/L Account '{glAccountNo}' does not exist.");
        }
        if (account.Blocked)
        {
            throw new UserFriendlyException($"G/L Account '{glAccountNo}' is blocked.");
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
            sourceNo
        );

        account.ApplyEntry(amount);
        await _glAccountRepository.UpdateAsync(account);
        await _glEntryRepository.InsertAsync(entry);
    }

    private async Task PostAccountEntryAsync(
        string accountType,
        string accountNo,
        DateTime postingDate,
        GLEntryDocumentType documentType,
        string documentNo,
        string description,
        decimal amount,
        Guid dimensionSetId
    )
    {
        if (accountType.Equals("G/L Account", StringComparison.OrdinalIgnoreCase))
        {
            await PostGLDirectAsync(accountNo, postingDate, documentType, documentNo, description, amount, null, dimensionSetId);
        }
        else if (accountType.Equals("Customer", StringComparison.OrdinalIgnoreCase))
        {
            var custEntry = new CustomerLedgerEntry(
                GuidGenerator.Create(),
                Guid.Empty,
                accountNo,
                postingDate,
                documentType.ToString(),
                documentNo,
                description,
                amount,
                postingDate.AddDays(30),
                dimensionSetId
            );
            await _custLedgerEntryRepository.InsertAsync(custEntry);
        }
        else if (accountType.Equals("Vendor", StringComparison.OrdinalIgnoreCase))
        {
            var vendorEntry = new VendorLedgerEntry(
                GuidGenerator.Create(),
                Guid.Empty,
                accountNo,
                postingDate,
                documentType.ToString(),
                documentNo,
                description,
                amount,
                postingDate.AddDays(30),
                dimensionSetId
            );
            await _vendorLedgerEntryRepository.InsertAsync(vendorEntry);
        }
    }
}
