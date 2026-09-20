using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Checks a general journal line before it is posted.
/// Mirrors Business Central codeunit 11 "Gen. Jnl.-Check Line".
/// <para>
/// Everything here is refused up front rather than part way through a posting run, so a batch
/// either posts whole or not at all.
/// </para>
/// </summary>
public class GenJnlCheckLine : DomainService
{
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Vendor, Guid> _vendorRepository;

    public GenJnlCheckLine(
        IRepository<GLAccount, Guid> glAccountRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Vendor, Guid> vendorRepository
    )
    {
        _glAccountRepository = glAccountRepository;
        _customerRepository = customerRepository;
        _vendorRepository = vendorRepository;
    }

    public async Task CheckAsync(GenJournalLine line)
    {
        Check.NotNull(line, nameof(line));

        if (line.PostingDate == default)
        {
            throw Failed(line, ErpErrorCodes.Journals.PostingDateRequired);
        }

        if (line.AccountNo.IsNullOrWhiteSpace())
        {
            throw Failed(line, ErpErrorCodes.Journals.AccountNoRequired);
        }

        // Posting an account against itself would write two entries that cancel out.
        if (
            line.BalAccountNo != null
            && line.BalAccountType == line.AccountType
            && string.Equals(line.BalAccountNo, line.AccountNo, StringComparison.OrdinalIgnoreCase)
        )
        {
            throw Failed(line, ErpErrorCodes.Journals.SameAccountAndBalAccount);
        }

        if (line.RecurringMethod != RecurringMethod.None && !DateFormula.TryParse(line.RecurringFrequency, out _))
        {
            throw Failed(line, ErpErrorCodes.Journals.InvalidDateFormula).WithData("formula", line.RecurringFrequency);
        }

        await CheckAccountAsync(line, line.AccountType, line.AccountNo);

        if (line.BalAccountNo != null)
        {
            await CheckAccountAsync(line, line.BalAccountType!.Value, line.BalAccountNo);
        }
    }

    private async Task CheckAccountAsync(GenJournalLine line, GenJournalAccountType accountType, string accountNo)
    {
        switch (accountType)
        {
            case GenJournalAccountType.GLAccount:
                var account = await _glAccountRepository.FirstOrDefaultAsync(a => a.No == accountNo);
                if (account == null)
                {
                    throw Failed(line, ErpErrorCodes.GLAccounts.GLAccountNotFound).WithData("accountNo", accountNo);
                }

                if (account.Blocked)
                {
                    throw Failed(line, ErpErrorCodes.GLAccounts.AccountBlocked).WithData("accountNo", accountNo);
                }

                // Totals and headings are calculated rows, never posted to.
                if (!account.DirectPosting || account.AccountType != GLAccountType.Posting)
                {
                    throw Failed(line, ErpErrorCodes.GLAccounts.DirectPostingNotAllowed).WithData("accountNo", accountNo);
                }

                break;

            case GenJournalAccountType.Customer:
                var customer = await _customerRepository.FirstOrDefaultAsync(c => c.No == accountNo);
                if (customer == null)
                {
                    throw Failed(line, ErpErrorCodes.Customers.CustomerNotFound).WithData("accountNo", accountNo);
                }

                if (customer.Blocked)
                {
                    throw Failed(line, ErpErrorCodes.Customers.CustomerBlocked).WithData("accountNo", accountNo);
                }

                break;

            case GenJournalAccountType.Vendor:
                var vendor = await _vendorRepository.FirstOrDefaultAsync(v => v.No == accountNo);
                if (vendor == null)
                {
                    throw Failed(line, ErpErrorCodes.Vendors.VendorNotFound).WithData("accountNo", accountNo);
                }

                if (vendor.Blocked)
                {
                    throw Failed(line, ErpErrorCodes.Vendors.VendorBlocked).WithData("accountNo", accountNo);
                }

                break;
        }
    }

    /// <summary>Every failure names the line, so the journal page can point at the right row.</summary>
    private static BusinessException Failed(GenJournalLine line, string errorCode)
    {
        return new BusinessException(errorCode)
            .WithData("lineNo", line.LineNo)
            .WithData("documentNo", line.DocumentNo);
    }
}
