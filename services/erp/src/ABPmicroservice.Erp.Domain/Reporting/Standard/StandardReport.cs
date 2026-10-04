using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>What a standard report asks for on its request page.</summary>
[Flags]
public enum StandardReportParameters
{
    None = 0,

    /// <summary>A from and a to date.</summary>
    Period = 1,

    /// <summary>A single date the report is as at; sent as the to date.</summary>
    AsOfDate = 2,

    /// <summary>A filter on the record number, in filter syntax: "V0010..V0020|V0100".</summary>
    NoFilter = 4,

    /// <summary>A G/L budget name.</summary>
    BudgetName = 8,

    /// <summary>A depreciation book code.</summary>
    DepreciationBook = 16,

    /// <summary>A pension scheme; blank runs across every scheme.</summary>
    Scheme = 32,
}

/// <summary>The functional area a standard report belongs to; it decides the menu and the permission.</summary>
public static class StandardReportAreas
{
    public const string Finance = "Finance";
    public const string Sales = "Sales";
    public const string Purchasing = "Purchasing";
    public const string CashManagement = "CashManagement";
    public const string HumanResources = "HumanResources";
    public const string FixedAssets = "FixedAssets";
    public const string Pensions = "Pensions";
    public const string Academics = "Academics";
    public const string Payroll = "Payroll";
}

/// <summary>One report of the catalog, as the report list and Report Selections name it.</summary>
public sealed class StandardReportDefinition
{
    public StandardReportDefinition(int id, string code, string name, string area, StandardReportParameters parameters)
    {
        Id = id;
        Code = code;
        Name = name;
        Area = area;
        Parameters = parameters;
    }

    /// <summary>The report ID, e.g. 304 for Vendor - Detail Trial Balance.</summary>
    public int Id { get; }

    /// <summary>Stable key the API addresses the report by, e.g. "VendorDetailTrialBalance".</summary>
    public string Code { get; }

    /// <summary>The caption.</summary>
    public string Name { get; }

    public string Area { get; }

    public StandardReportParameters Parameters { get; }
}

public class StandardReportRequest
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string NoFilter { get; set; }

    public string BudgetName { get; set; }

    public string DepreciationBookCode { get; set; }

    public string SchemeCode { get; set; }

    /// <summary>The first day of the period; the beginning of time when none was given.</summary>
    public DateTime From => FromDate?.Date ?? DateTime.MinValue;

    /// <summary>The last day of the period; the end of time when none was given.</summary>
    public DateTime To => ToDate?.Date ?? DateTime.MaxValue.Date;
}

/// <summary>
/// A report of the standard catalog. Every one produces a <see cref="ReportResult"/>, so the one
/// report viewer and the one exporter serve all of them.
/// </summary>
public interface IStandardReport
{
    StandardReportDefinition Definition { get; }

    Task<ReportResult> RunAsync(StandardReportRequest request);
}

/// <summary>Shared plumbing of the standard reports: the result, its columns and the record filter.</summary>
public abstract class StandardReportBase : DomainService, IStandardReport, ITransientDependency
{
    public abstract StandardReportDefinition Definition { get; }

    public abstract Task<ReportResult> RunAsync(StandardReportRequest request);

    protected ReportResult NewResult(StandardReportRequest request)
    {
        if (request.FromDate.HasValue && request.ToDate.HasValue && request.ToDate < request.FromDate)
        {
            throw new BusinessException(ErpErrorCodes.Reports.PeriodReversed);
        }

        return new ReportResult
        {
            Title = Definition.Name,
            FromDate = request.FromDate ?? request.ToDate ?? Clock.Now.Date,
            ToDate = request.ToDate ?? request.FromDate ?? Clock.Now.Date,
        };
    }

    protected static void Text(ReportResult result, string key, string header) =>
        result.Columns.Add(new ReportColumnDefinition(key, header, ReportColumnKind.Text));

    protected static void Date(ReportResult result, string key, string header) =>
        result.Columns.Add(new ReportColumnDefinition(key, header, ReportColumnKind.Date));

    protected static void Number(ReportResult result, string key, string header) =>
        result.Columns.Add(new ReportColumnDefinition(key, header));

    /// <summary>Adds a row from key/value pairs; a missing column simply stays blank.</summary>
    protected static ReportRow Row(ReportResult result, params (string Key, object Value)[] values)
    {
        var row = result.AddRow();
        foreach (var (key, value) in values)
        {
            row.Values[key] = value;
        }

        return row;
    }

    /// <summary>A heading or total line.</summary>
    protected static ReportRow BoldRow(ReportResult result, params (string Key, object Value)[] values)
    {
        var row = Row(result, values);
        row.Bold = true;
        return row;
    }

    /// <summary>The request's record filter; everything when it is blank.</summary>
    protected static AccountTotaling Filter(StandardReportRequest request) =>
        request.NoFilter.IsNullOrWhiteSpace() ? AccountTotaling.All : AccountTotaling.Parse(request.NoFilter.Trim().ToUpperInvariant());

    protected static decimal Debit(decimal amount) => amount > 0 ? amount : 0m;

    protected static decimal Credit(decimal amount) => amount < 0 ? -amount : 0m;
}

/// <summary>The standard reports this system can run, by code.</summary>
public class StandardReportCatalog : DomainService
{
    private readonly Dictionary<string, IStandardReport> _byCode;

    public StandardReportCatalog(IEnumerable<IStandardReport> reports)
    {
        _byCode = reports.ToDictionary(r => r.Definition.Code, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<StandardReportDefinition> GetAll() =>
        _byCode.Values.Select(r => r.Definition).OrderBy(d => d.Area, StringComparer.Ordinal).ThenBy(d => d.Id).ToList();

    public IStandardReport Find(string code) => _byCode.GetValueOrDefault(code ?? string.Empty);

    /// <summary>The report Report Selections points at by its report ID.</summary>
    public IStandardReport FindById(int id) => _byCode.Values.FirstOrDefault(r => r.Definition.Id == id);

    public IStandardReport Get(string code)
    {
        return Find(code) ?? throw new BusinessException(ErpErrorCodes.Reports.UnknownStandardReport).WithData("code", code ?? string.Empty);
    }
}
