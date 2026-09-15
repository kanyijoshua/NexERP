using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Domain service for exporting tab-delimited/CSV/Excel data streams.
/// </summary>
public class ExcelExportEngine : DomainService
{
    public byte[] ExportToCsv<T>(IEnumerable<T> data, Func<T, string[]> rowMapper, string[] headers)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers));

        foreach (var item in data)
        {
            var values = rowMapper(item);
            sb.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
