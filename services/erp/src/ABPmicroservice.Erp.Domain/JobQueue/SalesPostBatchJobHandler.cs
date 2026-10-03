using System;
using System.Text.Json;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Sales;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.JobQueue;

public class SalesPostBatchParameters
{
    public int MaxDocumentsToPost { get; set; } = 25;
}

/// <summary>
/// Background job that batch posts released sales invoices.
/// Mirrors Business Central Report 296 "Batch Post Sales Invoices".
/// </summary>
public class SalesPostBatchJobHandler : IJobHandler, ITransientDependency
{
    public string JobType => "SalesPostBatch";
    public string DisplayName => "Batch Post Sales Documents";
    public string Description => "Posts released sales invoices in the background up to the specified batch limit.";
    public string DefaultParametersJson => "{\"MaxDocumentsToPost\": 25}";

    private readonly IRepository<SalesHeader, Guid> _salesHeaderRepository;
    private readonly SalesPostingEngine _postingEngine;
    private readonly ILogger<SalesPostBatchJobHandler> _logger;

    public SalesPostBatchJobHandler(
        IRepository<SalesHeader, Guid> salesHeaderRepository,
        SalesPostingEngine postingEngine,
        ILogger<SalesPostBatchJobHandler> logger
    )
    {
        _salesHeaderRepository = salesHeaderRepository;
        _postingEngine = postingEngine;
        _logger = logger;
    }

    public async Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context)
    {
        var limit = 25;
        if (!string.IsNullOrWhiteSpace(context.ParameterString))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<SalesPostBatchParameters>(context.ParameterString);
                if (parsed != null && parsed.MaxDocumentsToPost > 0)
                {
                    limit = parsed.MaxDocumentsToPost;
                }
            }
            catch
            {
                // Fall back to default
            }
        }

        var released = await _salesHeaderRepository.GetListAsync(h => h.Status == DocumentStatus.Released);
        var postedCount = 0;
        var errorCount = 0;

        foreach (var header in released)
        {
            if (postedCount >= limit || context.CancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await _postingEngine.PostAsync(header.Id);
                postedCount++;
                _logger.LogInformation("Batch posted sales document {No}", header.No);
            }
            catch (Exception ex)
            {
                errorCount++;
                _logger.LogWarning(ex, "Failed to batch post sales document {No}", header.No);
            }
        }

        var output = $"Posted {postedCount} sales document(s). Failures: {errorCount}.";
        return errorCount > 0 && postedCount == 0
            ? JobExecutionResult.Fail(output)
            : JobExecutionResult.Ok(postedCount, output);
    }
}
