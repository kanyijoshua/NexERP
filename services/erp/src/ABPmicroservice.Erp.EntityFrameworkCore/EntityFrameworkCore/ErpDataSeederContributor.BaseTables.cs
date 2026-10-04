using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.FixedAssets;
using ABPmicroservice.Erp.HumanResources;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// The CRONUS-style rows of the base tables: countries, source codes, the fixed asset setup and
/// the relatives an employee can name. Each table is seeded on its own when it is empty.
/// </summary>
public partial class ErpDataSeederContributor
{
    private async Task SeedBaseTablesAsync()
    {
        if (await IsEmptyAsync<CountryRegion>())
        {
            await Repo<CountryRegion>().InsertAsync(new CountryRegion(NewId(), "KE", "Kenya"));
            await Repo<CountryRegion>().InsertAsync(new CountryRegion(NewId(), "UG", "Uganda"));
            await Repo<CountryRegion>().InsertAsync(new CountryRegion(NewId(), "TZ", "Tanzania"));
            await Repo<CountryRegion>().InsertAsync(new CountryRegion(NewId(), "GB", "United Kingdom"));
            await Repo<CountryRegion>().InsertAsync(new CountryRegion(NewId(), "US", "United States"));
        }

        if (await IsEmptyAsync<SourceCode>())
        {
            await Repo<SourceCode>().InsertAsync(new SourceCode(NewId(), "GENJNL", "General Journal"));
            await Repo<SourceCode>().InsertAsync(new SourceCode(NewId(), "SALES", "Sales"));
            await Repo<SourceCode>().InsertAsync(new SourceCode(NewId(), "PURCHASES", "Purchases"));
            await Repo<SourceCode>().InsertAsync(new SourceCode(NewId(), "REVERSAL", "Reversal Entry"));
            await Repo<SourceCode>().InsertAsync(new SourceCode(NewId(), GLRegister.ExchRateAdjustmentSourceCode, "Exchange Rate Adjustment"));
            await Repo<SourceCode>().InsertAsync(new SourceCode(NewId(), GLRegister.VatSettlementSourceCode, "VAT Settlement"));
            await Repo<SourceCode>().InsertAsync(new SourceCode(NewId(), "FAGLJNL", "Fixed Asset G/L Journal"));
        }

        await EnsureSeriesAsync("FA", "Fixed Assets", "FA000010");

        if (await IsEmptyAsync<DepreciationBook>())
        {
            await Repo<DepreciationBook>().InsertAsync(new DepreciationBook(NewId(), "COMPANY", "Company Book"));
        }

        if (await IsEmptyAsync<FASetup>())
        {
            var setup = new FASetup(NewId());
            setup.Set(allowPostingToMainAssets: false, defaultDeprBook: "COMPANY", allowFAPostingFrom: null, allowFAPostingTo: null, fixedAssetNos: "FA");
            await Repo<FASetup>().InsertAsync(setup);
        }

        if (await IsEmptyAsync<FAClass>())
        {
            await Repo<FAClass>().InsertAsync(new FAClass(NewId(), "TANGIBLE", "Tangible Fixed Assets"));
            await Repo<FAClass>().InsertAsync(new FAClass(NewId(), "INTANGIBLE", "Intangible Fixed Assets"));
        }

        if (await IsEmptyAsync<Relative>())
        {
            await Repo<Relative>().InsertAsync(new Relative(NewId(), "SPOUSE", "Spouse"));
            await Repo<Relative>().InsertAsync(new Relative(NewId(), "CHILD", "Child"));
            await Repo<Relative>().InsertAsync(new Relative(NewId(), "PARENT", "Parent"));
        }
    }
}
