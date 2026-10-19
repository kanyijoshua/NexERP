using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.FixedAssets;

/// <summary>
/// Acquiring, depreciating and disposing of fixed assets through the FA journal and through
/// purchase and sales documents. The seed has the COMPANY book integrated with the G/L, FA posting
/// group EQUIPMENT (cost 1700, accumulated depreciation 1710, depreciation 8200, gains 4960,
/// losses 8220) and FA journal batches DEFAULT and DEPR.
/// </summary>
public class FixedAssetPosting_Tests : ErpApplicationTestBase
{
    private readonly IFAJournalBatchAppService _batches;
    private readonly IFAJournalLineAppService _lines;
    private readonly IFixedAssetValueAppService _values;
    private readonly IRepository<FixedAsset, Guid> _assets;
    private readonly IRepository<FADepreciationBook, Guid> _assetBooks;
    private readonly IRepository<FALedgerEntry, Guid> _faEntries;
    private readonly IRepository<GLEntry, Guid> _glEntries;

    public FixedAssetPosting_Tests()
    {
        _batches = GetRequiredService<IFAJournalBatchAppService>();
        _lines = GetRequiredService<IFAJournalLineAppService>();
        _values = GetRequiredService<IFixedAssetValueAppService>();
        _assets = GetRequiredService<IRepository<FixedAsset, Guid>>();
        _assetBooks = GetRequiredService<IRepository<FADepreciationBook, Guid>>();
        _faEntries = GetRequiredService<IRepository<FALedgerEntry, Guid>>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
    }

    [Fact]
    public async Task Straight_Line_Depreciates_Evenly_And_Not_Twice_For_A_Period()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewAssetAsync("FA-SL", FADepreciationMethod.StraightLine, years: 5m);
            var batch = await BatchAsync("DEFAULT");
            await LineAsync(batch, "FA-SL", FAJournalPostingType.AcquisitionCost, 12000m, new DateTime(2026, 1, 1), "ACQ-1", "1010");
            await _batches.RunPostingAsync(batch.Id);

            var acquisition = await _glEntries.GetListAsync(e => e.DocumentNo == "ACQ-1");
            Sum(acquisition, "1700").ShouldBe(12000m);
            Sum(acquisition, "1010").ShouldBe(-12000m);

            var depr = await BatchAsync("DEPR");
            (await _batches.CalculateDepreciationAsync(depr.Id, new CalculateDepreciationInput { UntilDate = new DateTime(2026, 1, 31), DocumentNo = "DEPR-01" })).ShouldBe(1);
            var line = (await _lines.GetListAsync(new GetFAJournalLineListInput { FAJournalBatchId = depr.Id })).Items.Single();
            line.Amount.ShouldBe(-200m); // 12,000 over 60 months
            line.NoOfDepreciationDays.ShouldBe(30);
            await _batches.RunPostingAsync(depr.Id);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == "DEPR-01");
            Sum(entries, "8200").ShouldBe(200m);
            Sum(entries, "1710").ShouldBe(-200m);

            // The same period again: already depreciated, nothing to add.
            (await _batches.CalculateDepreciationAsync(depr.Id, new CalculateDepreciationInput { UntilDate = new DateTime(2026, 1, 31), DocumentNo = "DEPR-01B" })).ShouldBe(0);

            // A whole year more is twelve months more.
            await _batches.CalculateDepreciationAsync(depr.Id, new CalculateDepreciationInput { UntilDate = new DateTime(2027, 1, 31), DocumentNo = "DEPR-02" });
            (await _lines.GetListAsync(new GetFAJournalLineListInput { FAJournalBatchId = depr.Id })).Items.Single().Amount.ShouldBe(-2400m);

            var values = (await _values.GetBookValuesAsync(new GetFABookValuesInput { FANo = "FA-SL" })).Single();
            values.BookValue.ShouldBe(11800m);
            values.LastDepreciationDate.ShouldBe(new DateTime(2026, 1, 31));
        });
    }

    [Fact]
    public void The_Calculator_Covers_Declining_Balance_The_Last_Period_And_The_Ending_Value()
    {
        var input = new DepreciationInput(
            FADepreciationMethod.DecliningBalance1,
            new DateTime(2026, 1, 1),
            null,
            0m,
            0m,
            0m,
            20m,
            10000m,
            10000m,
            0m,
            0m,
            false,
            false,
            null
        );

        DepreciationCalculator.Calculate(input, new DateTime(2026, 12, 31)).Amount.ShouldBe(-2000m); // 20% of 10,000
        DepreciationCalculator.Calculate(input with { BookValue = 8000m, LastDepreciationDate = new DateTime(2026, 12, 31) }, new DateTime(2027, 12, 31)).Amount.ShouldBe(-1600m);

        // Straight-line: the last month of the life takes whatever is left, never below the ending book value.
        var straight = input with { Method = FADepreciationMethod.StraightLine, NoOfDepreciationYears = 1m, BookValue = 1000.03m, DepreciableBasis = 12000m, EndingBookValue = 1m, LastDepreciationDate = new DateTime(2026, 11, 30) };
        DepreciationCalculator.Calculate(straight, new DateTime(2026, 12, 31)).Amount.ShouldBe(-999.03m);

        // Half-year convention: an asset bought in March depreciates half a year in its first year.
        var halfYear = input with { Method = FADepreciationMethod.StraightLine, NoOfDepreciationYears = 5m, StartingDate = new DateTime(2026, 3, 10), UseHalfYearConvention = true };
        DepreciationCalculator.Calculate(halfYear, new DateTime(2026, 12, 31)).Amount.ShouldBe(-1000m); // 10,000 / 5 / 2

        DepreciationCalculator.Days(new DateTime(2026, 2, 1), new DateTime(2026, 2, 28), actual: false).ShouldBe(30);
    }

    [Fact]
    public async Task A_Purchase_Invoice_Acquires_The_Asset()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewAssetAsync("FA-PUR", FADepreciationMethod.StraightLine, years: 4m);
            var vendor = await GetRequiredService<IVendorAppService>().GetByNoAsync("V00010");
            var documents = GetRequiredService<IPurchaseDocumentAppService>();
            var invoice = await documents.CreateAsync(new CreateUpdatePurchaseHeaderDto
            {
                DocumentType = PurchaseDocumentType.Invoice,
                No = "PI-FA-1",
                VendorId = vendor.Id,
                PostingDate = new DateTime(2026, 2, 1),
                VendorInvoiceNo = "V-FA-1",
                Lines = [new PurchaseLineInputDto { Type = DocumentLineType.FixedAsset, No = "FA-PUR", Description = "Delivery van", Quantity = 1, DirectUnitCost = 4800m }],
            });
            var postedNo = (await documents.RunPostingAsync(invoice.Id)).PostedDocumentNo;

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == postedNo);
            Sum(entries, "1700").ShouldBe(4800m);
            entries.Sum(e => e.Amount).ShouldBe(0m);

            var fa = (await _faEntries.GetListAsync(e => e.FANo == "FA-PUR")).ShouldHaveSingleItem();
            fa.FAPostingType.ShouldBe(FALedgerEntryFAPostingType.AcquisitionCost);
            fa.Amount.ShouldBe(4800m);
            (await _assetBooks.FirstAsync(b => b.FANo == "FA-PUR")).AcquisitionDate.ShouldBe(new DateTime(2026, 2, 1));
        });
    }

    [Fact]
    public async Task Selling_An_Asset_Above_Its_Book_Value_Books_A_Gain_And_Leaves_Nothing()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await AcquireAndDepreciateAsync("FA-GAIN"); // cost 12,000, depreciated 200

            var customer = await GetRequiredService<ICustomerAppService>().GetByNoAsync("C00010");
            var documents = GetRequiredService<ISalesDocumentAppService>();
            var invoice = await documents.CreateAsync(new CreateUpdateSalesHeaderDto
            {
                DocumentType = SalesDocumentType.Invoice,
                No = "SI-FA-1",
                CustomerId = customer.Id,
                PostingDate = new DateTime(2026, 2, 15),
                Lines = [new SalesLineInputDto { Type = DocumentLineType.FixedAsset, No = "FA-GAIN", Description = "Used machine", Quantity = 1, UnitPrice = 12500m }],
            });
            var postedNo = (await documents.RunPostingAsync(invoice.Id)).PostedDocumentNo;

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == postedNo);
            Sum(entries, "1700").ShouldBe(-12000m);
            Sum(entries, "1710").ShouldBe(200m);
            Sum(entries, "4960").ShouldBe(-700m); // 12,500 for a book value of 11,800
            entries.Sum(e => e.Amount).ShouldBe(0m);

            var values = (await _values.GetBookValuesAsync(new GetFABookValuesInput { FANo = "FA-GAIN" })).Single();
            values.BookValue.ShouldBe(0m);
            values.DisposalDate.ShouldBe(new DateTime(2026, 2, 15));

            // A disposed asset takes no more postings.
            var batch = await BatchAsync("DEFAULT");
            await LineAsync(batch, "FA-GAIN", FAJournalPostingType.Depreciation, -10m, new DateTime(2026, 2, 28), "DEPR-X", null);
            var ex = await Should.ThrowAsync<BusinessException>(() => _batches.RunPostingAsync(batch.Id));
            ex.Code.ShouldBe(ErpErrorCodes.FixedAssets.AlreadyDisposed);
        });
    }

    [Fact]
    public async Task Disposing_Below_Book_Value_Books_A_Loss()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await AcquireAndDepreciateAsync("FA-LOSS");

            var batch = await BatchAsync("DEFAULT");
            await LineAsync(batch, "FA-LOSS", FAJournalPostingType.Disposal, 10000m, new DateTime(2026, 2, 20), "DISP-1", "1010");
            await _batches.RunPostingAsync(batch.Id);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == "DISP-1");
            Sum(entries, "1010").ShouldBe(10000m);
            Sum(entries, "8220").ShouldBe(1800m);
            Sum(entries, "1700").ShouldBe(-12000m);
            Sum(entries, "1710").ShouldBe(200m);
            entries.Sum(e => e.Amount).ShouldBe(0m);

            (await _values.GetBookValuesAsync(new GetFABookValuesInput { FANo = "FA-LOSS" })).Single().BookValue.ShouldBe(0m);
        });
    }

    // --- helpers --------------------------------------------------------------------------------

    private async Task AcquireAndDepreciateAsync(string no)
    {
        await NewAssetAsync(no, FADepreciationMethod.StraightLine, years: 5m);
        var batch = await BatchAsync("DEFAULT");
        await LineAsync(batch, no, FAJournalPostingType.AcquisitionCost, 12000m, new DateTime(2026, 1, 1), $"ACQ-{no}", "1010");
        await _batches.RunPostingAsync(batch.Id);
        var depr = await BatchAsync("DEPR");
        await _batches.CalculateDepreciationAsync(depr.Id, new CalculateDepreciationInput { UntilDate = new DateTime(2026, 1, 31), DocumentNo = $"DEPR-{no}" });
        await _batches.RunPostingAsync(depr.Id);
    }

    private async Task NewAssetAsync(string no, FADepreciationMethod method, decimal years)
    {
        var asset = new FixedAsset(Guid.NewGuid(), no);
        asset.Set(no + " asset", null, null, "TANGIBLE", null, null, null, null, null, null, default, null, false, null, null, null, false, null, false, null, false, "EQUIPMENT");
        await _assets.InsertAsync(asset, autoSave: true);

        var book = new FADepreciationBook(Guid.NewGuid(), no, "COMPANY");
        book.Set(method, null, 0m, years, years * 12m, 0m, 0m, 0m, 0m, "EQUIPMENT", null, null, 0m, false, true);
        await _assetBooks.InsertAsync(book, autoSave: true);
    }

    private async Task<FAJournalBatchDto> BatchAsync(string name) =>
        (await _batches.GetListAsync(new GetCodeTableListInput { Filter = name })).Items.First(b => b.Name == name);

    private async Task LineAsync(FAJournalBatchDto batch, string faNo, FAJournalPostingType type, decimal amount, DateTime date, string documentNo, string balAccountNo) =>
        await _lines.CreateAsync(new CreateUpdateFAJournalLineDto
        {
            FAJournalBatchId = batch.Id,
            PostingDate = date,
            DocumentNo = documentNo,
            FANo = faNo,
            FAPostingType = type,
            Amount = amount,
            BalAccountType = balAccountNo == null ? null : GenJournalAccountType.GLAccount,
            BalAccountNo = balAccountNo,
        });

    private static decimal Sum(IEnumerable<GLEntry> entries, string accountNo) =>
        entries.Where(e => e.GLAccountNo == accountNo).Sum(e => e.Amount);
}
