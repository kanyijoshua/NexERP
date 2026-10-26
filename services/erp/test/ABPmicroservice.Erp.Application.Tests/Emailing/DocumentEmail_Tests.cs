using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Emailing;

/// <summary>
/// Document emails built from the seeded templates (SALES-QUOTE, SALES-ORDER, SALES-INVOICE,
/// PURCH-ORDER). The test host's email sender drops every message, so a send is always logged as sent.
/// </summary>
public class DocumentEmail_Tests : ErpApplicationTestBase
{
    private readonly IDocumentEmailAppService _emails;
    private readonly IEmailTemplateAppService _templates;
    private readonly ISalesDocumentAppService _salesDocuments;
    private readonly ICustomerAppService _customers;

    public DocumentEmail_Tests()
    {
        _emails = GetRequiredService<IDocumentEmailAppService>();
        _templates = GetRequiredService<IEmailTemplateAppService>();
        _salesDocuments = GetRequiredService<ISalesDocumentAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
    }

    [Fact]
    public async Task A_Quote_Is_Emailed_From_Its_Template_And_Logged()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var quote = await QuoteAsync("QUO-MAIL-1");

            var preview = await _emails.PreviewAsync(new PreviewDocumentEmailInput { DocumentType = EmailDocumentType.SalesQuote, DocumentId = quote.Id });
            preview.TemplateCode.ShouldBe("SALES-QUOTE");
            preview.DocumentNo.ShouldBe("QUO-MAIL-1");
            preview.Subject.ShouldStartWith("Quote QUO-MAIL-1 from ");
            preview.BodyHtml.ShouldContain("696.00");   // 2 x 300 + 16% VAT

            var log = await _emails.SendAsync(new SendDocumentEmailInput
            {
                DocumentType = EmailDocumentType.SalesQuote,
                DocumentId = quote.Id,
                RecipientEmail = "buyer@example.com",
                CcEmail = "sales@example.com",
            });
            log.Status.ShouldBe(EmailDeliveryStatus.Sent);
            log.RecipientEmail.ShouldBe("buyer@example.com");
            log.Subject.ShouldBe(preview.Subject);

            var sent = await _emails.GetListAsync(new GetSentEmailLogListInput { DocumentId = quote.Id, MaxResultCount = 10 });
            sent.Items.ShouldHaveSingleItem().CcEmail.ShouldBe("sales@example.com");
        });
    }

    [Fact]
    public async Task A_Document_Type_Has_One_Default_Template()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var created = await _templates.CreateAsync(new CreateUpdateEmailTemplateDto
            {
                Code = "quote-short",
                Name = "Short quote",
                DocumentType = EmailDocumentType.SalesQuote,
                SubjectTemplate = "{{DocumentNo}}",
                BodyHtmlTemplate = "<p>{{PartyName}}</p>",
                IsDefault = true,
            });
            created.Code.ShouldBe("QUOTE-SHORT");

            var quoteTemplates = await _templates.GetListAsync(new GetEmailTemplateListInput { DocumentType = EmailDocumentType.SalesQuote, MaxResultCount = 10 });
            quoteTemplates.Items.Where(t => t.IsDefault).ShouldHaveSingleItem().Code.ShouldBe("QUOTE-SHORT");

            var quote = await QuoteAsync("QUO-MAIL-2");
            (await _emails.PreviewAsync(new PreviewDocumentEmailInput { DocumentType = EmailDocumentType.SalesQuote, DocumentId = quote.Id })).Subject.ShouldBe("QUO-MAIL-2");

            var duplicate = await Should.ThrowAsync<BusinessException>(() => _templates.CreateAsync(new CreateUpdateEmailTemplateDto
            {
                Code = "QUOTE-SHORT",
                Name = "Again",
                DocumentType = EmailDocumentType.SalesQuote,
                SubjectTemplate = "x",
                BodyHtmlTemplate = "x",
            }));
            duplicate.Code.ShouldBe(ErpErrorCodes.Emailing.TemplateCodeAlreadyExists);
        });
    }

    [Fact]
    public void Values_Put_Into_The_Body_Are_Escaped()
    {
        var tokens = new Dictionary<string, string> { ["PartyName"] = "<script>x</script> & Co" };

        EmailTemplate.Render("<p>{{PartyName}}</p>", tokens, htmlEncode: true).ShouldBe("<p>&lt;script&gt;x&lt;/script&gt; &amp; Co</p>");
        EmailTemplate.Render("Hello {{partyname}}", tokens, htmlEncode: false).ShouldBe("Hello <script>x</script> & Co");
        DocumentEmailManager.DescribeDocumentType(EmailDocumentType.PostedSalesInvoice).ShouldBe("Posted Sales Invoice");
    }

    private async Task<SalesHeaderDto> QuoteAsync(string no)
    {
        var customer = await _customers.GetByNoAsync("C00010");
        return await _salesDocuments.CreateAsync(new CreateUpdateSalesHeaderDto
        {
            DocumentType = SalesDocumentType.Quote,
            No = no,
            CustomerId = customer.Id,
            PostingDate = new DateTime(2026, 3, 1),
            Lines = [new SalesLineInputDto { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle Assembly", Quantity = 2, UnitPrice = 300m }],
        });
    }
}
