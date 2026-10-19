using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Manufacturing;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>The production WIP, capacity and variance accounts, the production order series and an assembly work center.</summary>
public partial class ErpDataSeederContributor
{
    private async Task SeedManufacturingAsync()
    {
        await EnsureAccountAsync("1480", "Work in Progress, Production", GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("5400", "Capacity Costs Applied", GLAccountCategory.CostOfGoodsSold, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("5410", "Production Variance", GLAccountCategory.CostOfGoodsSold, IncomeBalanceType.IncomeStatement);
        await EnsureSeriesAsync("PROD", "Production orders", "PO100001");

        if (await IsEmptyAsync<ManufacturingSetup>())
        {
            var setup = new ManufacturingSetup(NewId());
            setup.Set("PROD", "1480", "5400", "5410");
            await Repo<ManufacturingSetup>().InsertAsync(setup);
        }

        if (await IsEmptyAsync<WorkCenter>())
        {
            var assembly = new WorkCenter(NewId(), "ASSEMBLY", "Assembly line");
            assembly.Set(60m, 8m);
            await Repo<WorkCenter>().InsertAsync(assembly);
        }
    }
}
