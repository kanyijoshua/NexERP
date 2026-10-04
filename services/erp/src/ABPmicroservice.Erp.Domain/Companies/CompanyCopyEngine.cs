using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Companies;

/// <summary>
/// Company Copying Engine.
///
/// Duplicates the setup tables (chart of accounts, posting groups, posting setups and the
/// General Ledger Setup) into a newly created company.
/// </summary>
public class CompanyCopyEngine : DomainService
{
    private readonly IRepository<Company, Guid> _companyRepository;
    private readonly ICurrentCompany _currentCompany;

    public CompanyCopyEngine(IRepository<Company, Guid> companyRepository, ICurrentCompany currentCompany)
    {
        _companyRepository = companyRepository;
        _currentCompany = currentCompany;
    }

    public async Task<Company> CopyCompanyAsync(Guid sourceCompanyId, string newCompanyName, string newDisplayName)
    {
        var sourceCompany = await _companyRepository.GetAsync(sourceCompanyId);
        if (await _companyRepository.AnyAsync(c => c.Name == newCompanyName))
        {
            throw new BusinessException(ErpErrorCodes.Companies.CompanyNameAlreadyExists).WithData("Name", newCompanyName);
        }

        var targetCompany = new Company(GuidGenerator.Create(), newCompanyName, newDisplayName, CurrentTenant.Id);
        await _companyRepository.InsertAsync(targetCompany, autoSave: true);

        await CopyAsync<GLAccount>(sourceCompany, targetCompany, acc => new GLAccount(
            GuidGenerator.Create(),
            acc.No,
            acc.Name,
            acc.AccountType,
            acc.AccountCategory,
            acc.IncomeBalance,
            acc.Subcategory,
            acc.DirectPosting
        ));

        await CopyAsync<GenBusinessPostingGroup>(sourceCompany, targetCompany, g => new GenBusinessPostingGroup(GuidGenerator.Create(), g.Code, g.Description));
        await CopyAsync<GenProductPostingGroup>(sourceCompany, targetCompany, g => new GenProductPostingGroup(GuidGenerator.Create(), g.Code, g.Description));
        await CopyAsync<CustomerPostingGroup>(sourceCompany, targetCompany, g => new CustomerPostingGroup(GuidGenerator.Create(), g.Code, g.ReceivablesAccountNo, g.Description));
        await CopyAsync<VendorPostingGroup>(sourceCompany, targetCompany, g => new VendorPostingGroup(GuidGenerator.Create(), g.Code, g.PayablesAccountNo, g.Description));
        await CopyAsync<InventoryPostingGroup>(sourceCompany, targetCompany, g => new InventoryPostingGroup(GuidGenerator.Create(), g.Code, g.Description));
        await CopyAsync<InventoryPostingSetup>(sourceCompany, targetCompany, s => new InventoryPostingSetup(GuidGenerator.Create(), s.InventoryPostingGroup, s.InventoryAccountNo));

        await CopyAsync<GeneralPostingSetup>(sourceCompany, targetCompany, ps =>
        {
            var copy = new GeneralPostingSetup(GuidGenerator.Create(), ps.GenBusPostingGroup, ps.GenProdPostingGroup);
            copy.SetSalesAccounts(ps.SalesAccountNo, ps.SalesCreditMemoAccountNo, ps.SalesDiscountAccountNo);
            copy.SetPurchaseAccounts(ps.PurchAccountNo, ps.PurchCreditMemoAccountNo, ps.PurchDiscountAccountNo);
            copy.SetInventoryAccounts(ps.COGSAccountNo, ps.InventoryAdjmtAccountNo);
            return copy;
        });

        // Tax
        await CopyAsync<VatBusinessPostingGroup>(sourceCompany, targetCompany, g => new VatBusinessPostingGroup(GuidGenerator.Create(), g.Code, g.Description));
        await CopyAsync<VatProductPostingGroup>(sourceCompany, targetCompany, g => new VatProductPostingGroup(GuidGenerator.Create(), g.Code, g.Description));
        await CopyAsync<VatPostingSetup>(sourceCompany, targetCompany, v =>
        {
            var copy = new VatPostingSetup(GuidGenerator.Create(), v.VatBusPostingGroup, v.VatProdPostingGroup);
            copy.SetRate(v.VatCalculationType, v.VatPercent, v.VatIdentifier, v.Description);
            copy.SetAccounts(v.SalesVatAccountNo, v.PurchaseVatAccountNo, v.ReverseChrgVatAccountNo);
            copy.SetBlocked(v.Blocked);
            return copy;
        });

        // Finance and cash management code tables
        await CopyAsync<PaymentTerms>(sourceCompany, targetCompany, t => new PaymentTerms(GuidGenerator.Create(), t.Code, t.Description, t.DueDateCalculation, t.DiscountDateCalculation, t.DiscountPercent));
        await CopyAsync<Currency>(sourceCompany, targetCompany, c =>
        {
            var copy = new Currency(GuidGenerator.Create(), c.Code, c.Description, c.Symbol);
            copy.SetAmountRoundingPrecision(c.AmountRoundingPrecision);
            copy.SetGainLossAccounts(c.RealizedGainsAccountNo, c.RealizedLossesAccountNo);
            return copy;
        });
        await CopyAsync<BankAccountPostingGroup>(sourceCompany, targetCompany, g => new BankAccountPostingGroup(GuidGenerator.Create(), g.Code, g.GLAccountNo, g.Description));
        await CopyAsync<Location>(sourceCompany, targetCompany, l =>
        {
            var copy = new Location(GuidGenerator.Create(), l.Code, l.Description);
            copy.SetAddress(l.Address, l.City, l.PostCode, l.CountryRegionCode);
            copy.SetContact(l.Contact, l.PhoneNo);
            return copy;
        });

        // Human resources code tables
        await CopyAsync<HumanResourceUnitOfMeasure>(sourceCompany, targetCompany, u => new HumanResourceUnitOfMeasure(GuidGenerator.Create(), u.Code, u.Description, u.QtyPerUnitOfMeasure));
        await CopyAsync<EmployeePostingGroup>(sourceCompany, targetCompany, g => new EmployeePostingGroup(GuidGenerator.Create(), g.Code, g.PayablesAccountNo, g.Description));
        await CopyAsync<CauseOfAbsence>(sourceCompany, targetCompany, c => new CauseOfAbsence(GuidGenerator.Create(), c.Code, c.Description, c.UnitOfMeasureCode));
        await CopyAsync<Qualification>(sourceCompany, targetCompany, q => new Qualification(GuidGenerator.Create(), q.Code, q.Description));
        await CopyAsync<Union>(sourceCompany, targetCompany, u => new Union(GuidGenerator.Create(), u.Code, u.Description));
        await CopyAsync<EmploymentContract>(sourceCompany, targetCompany, c => new EmploymentContract(GuidGenerator.Create(), c.Code, c.Description));
        await CopyAsync<GroundsForTermination>(sourceCompany, targetCompany, g => new GroundsForTermination(GuidGenerator.Create(), g.Code, g.Description));

        // Posting date limits belong to the company being closed, not to a new one.
        await CopyAsync<GeneralLedgerSetup>(sourceCompany, targetCompany, gl =>
        {
            var copy = new GeneralLedgerSetup(GuidGenerator.Create());
            copy.SetLocalCurrency(gl.LcyCode);
            copy.SetRoundingPrecisions(gl.AmountRoundingPrecision, gl.UnitAmountRoundingPrecision, gl.InvRoundingPrecisionLcy);
            return copy;
        });

        return targetCompany;
    }

    /// <summary>
    /// Reads the rows inside the source company and writes the copies inside the new one, so they
    /// are stamped with its id. autoSave flushes while the target company is still ambient.
    /// </summary>
    private async Task CopyAsync<TEntity>(Company source, Company target, Func<TEntity, TEntity> clone)
        where TEntity : class, IEntity<Guid>
    {
        var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<TEntity, Guid>>();

        List<TEntity> rows;
        using (_currentCompany.Change(source.Id, source.Name))
        {
            rows = await repository.GetListAsync();
        }

        if (rows.Count == 0)
        {
            return;
        }

        using (_currentCompany.Change(target.Id, target.Name))
        {
            await repository.InsertManyAsync(rows.ConvertAll(row => clone(row)), autoSave: true);
        }
    }
}
