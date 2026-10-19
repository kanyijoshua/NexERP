using System;
using System.Text.Json;
using System.Threading.Tasks;
using ABPmicroservice.Erp.JobQueue;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.FixedAssets;

public class CalculateDepreciationParameters
{
    /// <summary>The FA journal batch the lines go to; created when missing.</summary>
    public string BatchName { get; set; } = "DEPR";

    /// <summary>Post the lines straight away instead of leaving them for review.</summary>
    public bool Post { get; set; }
}

/// <summary>
/// Calculates the depreciation of every asset up to the end of last month into an FA journal batch,
/// and posts it when asked to: the month-end depreciation run.
/// </summary>
public class CalculateDepreciationJobHandler : IJobHandler, ITransientDependency
{
    public string JobType => "CalculateDepreciation";
    public string DisplayName => "Calculate Depreciation";
    public string Description => "Calculates the depreciation of all fixed assets up to the end of last month into an FA journal batch.";
    public string DefaultParametersJson => "{\"BatchName\": \"DEPR\", \"Post\": false}";

    private readonly IRepository<FAJournalBatch, Guid> _batchRepository;
    private readonly FAJnlPostBatch _postBatch;

    public CalculateDepreciationJobHandler(IRepository<FAJournalBatch, Guid> batchRepository, FAJnlPostBatch postBatch)
    {
        _batchRepository = batchRepository;
        _postBatch = postBatch;
    }

    public async Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context)
    {
        var parameters = new CalculateDepreciationParameters();
        if (!string.IsNullOrWhiteSpace(context.ParameterString))
        {
            try
            {
                parameters = JsonSerializer.Deserialize<CalculateDepreciationParameters>(context.ParameterString) ?? parameters;
            }
            catch (JsonException)
            {
                // Fall back to the defaults.
            }
        }

        var name = (parameters.BatchName ?? "DEPR").Trim().ToUpperInvariant();
        var batch = await _batchRepository.FirstOrDefaultAsync(b => b.Name == name);
        if (batch == null)
        {
            batch = await _batchRepository.InsertAsync(new FAJournalBatch(Guid.NewGuid(), name, "Depreciation", null), autoSave: true);
        }

        var today = DateTime.Today;
        var until = new DateTime(today.Year, today.Month, 1).AddDays(-1);
        var lines = await _postBatch.CalculateDepreciationAsync(batch.Id, until, until, $"DEPR-{until:yyyyMM}");
        if (parameters.Post && lines > 0)
        {
            await _postBatch.PostAsync(batch.Id);
        }

        return JobExecutionResult.Ok(lines, $"Calculated depreciation for {lines} asset(s) up to {until:yyyy-MM-dd}.");
    }
}
