using System;
using System.Text.Json;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Purchasing;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.JobQueue;

public class PurchasingPostBatchParameters
{
    public int MaxDocumentsToPost { get; set; } = 25;
}

/// <summary>
/// Background job that batch posts released purchase invoices.
/// </summary>
public class PurchasingPostBatchJobHandler : IJobHandler, ITransientDependency
{
    public string JobType => "PurchasingPostBatch";
    public string DisplayName => "Batch Post Purchase Documents";
    public string Description => "Posts released purchase invoices in the background up to the specified batch limit.";
    public string DefaultParametersJson => "{\"MaxDocumentsToPost\": 25}";

    private readonly IRepository<PurchaseHeader, Guid> _purchaseHeaderRepository;
    private readonly PurchasePostingEngine _postingEngine;
    private readonly ILogger<PurchasingPostBatchJobHandler> _logger;

    public PurchasingPostBatchJobHandler(
        IRepository<PurchaseHeader, Guid> purchaseHeaderRepository,
        PurchasePostingEngine postingEngine,
        ILogger<PurchasingPostBatchJobHandler> logger
    )
    {
        _purchaseHeaderRepository = purchaseHeaderRepository;
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
                var parsed = JsonSerializer.Deserialize<PurchasingPostBatchParameters>(context.ParameterString);
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

        var released = await _purchaseHeaderRepository.GetListAsync(h => h.Status == DocumentStatus.Released);
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
                _logger.LogInformation("Batch posted purchase document {No}", header.No);
            }
            catch (Exception ex)
            {
                errorCount++;
                _logger.LogWarning(ex, "Failed to batch post purchase document {No}", header.No);
            }
        }

        var output = $"Posted {postedCount} purchase document(s). Failures: {errorCount}.";
        return errorCount > 0 && postedCount == 0
            ? JobExecutionResult.Fail(output)
            : JobExecutionResult.Ok(postedCount, output);
    }
}
