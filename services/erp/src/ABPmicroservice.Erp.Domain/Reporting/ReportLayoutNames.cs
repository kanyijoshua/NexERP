using System;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// How a report is named when a layout is attached to it.
/// <para>
/// Layouts are attached to a report, and each Financial Report is its own
/// report: every fixed report is named after its kind, and an account
/// schedule is named after itself, so two schedules can print through different layouts.
/// </para>
/// </summary>
public static class ReportLayoutNames
{
    private const string SchedulePrefix = "AccountSchedule:";

    public static string For(ReportKind kind, string scheduleName = null)
    {
        if (kind != ReportKind.AccountSchedule)
        {
            return kind.ToString();
        }

        return scheduleName.IsNullOrWhiteSpace()
            ? nameof(ReportKind.AccountSchedule)
            : SchedulePrefix + scheduleName.Trim();
    }

    private const string StandardReportPrefix = "StandardReport:";

    /// <summary>The name a layout is attached to a report of the standard catalog under.</summary>
    public static string ForStandardReport(string code) => StandardReportPrefix + code;

    /// <summary>The standard report a report name refers to, or null if it names something else.</summary>
    public static string StandardReportOf(string reportName)
    {
        return reportName != null && reportName.StartsWith(StandardReportPrefix, StringComparison.OrdinalIgnoreCase)
            ? reportName[StandardReportPrefix.Length..]
            : null;
    }

    /// <summary>The schedule a report name refers to, or null if it names a fixed report.</summary>
    public static string ScheduleOf(string reportName)
    {
        return reportName != null && reportName.StartsWith(SchedulePrefix, StringComparison.Ordinal)
            ? reportName[SchedulePrefix.Length..]
            : null;
    }
}
