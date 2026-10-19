using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Crm;

/// <summary>
/// Leads, opportunities and their pipelines, contacts and the customers made from them. The seed
/// has a Lead pipeline (New 5%, Contacted, Qualified, Disqualified) and an Opportunity pipeline
/// (Qualification 20%, Proposal 50%, Negotiation 75%, Won, Lost), close codes PRICE (lost) and
/// BEST-OFFER (won), and customer defaults DOMESTIC with 30D terms.
/// </summary>
public class Crm_Tests : ErpApplicationTestBase
{
    private readonly IOpportunityAppService _opportunities;
    private readonly IContactAppService _contacts;
    private readonly ISalesDocumentAppService _salesDocuments;
    private readonly IRepository<Customer, Guid> _customers;

    public Crm_Tests()
    {
        _opportunities = GetRequiredService<IOpportunityAppService>();
        _contacts = GetRequiredService<IContactAppService>();
        _salesDocuments = GetRequiredService<ISalesDocumentAppService>();
        _customers = GetRequiredService<IRepository<Customer, Guid>>();
    }

    [Fact]
    public async Task A_Lead_Is_Converted_Won_And_Gets_A_Linked_Sales_Quote()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var lead = await _opportunities.CreateAsync(new CreateUpdateOpportunityDto
            {
                Kind = OpportunityKind.Lead,
                Description = "Office fit-out",
                ContactCompanyName = "Northwind Traders",
                Email = "buyer@northwind.example",
                ExpectedRevenue = 40000m,
            });
            lead.StageName.ShouldBe("New");
            lead.Probability.ShouldBe(5m);
            lead.No.ShouldNotBeNullOrWhiteSpace();

            var opportunity = await _opportunities.ConvertAsync(lead.Id);
            opportunity.Kind.ShouldBe(OpportunityKind.Opportunity);
            opportunity.StageName.ShouldBe("Qualification");
            opportunity.ContactNo.ShouldNotBeNull();

            var pipeline = await _opportunities.GetPipelineAsync(new GetPipelineInput { Pipeline = OpportunityManager.OpportunityPipeline });
            var proposal = pipeline.Stages.Single(s => s.Name == "Proposal");
            opportunity = await _opportunities.MoveAsync(opportunity.Id, new MoveOpportunityInput { StageId = proposal.StageId });
            opportunity.Probability.ShouldBe(50m);
            opportunity.WeightedRevenue.ShouldBe(20000m);

            pipeline = await _opportunities.GetPipelineAsync(new GetPipelineInput { Pipeline = OpportunityManager.OpportunityPipeline });
            pipeline.Stages.Single(s => s.Name == "Proposal").WeightedRevenue.ShouldBe(20000m);

            opportunity = await _opportunities.MakeSalesQuoteAsync(opportunity.Id);
            opportunity.SalesQuoteNo.ShouldNotBeNull();
            var quotes = await _salesDocuments.GetListAsync(new GetSalesDocumentListInput { DocumentType = SalesDocumentType.Quote, MaxResultCount = 10 });
            var quote = quotes.Items.Single(q => q.No == opportunity.SalesQuoteNo);
            quote.SellToCustomerName.ShouldBe("Northwind Traders");

            opportunity = await _opportunities.WinAsync(opportunity.Id, new CloseOpportunityInput { CloseCode = "BEST-OFFER" });
            opportunity.Status.ShouldBe(OpportunityStatus.Won);
            opportunity.StageName.ShouldBe("Won");
            opportunity.Probability.ShouldBe(100m);

            var history = await _opportunities.GetHistoryAsync(opportunity.Id);
            history.Select(h => h.Note).ShouldContain("Won");
            history.Count.ShouldBeGreaterThanOrEqualTo(5);
        });
    }

    [Fact]
    public async Task Losing_Needs_A_Lost_Close_Code_And_A_Closed_Opportunity_Does_Not_Move()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var opportunity = await _opportunities.CreateAsync(new CreateUpdateOpportunityDto { Kind = OpportunityKind.Opportunity, Description = "Fleet tracking", ExpectedRevenue = 10000m });
            var pipeline = await _opportunities.GetPipelineAsync(new GetPipelineInput { Pipeline = OpportunityManager.OpportunityPipeline });
            var lost = pipeline.Stages.Single(s => s.IsLost);

            var noCode = await Should.ThrowAsync<BusinessException>(() => _opportunities.MoveAsync(opportunity.Id, new MoveOpportunityInput { StageId = lost.StageId }));
            noCode.Code.ShouldBe(ErpErrorCodes.Crm.CloseCodeRequired);

            var wrongType = await Should.ThrowAsync<BusinessException>(() => _opportunities.LoseAsync(opportunity.Id, new CloseOpportunityInput { CloseCode = "BEST-OFFER" }));
            wrongType.Code.ShouldBe(ErpErrorCodes.Crm.CloseCodeWrongType);

            var closed = await _opportunities.MoveAsync(opportunity.Id, new MoveOpportunityInput { StageId = lost.StageId, CloseCode = "PRICE" });
            closed.Status.ShouldBe(OpportunityStatus.Lost);
            closed.Probability.ShouldBe(0m);

            var moved = await Should.ThrowAsync<BusinessException>(() =>
                _opportunities.MoveAsync(opportunity.Id, new MoveOpportunityInput { StageId = pipeline.Stages.First().StageId }));
            moved.Code.ShouldBe(ErpErrorCodes.Crm.OpportunityClosed);

            var reopened = await _opportunities.ReopenAsync(opportunity.Id);
            reopened.Status.ShouldBe(OpportunityStatus.Open);
            reopened.StageName.ShouldBe("Qualification");

            // A stage of the other pipeline is refused.
            var leadStage = (await _opportunities.GetPipelineAsync(new GetPipelineInput { Pipeline = OpportunityManager.LeadPipeline })).Stages.First();
            var other = await Should.ThrowAsync<BusinessException>(() => _opportunities.MoveAsync(opportunity.Id, new MoveOpportunityInput { StageId = leadStage.StageId }));
            other.Code.ShouldBe(ErpErrorCodes.Crm.StageNotInPipeline);
        });
    }

    [Fact]
    public async Task A_Contact_Becomes_A_Customer_With_The_Setup_Defaults_Once()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var company = await _contacts.CreateAsync(new CreateUpdateContactDto { Type = ContactType.Company, Name = "Litware Inc.", Email = "info@litware.example", City = "Nairobi" });
            var person = await _contacts.CreateAsync(new CreateUpdateContactDto { Type = ContactType.Person, Name = "Ann Beebe", CompanyNo = company.No, JobTitle = "Buyer" });
            person.CompanyName.ShouldBe("Litware Inc.");

            var made = await _contacts.MakeCustomerAsync(company.Id);
            made.CustomerNo.ShouldNotBeNull();
            var customer = await _customers.FirstAsync(c => c.No == made.CustomerNo);
            customer.Name.ShouldBe("Litware Inc.");
            customer.CustomerPostingGroup.ShouldBe("DOMESTIC");
            customer.PaymentTermsCode.ShouldBe("30D");

            // A second time gives the same customer.
            (await _contacts.MakeCustomerAsync(company.Id)).CustomerNo.ShouldBe(made.CustomerNo);
            (await _customers.GetListAsync(c => c.Name == "Litware Inc.")).Count.ShouldBe(1);
        });
    }
}
