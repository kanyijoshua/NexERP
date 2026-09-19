using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Numbering;

public class NoSeriesAppService_Tests : ErpApplicationTestBase
{
    private readonly INoSeriesAppService _noSeries;
    private readonly ISalesSetupAppService _salesSetup;
    private readonly ISalesDocumentAppService _salesDocuments;
    private readonly ICustomerAppService _customers;

    public NoSeriesAppService_Tests()
    {
        _noSeries = GetRequiredService<INoSeriesAppService>();
        _salesSetup = GetRequiredService<ISalesSetupAppService>();
        _salesDocuments = GetRequiredService<ISalesDocumentAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
    }

    [Fact]
    public async Task The_Seeded_Setup_Points_Every_Sales_Record_At_A_Series()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var setup = await _salesSetup.GetAsync();
            setup.InvoiceNos.ShouldBe("S-INV");
            setup.PostedInvoiceNos.ShouldBe("S-INV+");
            setup.CustomerNos.ShouldBe("CUST");

            var list = await _noSeries.GetListAsync(new GetNoSeriesListInput { Filter = "S-INV", MaxResultCount = 10 });
            list.Items.Select(s => s.Code).ShouldBe(new[] { "S-INV", "S-INV+" });
            list.Items[0].NextNo.ShouldBe("SI-00001");
        });
    }

    [Fact]
    public async Task A_Document_Created_Without_A_Number_Takes_The_Next_One_Of_Its_Series()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var first = await _salesDocuments.CreateAsync(await NewDocumentAsync(SalesDocumentType.Invoice, no: null));
            var second = await _salesDocuments.CreateAsync(await NewDocumentAsync(SalesDocumentType.Invoice, no: ""));
            var order = await _salesDocuments.CreateAsync(await NewDocumentAsync(SalesDocumentType.Order, no: null));

            first.No.ShouldBe("SI-00001");
            second.No.ShouldBe("SI-00002");
            order.No.ShouldBe("SO-00001"); // each document type has its own series

            // The seeded series allow manual numbers, as CRONUS does.
            (await _salesDocuments.CreateAsync(await NewDocumentAsync(SalesDocumentType.Invoice, "MANUAL-1"))).No.ShouldBe("MANUAL-1");

            (await _noSeries.GetNextNoPreviewAsync(new NextNoPreviewInput { Code = "S-INV" })).NextNo.ShouldBe("SI-00003");
        });
    }

    [Fact]
    public async Task Manual_Numbers_Are_Refused_Once_The_Series_Forbids_Them()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var series = (await _noSeries.GetListAsync(new GetNoSeriesListInput { Filter = "S-INV" })).Items.First(s => s.Code == "S-INV");
            await _noSeries.UpdateAsync(series.Id, ToInput(series, manualNos: false));

            var ex = await Should.ThrowAsync<BusinessException>(
                async () => await _salesDocuments.CreateAsync(await NewDocumentAsync(SalesDocumentType.Invoice, "TYPED-1")));
            ex.Code.ShouldBe(ErpErrorCodes.NoSeries.ManualNumbersNotAllowed);
        });
    }

    [Fact]
    public async Task With_No_Series_Set_Up_The_Number_Must_Be_Typed()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var setup = await _salesSetup.GetAsync();
            setup.InvoiceNos = null;
            await _salesSetup.UpdateAsync(setup);

            (await Should.ThrowAsync<BusinessException>(
                async () => await _salesDocuments.CreateAsync(await NewDocumentAsync(SalesDocumentType.Invoice, null)))).Code
                .ShouldBe(ErpErrorCodes.NoSeries.NumberRequired);

            (await _salesDocuments.CreateAsync(await NewDocumentAsync(SalesDocumentType.Invoice, "HAND-1"))).No.ShouldBe("HAND-1");
        });
    }

    [Fact]
    public async Task New_Customers_Are_Numbered_From_The_Customer_Series()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var a = await _customers.CreateAsync(new CreateUpdateCustomerDto { Name = "Relecloud" });
            var b = await _customers.CreateAsync(new CreateUpdateCustomerDto { Name = "Trey Research" });

            a.No.ShouldBe("C00020");
            b.No.ShouldBe("C00030"); // the CUST series increments by 10
        });
    }

    [Fact]
    public async Task Updating_A_Series_Keeps_Each_Lines_Last_No_Used()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _salesDocuments.CreateAsync(await NewDocumentAsync(SalesDocumentType.Invoice, null));
            var series = (await _noSeries.GetListAsync(new GetNoSeriesListInput { Filter = "S-INV" })).Items.First(s => s.Code == "S-INV");
            series.LastNoUsed.ShouldBe("SI-00001");

            // Extend it with next year's line and an ending number on the current one.
            var input = ToInput(series, manualNos: true);
            input.Lines[0].EndingNo = "SI-99999";
            input.Lines.Add(new NoSeriesLineInputDto { StartingDate = new DateTime(2099, 1, 1), StartingNo = "SI99-00001" });
            var updated = await _noSeries.UpdateAsync(series.Id, input);

            updated.Lines.Count.ShouldBe(2);
            updated.Lines[0].LastNoUsed.ShouldBe("SI-00001");
            updated.NextNo.ShouldBe("SI-00002");

            // A line cannot be restarted above what it already handed out.
            input = ToInput(updated, manualNos: true);
            input.Lines[0].StartingNo = "SI-50000";
            (await Should.ThrowAsync<BusinessException>(() => _noSeries.UpdateAsync(series.Id, input))).Code
                .ShouldBe(ErpErrorCodes.NoSeries.InvalidLine);
        });
    }

    [Fact]
    public async Task Setup_Refuses_A_Series_Code_That_Does_Not_Exist()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var setup = await _salesSetup.GetAsync();
            setup.QuoteNos = "NOPE";

            (await Should.ThrowAsync<BusinessException>(() => _salesSetup.UpdateAsync(setup))).Code
                .ShouldBe(ErpErrorCodes.NoSeries.NoSeriesNotFound);
        });
    }

    [Fact]
    public async Task Codes_Are_Unique_Within_A_Company()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var input = new CreateUpdateNoSeriesDto
            {
                Code = "s-inv", // normalised to upper case
                Description = "Duplicate",
                Lines = { new NoSeriesLineInputDto { StartingNo = "X1" } },
            };

            (await Should.ThrowAsync<BusinessException>(() => _noSeries.CreateAsync(input))).Code
                .ShouldBe(ErpErrorCodes.NoSeries.NoSeriesCodeAlreadyExists);
        });
    }

    private static CreateUpdateNoSeriesDto ToInput(NoSeriesDto series, bool manualNos)
    {
        return new CreateUpdateNoSeriesDto
        {
            Code = series.Code,
            Description = series.Description,
            DefaultNos = series.DefaultNos,
            ManualNos = manualNos,
            DateOrder = series.DateOrder,
            Lines = series.Lines
                .Select(l => new NoSeriesLineInputDto
                {
                    Id = l.Id,
                    StartingDate = l.StartingDate,
                    StartingNo = l.StartingNo,
                    EndingNo = l.EndingNo,
                    WarningNo = l.WarningNo,
                    IncrementByNo = l.IncrementByNo,
                })
                .ToList(),
        };
    }

    private async Task<CreateUpdateSalesHeaderDto> NewDocumentAsync(SalesDocumentType type, string no)
    {
        var customer = await _customers.GetByNoAsync("C00010");

        return new CreateUpdateSalesHeaderDto
        {
            DocumentType = type,
            No = no,
            CustomerId = customer.Id,
            PostingDate = new DateTime(2026, 3, 1),
            Lines = new List<SalesLineInputDto>
            {
                new() { Type = DocumentLineType.GLAccount, No = "4000", Description = "Consulting", Quantity = 1, UnitPrice = 100m },
            },
        };
    }
}
