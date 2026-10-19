using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.JobQueue;
using Volo.Abp.DependencyInjection;

namespace ABPmicroservice.Erp.ServiceManagement;

/// <summary>Invoices every contract period that has started: run it daily, or once at the start of each month.</summary>
public class ServiceContractInvoicingJobHandler : IJobHandler, ITransientDependency
{
    public string JobType => "InvoiceServiceContracts";
    public string DisplayName => "Invoice Service Contracts";
    public string Description => "Creates and posts a sales invoice for every signed service contract period that has started.";
    public string DefaultParametersJson => "{}";

    private readonly ServicePostingEngine _engine;

    public ServiceContractInvoicingJobHandler(ServicePostingEngine engine)
    {
        _engine = engine;
    }

    public async Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context)
    {
        var invoiced = await _engine.InvoiceContractsAsync(DateTime.Today);
        return JobExecutionResult.Ok(invoiced, $"Invoiced {invoiced} contract period(s).");
    }
}
