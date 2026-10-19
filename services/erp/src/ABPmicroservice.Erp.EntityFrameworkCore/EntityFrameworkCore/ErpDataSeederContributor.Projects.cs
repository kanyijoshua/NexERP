using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Projects;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// The project accounts and posting group, the series of projects, resources and time sheets, and
/// a default project journal batch.
/// </summary>
public partial class ErpDataSeederContributor
{
    private async Task SeedProjectsAsync()
    {
        await EnsureAccountAsync("1450", "Work in Progress, Project Costs", GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("1460", "Work in Progress, Project Sales", GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("4100", "Project Sales", GLAccountCategory.Income, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("4110", "Recognized Project Sales", GLAccountCategory.Income, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("5300", "Project Costs", GLAccountCategory.CostOfGoodsSold, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("5310", "Project Costs Applied", GLAccountCategory.CostOfGoodsSold, IncomeBalanceType.IncomeStatement);

        if (await IsEmptyAsync<ProjectPostingGroup>())
        {
            var group = new ProjectPostingGroup(NewId(), "PROJECTS", "Customer projects");
            group.SetAccounts("5300", "4100", "1450", "5310", "1460", "4110");
            await Repo<ProjectPostingGroup>().InsertAsync(group);
        }

        await EnsureSeriesAsync("PROJ", "Projects", "PR00010");
        await EnsureSeriesAsync("RES", "Resources", "R0010");
        await EnsureSeriesAsync("TS", "Time sheets", "TS00001");

        if (await IsEmptyAsync<ProjectsSetup>())
        {
            var setup = new ProjectsSetup(NewId());
            setup.Set("PROJ", "RES", "TS", "PROJECTS");
            await Repo<ProjectsSetup>().InsertAsync(setup);
        }

        if (await IsEmptyAsync<ProjectJournalBatch>())
        {
            await Repo<ProjectJournalBatch>().InsertAsync(new ProjectJournalBatch(NewId(), "DEFAULT", "Project journal"));
        }
    }
}
