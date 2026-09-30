using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Sales;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// The CRONUS-style setup tables: VAT, payment terms, currencies, accounting periods, locations,
/// salespeople, bank accounts, payment methods and human resources. Every table is seeded on its
/// own when it is empty, so a database seeded before a table existed still gets it.
/// </summary>
public partial class ErpDataSeederContributor
{
    private IRepository<T, Guid> Repo<T>()
        where T : class, IEntity<Guid> => _serviceProvider.GetRequiredService<IRepository<T, Guid>>();

    private async Task<bool> IsEmptyAsync<T>()
        where T : class, IEntity<Guid> => await Repo<T>().GetCountAsync() == 0;

    private async Task EnsureAccountAsync(string no, string name, GLAccountCategory category, IncomeBalanceType incomeBalance, bool directPosting = false)
    {
        if (!await _glAccountRepository.AnyAsync(a => a.No == no))
        {
            await _glAccountRepository.InsertAsync(new GLAccount(NewId(), no, name, GLAccountType.Posting, category, incomeBalance, directPosting: directPosting));
        }
    }

    private async Task EnsureSeriesAsync(string code, string description, string startingNo)
    {
        if (!await _noSeriesRepository.AnyAsync(s => s.Code == code))
        {
            await AddSeriesAsync(code, description, startingNo, manualNos: true);
        }
    }

    private async Task SeedTaxAndFinanceSetupAsync()
    {
        await EnsureAccountAsync("2310", "Sales VAT", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("2320", "Purchase VAT", GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("2330", "Reverse Charge VAT", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);

        if (await IsEmptyAsync<VatBusinessPostingGroup>())
        {
            await Repo<VatBusinessPostingGroup>().InsertAsync(new VatBusinessPostingGroup(NewId(), "DOMESTIC", "Domestic customers and vendors"));
            await Repo<VatBusinessPostingGroup>().InsertAsync(new VatBusinessPostingGroup(NewId(), "EXPORT", "Customers and vendors abroad"));
        }

        if (await IsEmptyAsync<VatProductPostingGroup>())
        {
            await Repo<VatProductPostingGroup>().InsertAsync(new VatProductPostingGroup(NewId(), "STANDARD", "Standard rate"));
            await Repo<VatProductPostingGroup>().InsertAsync(new VatProductPostingGroup(NewId(), "ZERO", "Zero rated"));
        }

        if (await IsEmptyAsync<VatPostingSetup>())
        {
            await AddVatSetupAsync("DOMESTIC", "STANDARD", VatCalculationType.NormalVat, 16m, "VAT16", "Standard rate");
            await AddVatSetupAsync("DOMESTIC", "ZERO", VatCalculationType.NormalVat, 0m, "VAT0", "Zero rated");
            await AddVatSetupAsync("EXPORT", "STANDARD", VatCalculationType.ReverseChargeVat, 16m, "VAT16", "Reverse charge");
            await AddVatSetupAsync("EXPORT", "ZERO", VatCalculationType.NormalVat, 0m, "VAT0", "Zero rated");
        }

        if (await IsEmptyAsync<PaymentTerms>())
        {
            await Repo<PaymentTerms>().InsertAsync(new PaymentTerms(NewId(), "COD", "Cash on delivery", "0D"));
            await Repo<PaymentTerms>().InsertAsync(new PaymentTerms(NewId(), "14D", "Net 14 days", "14D"));
            await Repo<PaymentTerms>().InsertAsync(new PaymentTerms(NewId(), "30D", "Net 30 days", "30D"));
            await Repo<PaymentTerms>().InsertAsync(new PaymentTerms(NewId(), "1M(8D)", "1 month, 2% within 8 days", "1M", "8D", 2m));
            await Repo<PaymentTerms>().InsertAsync(new PaymentTerms(NewId(), "CM", "Current month", "CM"));
        }

        await EnsureAccountAsync("2340", "VAT Settlement", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("4910", "Exchange Rate Gains", GLAccountCategory.Income, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("8910", "Exchange Rate Losses", GLAccountCategory.Expense, IncomeBalanceType.IncomeStatement);

        if (!await _glAccountRepository.AnyAsync(a => a.No == "6100"))
        {
            // An expense account employees claim against; journal lines on it default to purchase VAT.
            var expenses = new GLAccount(NewId(), "6100", "Travel and Expenses", GLAccountType.Posting, GLAccountCategory.Expense, IncomeBalanceType.IncomeStatement);
            expenses.SetVatProdPostingGroup("STANDARD");
            expenses.SetJournalVatDefaults(GeneralPostingType.Purchase, "DOMESTIC");
            await _glAccountRepository.InsertAsync(expenses);
        }

        if (await IsEmptyAsync<Currency>())
        {
            await Repo<Currency>().InsertAsync(WithExchangeAccounts(new Currency(NewId(), "USD", "US dollar", "$")));
            await Repo<Currency>().InsertAsync(WithExchangeAccounts(new Currency(NewId(), "EUR", "Euro", "€")));
        }
        else
        {
            // A database seeded before exchange differences were posted gets the accounts too.
            var currencies = await Repo<Currency>().GetListAsync();
            foreach (var currency in currencies.Where(c => c.RealizedGainsAccountNo == null && c.UnrealizedGainsAccountNo == null))
            {
                await Repo<Currency>().UpdateAsync(WithExchangeAccounts(currency));
            }
        }

        if (await IsEmptyAsync<CurrencyExchangeRate>())
        {
            var start = new DateTime(DateTime.Today.Year, 1, 1);
            await Repo<CurrencyExchangeRate>().InsertAsync(new CurrencyExchangeRate(NewId(), "USD", start, 1m, 0.79m));
            await Repo<CurrencyExchangeRate>().InsertAsync(new CurrencyExchangeRate(NewId(), "EUR", start, 1m, 0.85m));
        }

        if (await IsEmptyAsync<AccountingPeriod>())
        {
            var manager = _serviceProvider.GetRequiredService<AccountingPeriodManager>();
            await manager.CreateFiscalYearAsync(new DateTime(DateTime.Today.Year, 1, 1), 12, "1M");
        }
    }

    // Realized and unrealized differences share one gains and one losses account, as in CRONUS.
    private static Currency WithExchangeAccounts(Currency currency)
    {
        currency.SetGainLossAccounts("4910", "8910");
        currency.SetUnrealizedAccounts("4910", "8910");
        return currency;
    }

    private async Task AddVatSetupAsync(string bus, string prod, VatCalculationType type, decimal percent, string identifier, string description)
    {
        var setup = new VatPostingSetup(NewId(), bus, prod);
        setup.SetRate(type, percent, identifier, description);
        setup.SetAccounts("2310", "2320", type == VatCalculationType.ReverseChargeVat ? "2330" : null);
        await Repo<VatPostingSetup>().InsertAsync(setup);
    }

    private async Task SeedInventoryAndSalesSetupAsync()
    {
        await EnsureSeriesAsync("ITEM", "Items", "I00100");

        if (await IsEmptyAsync<Location>())
        {
            var main = new Location(NewId(), "MAIN", "Main warehouse");
            main.SetAddress("1 Main Street", "London", null, "GB");
            await Repo<Location>().InsertAsync(main);
        }

        if (await IsEmptyAsync<InventorySetup>())
        {
            var setup = new InventorySetup(NewId());
            setup.SetNumbering("ITEM");
            await Repo<InventorySetup>().InsertAsync(setup);
        }

        if (await IsEmptyAsync<SalespersonPurchaser>())
        {
            var person = new SalespersonPurchaser(NewId(), "PS", "Peter Saddow");
            person.SetContact("peter@cronus.example", null, "Sales manager");
            await Repo<SalespersonPurchaser>().InsertAsync(person);
        }
    }

    private async Task SeedCashManagementAsync()
    {
        await EnsureSeriesAsync("BANK", "Bank accounts", "B010");

        if (await IsEmptyAsync<BankAccountPostingGroup>())
        {
            await Repo<BankAccountPostingGroup>().InsertAsync(new BankAccountPostingGroup(NewId(), "CHECKING", "1020", "Checking accounts"));
        }

        if (await IsEmptyAsync<BankAccount>())
        {
            var bank = new BankAccount(NewId(), "WWB-OPERATING", "World Wide Bank - operating");
            bank.SetBankDetails("99-99-888", "BR01", null, null);
            bank.SetPosting("CHECKING", null);
            await Repo<BankAccount>().InsertAsync(bank);
        }

        if (await IsEmptyAsync<PaymentMethod>())
        {
            var cash = new PaymentMethod(NewId(), "CASH", "Cash payment");
            cash.SetBalancingAccount(GenJournalAccountType.GLAccount, "1010");
            await Repo<PaymentMethod>().InsertAsync(cash);

            var bank = new PaymentMethod(NewId(), "BANK", "Bank transfer");
            bank.SetBalancingAccount(GenJournalAccountType.BankAccount, "WWB-OPERATING");
            await Repo<PaymentMethod>().InsertAsync(bank);
        }
    }

    private async Task SeedHumanResourcesAsync()
    {
        await EnsureSeriesAsync("EMP", "Employees", "E0010");
        await EnsureAccountAsync("2140", "Employee Payables", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);

        if (await IsEmptyAsync<HumanResourceUnitOfMeasure>())
        {
            await Repo<HumanResourceUnitOfMeasure>().InsertAsync(new HumanResourceUnitOfMeasure(NewId(), "DAY", "Day", 1m));
            await Repo<HumanResourceUnitOfMeasure>().InsertAsync(new HumanResourceUnitOfMeasure(NewId(), "HOUR", "Hour", 0.125m));
        }

        if (await IsEmptyAsync<HumanResourcesSetup>())
        {
            var setup = new HumanResourcesSetup(NewId());
            setup.Set("EMP", "DAY");
            await Repo<HumanResourcesSetup>().InsertAsync(setup);
        }

        if (await IsEmptyAsync<EmployeePostingGroup>())
        {
            await Repo<EmployeePostingGroup>().InsertAsync(new EmployeePostingGroup(NewId(), "EMPLOYEES", "2140", "Employee expenses"));
        }

        if (await IsEmptyAsync<CauseOfAbsence>())
        {
            await Repo<CauseOfAbsence>().InsertAsync(new CauseOfAbsence(NewId(), "HOLIDAY", "Annual leave", "DAY"));
            await Repo<CauseOfAbsence>().InsertAsync(new CauseOfAbsence(NewId(), "SICK", "Sick leave", "DAY"));
            await Repo<CauseOfAbsence>().InsertAsync(new CauseOfAbsence(NewId(), "TRAINING", "Training", "DAY"));
        }

        if (await IsEmptyAsync<EmploymentContract>())
        {
            await Repo<EmploymentContract>().InsertAsync(new EmploymentContract(NewId(), "PERMANENT", "Permanent"));
            await Repo<EmploymentContract>().InsertAsync(new EmploymentContract(NewId(), "FIXEDTERM", "Fixed term"));
        }

        if (await IsEmptyAsync<GroundsForTermination>())
        {
            await Repo<GroundsForTermination>().InsertAsync(new GroundsForTermination(NewId(), "RESIGNED", "Resigned"));
            await Repo<GroundsForTermination>().InsertAsync(new GroundsForTermination(NewId(), "RETIRED", "Retired"));
            await Repo<GroundsForTermination>().InsertAsync(new GroundsForTermination(NewId(), "DISMISSED", "Dismissed"));
        }

        if (await IsEmptyAsync<Qualification>())
        {
            await Repo<Qualification>().InsertAsync(new Qualification(NewId(), "DEGREE", "Bachelor's degree"));
            await Repo<Qualification>().InsertAsync(new Qualification(NewId(), "CPA", "Certified Public Accountant"));
        }
    }
}
