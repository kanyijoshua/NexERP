using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Reporting;

public class FinancialReportResultDto
{
    public string ReportTitle { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public List<FinancialReportRowDto> Rows { get; set; } = new();
}

public class FinancialReportRowDto
{
    public string RowNo { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Domain service for financial report calculation (Balance Sheet, Income Statement, Trial Balance).
/// </summary>
public class FinancialReportEngine : DomainService
{
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly IRepository<GLEntry, Guid> _glEntryRepository;

    public FinancialReportEngine(
        IRepository<GLAccount, Guid> glAccountRepository,
        IRepository<GLEntry, Guid> glEntryRepository
    )
    {
        _glAccountRepository = glAccountRepository;
        _glEntryRepository = glEntryRepository;
    }

    public async Task<FinancialReportResultDto> GenerateTrialBalanceAsync(DateTime fromDate, DateTime toDate)
    {
        var accounts = await _glAccountRepository.GetListAsync();
        var entries = await _glEntryRepository.GetListAsync(e => e.PostingDate >= fromDate && e.PostingDate <= toDate);

        var report = new FinancialReportResultDto
        {
            ReportTitle = "Trial Balance",
            FromDate = fromDate,
            ToDate = toDate
        };

        foreach (var acc in accounts.OrderBy(a => a.No))
        {
            decimal netChange = entries.Where(e => e.GLAccountId == acc.Id).Sum(e => e.Amount);
            report.Rows.Add(new FinancialReportRowDto
            {
                RowNo = acc.No,
                Description = acc.Name,
                Amount = netChange
            });
        }

        return report;
    }
}
