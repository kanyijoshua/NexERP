using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.HumanResources;
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
    private readonly GeneralLedgerSetupManager _glSetupManager;

    public GenJnlCheckLine(
        IRepository<GLAccount, Guid> glAccountRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Vendor, Guid> vendorRepository,
        GeneralLedgerSetupManager glSetupManager
    )
    {
        _glSetupManager = glSetupManager;
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

        await _glSetupManager.CheckPostingDateAsync(line.PostingDate);

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
        await CheckVatAsync(line, line.AccountType, line.HasVat, line.VatBusPostingGroup, line.VatProdPostingGroup);

        if (line.BalAccountNo != null)
        {
            await CheckAccountAsync(line, line.BalAccountType!.Value, line.BalAccountNo);
            await CheckVatAsync(line, line.BalAccountType!.Value, line.HasBalVat, line.BalVatBusPostingGroup, line.BalVatProdPostingGroup);
        }
    }

    /// <summary>VAT belongs on G/L account lines only, and its posting setup must exist (BC checks the same).</summary>
    private async Task CheckVatAsync(GenJournalLine line, GenJournalAccountType accountType, bool hasVat, string bus, string prod)
    {
        if (!hasVat)
        {
            return;
        }

        if (accountType != GenJournalAccountType.GLAccount)
        {
            throw Failed(line, ErpErrorCodes.Journals.VatOnlyOnGLAccounts);
        }

        try
        {
            await LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>().GetVatPostingSetupAsync(bus, prod);
        }
        catch (BusinessException exception)
        {
            throw Failed(line, exception.Code)
                .WithData("vatBusPostingGroup", bus ?? "")
                .WithData("vatProdPostingGroup", prod ?? "");
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

            case GenJournalAccountType.BankAccount:
                var bankAccounts = LazyServiceProvider.LazyGetRequiredService<IRepository<BankAccount, Guid>>();
                var bankAccount = await bankAccounts.FirstOrDefaultAsync(b => b.No == accountNo);
                if (bankAccount == null)
                {
                    throw Failed(line, ErpErrorCodes.CashManagement.BankAccountNotFound).WithData("accountNo", accountNo);
                }

                if (bankAccount.Blocked)
                {
                    throw Failed(line, ErpErrorCodes.CashManagement.BankAccountBlocked).WithData("accountNo", accountNo);
                }

                // A bank account moves in its own currency only: the line must be in it.
                if (!string.Equals(bankAccount.CurrencyCode, line.CurrencyCode, StringComparison.OrdinalIgnoreCase))
                {
                    throw Failed(line, ErpErrorCodes.Journals.CurrencyMismatch)
                        .WithData("accountNo", accountNo)
                        .WithData("currencyCode", bankAccount.CurrencyCode ?? "LCY");
                }

                break;

            case GenJournalAccountType.Employee:
                var employees = LazyServiceProvider.LazyGetRequiredService<IRepository<Employee, Guid>>();
                var employee = await employees.FirstOrDefaultAsync(e => e.No == accountNo);
                if (employee == null)
                {
                    throw Failed(line, ErpErrorCodes.HumanResources.EmployeeNotFound).WithData("no", accountNo);
                }

                if (employee.Blocked)
                {
                    throw Failed(line, ErpErrorCodes.HumanResources.EmployeeBlocked).WithData("no", accountNo);
                }

                if (employee.EmployeePostingGroup == null)
                {
                    throw Failed(line, ErpErrorCodes.HumanResources.PostingGroupNotFound)
                        .WithData("employeeNo", accountNo)
                        .WithData("postingGroup", "");
                }

                // Employees are paid back, not invoiced: BC allows only blank, payment and refund.
                if (line.DocumentType is not (GLEntryDocumentType.None or GLEntryDocumentType.Payment or GLEntryDocumentType.Refund))
                {
                    throw Failed(line, ErpErrorCodes.Journals.EmployeeDocumentTypeNotAllowed).WithData("no", accountNo);
                }

                if (line.CurrencyCode != null)
                {
                    throw Failed(line, ErpErrorCodes.Journals.EmployeeInLocalCurrencyOnly).WithData("no", accountNo);
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
