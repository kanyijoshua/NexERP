using System;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;

namespace ABPmicroservice.Erp.JobQueue;

/// <summary>
/// A category for grouping job queue entries and managing worker queues.
/// Mirrors Business Central Table 471 "Job Queue Category" and Odoo queue.job.channel.
/// </summary>
public class JobQueueCategory : CompanyAggregateRoot
{
    public string Code { get; private set; }
    public string Description { get; private set; }

    protected JobQueueCategory() { }

    public JobQueueCategory(Guid id, string code, string description = null)
        : base(id)
    {
        SetCode(code);
        SetDescription(description);
    }

    public void Update(string description)
    {
        SetDescription(description);
    }

    private void SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxJobCategoryCodeLength)
            .Trim()
            .ToUpperInvariant();
    }

    private void SetDescription(string description)
    {
        Description = Check.Length(description?.Trim(), nameof(description), ErpDomainConsts.MaxDescriptionLength);
    }
}
