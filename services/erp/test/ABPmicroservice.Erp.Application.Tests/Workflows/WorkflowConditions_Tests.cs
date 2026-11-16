using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Workflows;

public class WorkflowConditions_Tests : ErpApplicationTestBase
{
    private readonly IWorkflowAppService _workflows;

    public WorkflowConditions_Tests()
    {
        _workflows = GetRequiredService<IWorkflowAppService>();
    }

    [Theory]
    [InlineData(ApprovalDocumentKind.SalesDocument, "SellToCustomerNo")]
    [InlineData(ApprovalDocumentKind.PurchaseDocument, "PayToVendorNo")]
    [InlineData(ApprovalDocumentKind.PaymentVoucher, "No")]
    [InlineData(ApprovalDocumentKind.PurchaseRequisition, "No")]
    [InlineData(ApprovalDocumentKind.Tender, "No")]
    [InlineData(ApprovalDocumentKind.LeaveApplication, "EmployeeNo")]
    public async Task Every_Document_Kind_Offers_All_Its_Fields(ApprovalDocumentKind kind, string expectedField)
    {
        var fields = (await _workflows.GetConditionFieldsAsync(kind)).Items;

        fields.First().Field.ShouldBe(WorkflowConditions.AmountField);
        fields.Count.ShouldBeGreaterThan(3);
        fields.ShouldContain(f => f.Field == expectedField);
        fields.ShouldNotContain(f => f.Field == "Id" || f.Field == "CompanyId" || f.Field == "ConcurrencyStamp");
    }

    [Fact]
    public async Task Option_Fields_List_Their_Values()
    {
        var fields = (await _workflows.GetConditionFieldsAsync(ApprovalDocumentKind.PurchaseDocument)).Items;

        var status = fields.Single(f => f.Field == "Status");
        status.Type.ShouldBe(WorkflowConditionFieldType.Option);
        status.Options.ShouldContain(nameof(DocumentStatus.Open));
    }

    [Theory]
    [InlineData("Amount >= 1000", true)]
    [InlineData("Amount > 1500", false)]
    [InlineData("PayToVendorNo = V200", true)]
    [InlineData("BuyFromVendorNo = V200", false)]
    [InlineData("LocationCode <> MAIN", false)]
    [InlineData("Description contains urgent", true)]
    [InlineData("Status = Open", true)]
    [InlineData("Status = Released", false)]
    [InlineData("Imported = Yes", true)]
    [InlineData("DocumentDate < 2026-01-01", false)]
    [InlineData("DocumentDate >= 2026-01-01", true)]
    [InlineData("Lines > 2", true)]
    [InlineData("UnknownField = x", true)]
    public void Rules_Test_Any_Field_Of_The_Document(string rule, bool expected)
    {
        WorkflowConditions.Evaluate(rule, new TestDocument()).ShouldBe(expected);
    }

    private sealed class TestDocument : IApprovalDocument
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string No => "D001";
        public DocumentStatus Status => DocumentStatus.Open;
        public bool HasLines => true;
        public decimal ApprovalAmount => 1500m;
        public string BuyFromVendorNo => "V100";
        public string PayToVendorNo => "V200";
        public string LocationCode => "MAIN";
        public string Description => "An URGENT order";
        public bool Imported => true;
        public DateTime DocumentDate => new(2026, 3, 1);
        public int Lines => 3;

        public void SendForApproval() { }
        public void Release() { }
        public void Reopen() { }
    }
}
