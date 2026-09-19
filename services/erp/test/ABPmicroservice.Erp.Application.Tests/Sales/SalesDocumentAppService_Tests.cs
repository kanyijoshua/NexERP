using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace ABPmicroservice.Erp.Sales;

public class SalesDocumentAppService_Tests : ErpApplicationTestBase
{
    private readonly ISalesDocumentAppService _documents;
    private readonly ICustomerAppService _customers;

    public SalesDocumentAppService_Tests()
    {
        _documents = GetRequiredService<ISalesDocumentAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
    }

    [Fact]
    public async Task Create_Takes_The_Customer_From_The_Master_And_Totals_The_Lines()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var created = await _documents.CreateAsync(await NewInvoiceAsync("SI-1001"));

            created.SellToCustomerNo.ShouldBe("C00010");
            created.SellToCustomerName.ShouldBe("Adatum Corporation");
            created.Status.ShouldBe(DocumentStatus.Open);
            created.Lines.Count.ShouldBe(2);
            created.TotalAmount.ShouldBe(2 * 300m + 1 * 50m);

            // Reading it back returns the lines too (eager loading is configured).
            (await _documents.GetAsync(created.Id)).Lines.Count.ShouldBe(2);
        });
    }

    [Fact]
    public async Task Update_Replaces_The_Lines_And_A_Released_Document_Is_Frozen()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var input = await NewInvoiceAsync("SI-1002");
            var created = await _documents.CreateAsync(input);

            input.Lines.RemoveAt(1);
            input.Lines[0].Quantity = 5;
            var updated = await _documents.UpdateAsync(created.Id, input);
            updated.Lines.Count.ShouldBe(1);
            updated.TotalAmount.ShouldBe(1500m);

            (await _documents.ReleaseAsync(created.Id)).Status.ShouldBe(DocumentStatus.Released);
            await Should.ThrowAsync<DocumentNotOpenException>(() => _documents.UpdateAsync(created.Id, input));

            (await _documents.ReopenAsync(created.Id)).Status.ShouldBe(DocumentStatus.Open);
            await _documents.UpdateAsync(created.Id, input);
        });
    }

    [Fact]
    public async Task A_Document_Without_Lines_Cannot_Be_Released()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var input = await NewInvoiceAsync("SI-1003");
            input.Lines.Clear();
            var created = await _documents.CreateAsync(input);

            var ex = await Should.ThrowAsync<BusinessException>(() => _documents.ReleaseAsync(created.Id));
            ex.Code.ShouldBe(ErpErrorCodes.Documents.DocumentHasNoLines);
        });
    }

    [Fact]
    public async Task Duplicate_Numbers_And_Blocked_Customers_Are_Refused()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var input = await NewInvoiceAsync("SI-1004");
            await _documents.CreateAsync(input);

            var duplicate = await Should.ThrowAsync<BusinessException>(() => _documents.CreateAsync(input));
            duplicate.Code.ShouldBe(ErpErrorCodes.Documents.DocumentNoAlreadyExists);

            await _customers.BlockAsync(input.CustomerId);
            input.No = "SI-1005";
            var blocked = await Should.ThrowAsync<BusinessException>(() => _documents.CreateAsync(input));
            blocked.Code.ShouldBe(ErpErrorCodes.Customers.CustomerBlocked);
            await _customers.UnblockAsync(input.CustomerId);
        });
    }

    [Fact]
    public async Task The_List_Is_Paged_Filtered_And_Company_Scoped()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _documents.CreateAsync(await NewInvoiceAsync("SI-2001"));
            await _documents.CreateAsync(await NewInvoiceAsync("SI-2002"));

            var page = await _documents.GetListAsync(
                new GetSalesDocumentListInput { Filter = "SI-200", MaxResultCount = 1 }
            );
            page.TotalCount.ShouldBe(2);
            page.Items.Count.ShouldBe(1);
        });

        var other = await InCompanyAsync(
            SecondCompanyName,
            () => _documents.GetListAsync(new GetSalesDocumentListInput { Filter = "SI-200" })
        );
        other.TotalCount.ShouldBe(0);
    }

    private async Task<CreateUpdateSalesHeaderDto> NewInvoiceAsync(string no)
    {
        var customer = await _customers.GetByNoAsync("C00010");

        return new CreateUpdateSalesHeaderDto
        {
            DocumentType = SalesDocumentType.Invoice,
            No = no,
            CustomerId = customer.Id,
            PostingDate = new DateTime(2026, 3, 1),
            Lines = new List<SalesLineInputDto>
            {
                new() { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle Assembly", Quantity = 2, UnitPrice = 300m },
                new() { Type = DocumentLineType.GLAccount, No = "4000", Description = "Delivery charge", Quantity = 1, UnitPrice = 50m },
            },
        };
    }
}
