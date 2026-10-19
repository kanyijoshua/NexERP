using System;
using System.Text.Json;
using System.Threading.Tasks;
using ABPmicroservice.Erp.JobQueue;
using Volo.Abp.DependencyInjection;

namespace ABPmicroservice.Erp.Manufacturing;

public class CalculatePlanParameters
{
    /// <summary>How many days ahead the plan looks.</summary>
    public int HorizonDays { get; set; } = 30;
}

/// <summary>Recalculates the planning worksheet in the background, so buyers and planners start the day with fresh suggestions.</summary>
public class CalculatePlanJobHandler : IJobHandler, ITransientDependency
{
    public string JobType => "CalculatePlan";
    public string DisplayName => "Calculate Plan";
    public string Description => "Fills the planning worksheet with what to buy and make, from stock, open orders, reorder points and safety stock.";
    public string DefaultParametersJson => "{\"HorizonDays\": 30}";

    private readonly PlanningManager _planning;

    public CalculatePlanJobHandler(PlanningManager planning)
    {
        _planning = planning;
    }

    public async Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context)
    {
        var parameters = new CalculatePlanParameters();
        if (!string.IsNullOrWhiteSpace(context.ParameterString))
        {
            try
            {
                parameters = JsonSerializer.Deserialize<CalculatePlanParameters>(context.ParameterString) ?? parameters;
            }
            catch (JsonException)
            {
                // Fall back to the defaults.
            }
        }

        var lines = await _planning.CalculateAsync(DateTime.Today.AddDays(Math.Max(1, parameters.HorizonDays)));
        return JobExecutionResult.Ok(lines, $"Suggested {lines} planning line(s).");
    }
}
