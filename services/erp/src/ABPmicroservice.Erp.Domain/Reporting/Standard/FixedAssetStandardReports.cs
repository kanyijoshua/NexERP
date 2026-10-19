using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.FixedAssets;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Fixed Asset Projected Value: the depreciation each asset will take month by month from the
/// first day of the period to its end, and the book value it leaves, as the depreciation run would
/// post it. Nothing is posted.
/// </summary>
public class FixedAssetProjectedValueReport : StandardReportBase
{
    private readonly IRepository<FADepreciationBook, Guid> _assetBooks;
    private readonly IRepository<DepreciationBook, Guid> _books;
    private readonly FAJnlPostLine _postLine;

    public FixedAssetProjectedValueReport(IRepository<FADepreciationBook, Guid> assetBooks, IRepository<DepreciationBook, Guid> books, FAJnlPostLine postLine)
    {
        _assetBooks = assetBooks;
        _books = books;
        _postLine = postLine;
    }

    public override StandardReportDefinition Definition { get; } =
        new(5607, "FixedAssetProjectedValue", "Fixed Asset - Projected Value", StandardReportAreas.FixedAssets, StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "faNo", "No.");
        Date(result, "date", "Period End");
        Number(result, "depreciation", "Depreciation");
        Number(result, "bookValue", "Book Value");

        var filter = Filter(request);
        var bookCode = await _postLine.DefaultBookAsync();
        var book = await _books.FirstAsync(b => b.Code == bookCode);
        var from = request.FromDate?.Date ?? DateTime.Today;
        var to = request.ToDate?.Date ?? from.AddYears(1);
        var assetBooks = (await _assetBooks.GetListAsync(b => b.DepreciationBookCode == bookCode && b.DisposalDate == null && b.AcquisitionDate != null))
            .Where(b => filter.Matches(b.FANo))
            .OrderBy(b => b.FANo, StringComparer.Ordinal);

        foreach (var assetBook in assetBooks)
        {
            var values = await _postLine.GetValuesAsync(assetBook.FANo, bookCode);
            var input = FAJnlPostBatch.InputOf(assetBook, book, values);
            var bookValue = values.BookValue;
            var last = input.LastDepreciationDate;
            for (var end = new DateTime(from.Year, from.Month, 1).AddMonths(1).AddDays(-1); end <= to; end = new DateTime(end.Year, end.Month, 1).AddMonths(2).AddDays(-1))
            {
                var period = DepreciationCalculator.Calculate(input with { BookValue = bookValue, LastDepreciationDate = last }, end);
                if (period.Amount == 0m)
                {
                    continue;
                }

                bookValue += period.Amount;
                last = end;
                Row(result, ("faNo", assetBook.FANo), ("date", end), ("depreciation", period.Amount), ("bookValue", bookValue));
            }
        }

        return result;
    }
}
