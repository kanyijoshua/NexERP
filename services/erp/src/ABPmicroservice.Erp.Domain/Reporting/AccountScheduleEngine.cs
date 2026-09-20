using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>What to run and over which period.</summary>
public class AccountScheduleRunRequest
{
    public string ScheduleName { get; set; }

    /// <summary>Blank runs a single "Net Change" column over the period.</summary>
    public string ColumnLayoutName { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }
}

/// <summary>
/// Evaluates an account schedule against a column layout.
/// Mirrors Business Central codeunit 8 "AccSchedManagement".
/// <para>
/// Rows say what to add up (account ranges, or other rows through a formula), columns say over
/// which period. This is what makes a balance sheet or an income statement something a user
/// defines rather than something that has to be coded.
/// </para>
/// </summary>
public class AccountScheduleEngine : DomainService
{
    private readonly IRepository<AccountSchedule, Guid> _scheduleRepository;
    private readonly IRepository<AccountScheduleLine, Guid> _scheduleLineRepository;
    private readonly IRepository<ColumnLayout, Guid> _columnLayoutRepository;
    private readonly IRepository<ColumnLayoutLine, Guid> _columnLayoutLineRepository;
    private readonly IRepository<GLEntry, Guid> _glEntryRepository;

    public AccountScheduleEngine(
        IRepository<AccountSchedule, Guid> scheduleRepository,
        IRepository<AccountScheduleLine, Guid> scheduleLineRepository,
        IRepository<ColumnLayout, Guid> columnLayoutRepository,
        IRepository<ColumnLayoutLine, Guid> columnLayoutLineRepository,
        IRepository<GLEntry, Guid> glEntryRepository
    )
    {
        _scheduleRepository = scheduleRepository;
        _scheduleLineRepository = scheduleLineRepository;
        _columnLayoutRepository = columnLayoutRepository;
        _columnLayoutLineRepository = columnLayoutLineRepository;
        _glEntryRepository = glEntryRepository;
    }

    public async Task<ReportResult> RunAsync(AccountScheduleRunRequest request)
    {
        Check.NotNull(request, nameof(request));

        var schedule = await _scheduleRepository.FirstOrDefaultAsync(s => s.Name == request.ScheduleName);
        if (schedule == null)
        {
            throw new BusinessException(ErpErrorCodes.Reports.ScheduleNotFound)
                .WithData("scheduleName", request.ScheduleName);
        }

        if (request.ToDate < request.FromDate)
        {
            throw new BusinessException(ErpErrorCodes.Reports.PeriodReversed);
        }

        var lines = (await _scheduleLineRepository.GetListAsync(l => l.AccountScheduleId == schedule.Id))
            .OrderBy(l => l.LineNo)
            .ToList();

        var columns = await GetColumnsAsync(request);
        var balances = await LoadBalancesAsync(columns);

        var result = new ReportResult
        {
            Title = schedule.Description.IsNullOrWhiteSpace() ? schedule.Name : schedule.Description,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
        };

        result.Columns.Add(new ReportColumnDefinition("rowNo", "Row", ReportColumnKind.Text));
        result.Columns.Add(new ReportColumnDefinition("description", "Description", ReportColumnKind.Text));
        foreach (var column in columns)
        {
            result.Columns.Add(new ReportColumnDefinition(column.Key, column.Header));
        }

        var byRowNo = new Dictionary<string, AccountScheduleLine>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
        {
            byRowNo[line.RowNo] = line;
        }

        var evaluator = new RowEvaluator(byRowNo, balances);

        foreach (var line in lines)
        {
            var values = new Dictionary<string, decimal>();
            foreach (var column in columns)
            {
                values[column.Key] = evaluator.Evaluate(line, column);
            }

            if (line.HideIfZero && values.Values.All(v => v == 0m))
            {
                continue;
            }

            var row = result.AddRow();
            row.Values["rowNo"] = line.RowNo;
            row.Values["description"] = line.Description;
            row.Bold = line.Bold;
            row.Italic = line.Italic;
            row.Indentation = line.Indentation;

            if (line.TotalingType is AccountScheduleTotalingType.PostingAccounts or AccountScheduleTotalingType.TotalAccounts)
            {
                row.DrillDownFilter = line.Totaling;
            }

            foreach (var column in columns)
            {
                row.Values[column.Key] = values[column.Key];
            }
        }

        return result;
    }

    private async Task<List<ColumnWindow>> GetColumnsAsync(AccountScheduleRunRequest request)
    {
        if (request.ColumnLayoutName.IsNullOrWhiteSpace())
        {
            return
            [
                new ColumnWindow("c1", "Net Change", ColumnLayoutType.NetChange, request.FromDate, request.ToDate, false),
            ];
        }

        var layout = await _columnLayoutRepository.FirstOrDefaultAsync(l => l.Name == request.ColumnLayoutName);
        if (layout == null)
        {
            throw new BusinessException(ErpErrorCodes.Reports.ScheduleNotFound)
                .WithData("scheduleName", request.ColumnLayoutName);
        }

        var layoutLines = (await _columnLayoutLineRepository.GetListAsync(l => l.ColumnLayoutId == layout.Id))
            .OrderBy(l => l.LineNo)
            .ToList();

        var columns = new List<ColumnWindow>();
        foreach (var layoutLine in layoutLines)
        {
            var from = request.FromDate;
            var to = request.ToDate;

            if (layoutLine.ComparisonDateFormula != null)
            {
                var shift = DateFormula.Parse(layoutLine.ComparisonDateFormula);
                from = shift.Apply(from);
                to = shift.Apply(to);
            }

            columns.Add(
                new ColumnWindow(
                    layoutLine.ColumnNo.ToLowerInvariant(),
                    layoutLine.ColumnHeader.IsNullOrWhiteSpace() ? layoutLine.ColumnNo : layoutLine.ColumnHeader,
                    layoutLine.ColumnType,
                    from,
                    to,
                    layoutLine.ShowOppositeSign
                )
            );
        }

        return columns.Count == 0
            ?
            [
                new ColumnWindow("c1", "Net Change", ColumnLayoutType.NetChange, request.FromDate, request.ToDate, false),
            ]
            : columns;
    }

    /// <summary>
    /// Reads every entry up to the latest date any column needs, once. Each column then sums the
    /// part of it inside its own window, so the report costs one query however many columns it has.
    /// </summary>
    private async Task<List<PostedAmount>> LoadBalancesAsync(IEnumerable<ColumnWindow> columns)
    {
        var lastDate = columns.Max(c => c.ToDate);
        var entries = await _glEntryRepository.GetListAsync(e => e.PostingDate <= lastDate);

        return entries.Select(e => new PostedAmount(e.GLAccountNo, e.PostingDate, e.Amount)).ToList();
    }

    private readonly record struct PostedAmount(string AccountNo, DateTime PostingDate, decimal Amount);

    private sealed record ColumnWindow(
        string Key,
        string Header,
        ColumnLayoutType Type,
        DateTime FromDate,
        DateTime ToDate,
        bool ShowOppositeSign
    )
    {
        /// <summary>The date range this column actually sums, given its column type.</summary>
        public (DateTime From, DateTime To) Period =>
            Type switch
            {
                ColumnLayoutType.NetChange => (FromDate, ToDate),
                ColumnLayoutType.BalanceAtDate => (DateTime.MinValue, ToDate),
                ColumnLayoutType.BeginningBalance => (DateTime.MinValue, FromDate.AddDays(-1)),
                // No accounting periods yet, so a fiscal year is a calendar year.
                ColumnLayoutType.YearToDateNetChange => (new DateTime(ToDate.Year, 1, 1), ToDate),
                _ => (FromDate, ToDate),
            };
    }

    /// <summary>
    /// Works out one row's amount in one column, remembering what it has already worked out so a
    /// formula row that refers to the same row twice costs nothing extra.
    /// </summary>
    private sealed class RowEvaluator
    {
        private readonly Dictionary<string, AccountScheduleLine> _byRowNo;
        private readonly List<PostedAmount> _balances;
        private readonly Dictionary<(string RowNo, string ColumnKey), decimal> _memo = new();
        private readonly HashSet<string> _visiting = new(StringComparer.OrdinalIgnoreCase);

        public RowEvaluator(Dictionary<string, AccountScheduleLine> byRowNo, List<PostedAmount> balances)
        {
            _byRowNo = byRowNo;
            _balances = balances;
        }

        public decimal Evaluate(AccountScheduleLine line, ColumnWindow column)
        {
            var key = (line.RowNo, column.Key);
            if (_memo.TryGetValue(key, out var cached))
            {
                return cached;
            }

            if (!_visiting.Add(line.RowNo))
            {
                throw new BusinessException(ErpErrorCodes.Reports.CircularRowFormula).WithData("rowNo", line.RowNo);
            }

            try
            {
                var amount = line.TotalingType switch
                {
                    AccountScheduleTotalingType.Description => 0m,
                    AccountScheduleTotalingType.Formula => EvaluateFormula(line, column),
                    _ => SumAccounts(line.Totaling, column),
                };

                if (line.ShowOppositeSign)
                {
                    amount = -amount;
                }

                if (column.ShowOppositeSign)
                {
                    amount = -amount;
                }

                _memo[key] = amount;
                return amount;
            }
            finally
            {
                _visiting.Remove(line.RowNo);
            }
        }

        private decimal EvaluateFormula(AccountScheduleLine line, ColumnWindow column)
        {
            var total = 0m;

            foreach (var (sign, rowNo) in RowFormula.Parse(line.Totaling, line.RowNo))
            {
                if (!_byRowNo.TryGetValue(rowNo, out var referenced))
                {
                    throw new BusinessException(ErpErrorCodes.Reports.UnknownRowReference)
                        .WithData("rowNo", line.RowNo)
                        .WithData("reference", rowNo);
                }

                total += sign * Evaluate(referenced, column);
            }

            return total;
        }

        private decimal SumAccounts(string totaling, ColumnWindow column)
        {
            var filter = AccountTotaling.Parse(totaling);
            if (filter.IsEmpty)
            {
                return 0m;
            }

            var (from, to) = column.Period;

            return _balances
                .Where(b => b.PostingDate >= from && b.PostingDate <= to && filter.Matches(b.AccountNo))
                .Sum(b => b.Amount);
        }

    }
}
