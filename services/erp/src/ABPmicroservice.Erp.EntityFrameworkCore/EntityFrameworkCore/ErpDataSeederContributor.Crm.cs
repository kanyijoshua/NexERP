using System.Threading.Tasks;
using ABPmicroservice.Erp.Crm;
using ABPmicroservice.Erp.Kanban;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// The lead and opportunity pipelines with their probabilities, the reasons opportunities close,
/// the kinds of activity, and the number series of contacts, opportunities and campaigns.
/// </summary>
public partial class ErpDataSeederContributor
{
    private async Task SeedCrmAsync()
    {
        if (!await Repo<KanbanStage>().AnyAsync(s => s.PipelineType == OpportunityManager.LeadPipeline))
        {
            await AddStageAsync(OpportunityManager.LeadPipeline, 1, "New", 5m);
            await AddStageAsync(OpportunityManager.LeadPipeline, 2, "Contacted", 10m);
            await AddStageAsync(OpportunityManager.LeadPipeline, 3, "Qualified", 20m);
            await AddStageAsync(OpportunityManager.LeadPipeline, 4, "Disqualified", 0m, lost: true);
        }

        if (!await Repo<KanbanStage>().AnyAsync(s => s.PipelineType == OpportunityManager.OpportunityPipeline))
        {
            await AddStageAsync(OpportunityManager.OpportunityPipeline, 1, "Qualification", 20m);
            await AddStageAsync(OpportunityManager.OpportunityPipeline, 2, "Proposal", 50m);
            await AddStageAsync(OpportunityManager.OpportunityPipeline, 3, "Negotiation", 75m);
            await AddStageAsync(OpportunityManager.OpportunityPipeline, 4, "Won", 100m, won: true);
            await AddStageAsync(OpportunityManager.OpportunityPipeline, 5, "Lost", 0m, lost: true);
        }

        if (await IsEmptyAsync<CloseOpportunityCode>())
        {
            await Repo<CloseOpportunityCode>().InsertAsync(new CloseOpportunityCode(NewId(), "PRICE", "Better price elsewhere", CloseOpportunityType.Lost));
            await Repo<CloseOpportunityCode>().InsertAsync(new CloseOpportunityCode(NewId(), "NO-NEED", "No longer needed", CloseOpportunityType.Lost));
            await Repo<CloseOpportunityCode>().InsertAsync(new CloseOpportunityCode(NewId(), "COMPETITOR", "Went to a competitor", CloseOpportunityType.Lost));
            await Repo<CloseOpportunityCode>().InsertAsync(new CloseOpportunityCode(NewId(), "BEST-OFFER", "Best offer", CloseOpportunityType.Won));
            await Repo<CloseOpportunityCode>().InsertAsync(new CloseOpportunityCode(NewId(), "RELATION", "Existing relationship", CloseOpportunityType.Won));
        }

        if (await IsEmptyAsync<ActivityType>())
        {
            await Repo<ActivityType>().InsertAsync(new ActivityType(NewId(), "CALL", "Phone call", "fas fa-phone"));
            await Repo<ActivityType>().InsertAsync(new ActivityType(NewId(), "EMAIL", "E-mail", "fas fa-envelope"));
            await Repo<ActivityType>().InsertAsync(new ActivityType(NewId(), "MEETING", "Meeting", "fas fa-people-group"));
            await Repo<ActivityType>().InsertAsync(new ActivityType(NewId(), "DEMO", "Demonstration", "fas fa-display"));
            await Repo<ActivityType>().InsertAsync(new ActivityType(NewId(), "TODO", "To do", "fas fa-list-check"));
        }

        await EnsureSeriesAsync("CONT", "Contacts", "CT000010");
        await EnsureSeriesAsync("OPP", "Opportunities", "OP000010");
        await EnsureSeriesAsync("CAMP", "Campaigns", "CP000010");

        if (await IsEmptyAsync<MarketingSetup>())
        {
            var setup = new MarketingSetup(NewId());
            setup.SetNumbering("CONT", "OPP", "CAMP");
            setup.SetCustomerDefaults("DOMESTIC", "DOMESTIC", "DOMESTIC", "30D");
            await Repo<MarketingSetup>().InsertAsync(setup);
        }
    }

    private async Task AddStageAsync(string pipeline, int sequence, string name, decimal probability, bool won = false, bool lost = false)
    {
        var stage = new KanbanStage(NewId(), pipeline, sequence, name, foldedInKanban: won || lost, isWonStage: won);
        stage.Update(pipeline, sequence, name, won || lost, won, lost, probability);
        await Repo<KanbanStage>().InsertAsync(stage);
    }
}
