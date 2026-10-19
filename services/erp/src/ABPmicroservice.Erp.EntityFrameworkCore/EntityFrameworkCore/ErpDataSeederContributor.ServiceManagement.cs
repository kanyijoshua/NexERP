using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.ServiceManagement;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>The service and contract revenue accounts, the service series, and the common order types, faults and resolutions.</summary>
public partial class ErpDataSeederContributor
{
    private async Task SeedServiceManagementAsync()
    {
        await EnsureAccountAsync("4300", "Service Sales", GLAccountCategory.Income, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("4310", "Service Contract Sales", GLAccountCategory.Income, IncomeBalanceType.IncomeStatement);
        await EnsureSeriesAsync("SVO", "Service orders", "SVO100001");
        await EnsureSeriesAsync("SVITEM", "Service items", "SI100001");
        await EnsureSeriesAsync("SVCON", "Service contracts", "SC100001");

        if (await IsEmptyAsync<ServiceMgtSetup>())
        {
            var setup = new ServiceMgtSetup(NewId());
            setup.Set("SVO", "SVITEM", "SVCON", "4300", "4310", 12);
            await Repo<ServiceMgtSetup>().InsertAsync(setup);
        }

        if (await IsEmptyAsync<ServiceOrderType>())
        {
            await Repo<ServiceOrderType>().InsertAsync(new ServiceOrderType(NewId(), "REPAIR", "Repair"));
            await Repo<ServiceOrderType>().InsertAsync(new ServiceOrderType(NewId(), "MAINT", "Maintenance"));
            await Repo<ServiceOrderType>().InsertAsync(new ServiceOrderType(NewId(), "INSTALL", "Installation"));
        }

        if (await IsEmptyAsync<FaultCode>())
        {
            await Repo<FaultCode>().InsertAsync(new FaultCode(NewId(), "NOPOWER", "Does not power on"));
            await Repo<FaultCode>().InsertAsync(new FaultCode(NewId(), "NOISE", "Unusual noise"));
            await Repo<FaultCode>().InsertAsync(new FaultCode(NewId(), "WEAR", "Worn part"));
        }

        if (await IsEmptyAsync<ResolutionCode>())
        {
            await Repo<ResolutionCode>().InsertAsync(new ResolutionCode(NewId(), "REPLACED", "Part replaced"));
            await Repo<ResolutionCode>().InsertAsync(new ResolutionCode(NewId(), "ADJUSTED", "Adjusted"));
            await Repo<ResolutionCode>().InsertAsync(new ResolutionCode(NewId(), "NOFAULT", "No fault found"));
        }
    }
}
