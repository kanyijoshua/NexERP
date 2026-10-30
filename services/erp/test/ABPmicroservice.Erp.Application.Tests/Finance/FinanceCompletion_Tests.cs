using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.CostAccounting;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Intercompany;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Vendor payment suggestion and bank files, direct debits, inventory cost adjustment and posting
/// inventory cost to the G/L, consolidation, intercompany, cost accounting, responsibility centers
/// and standard sales lines.
/// </summary>
public class FinanceCompletion_Tests : ErpApplicationTestBase
{
    private static readonly DateTime PostingDate = new(2026, 3, 1);

    private readonly IGeneralJournalAppService _journals;
    private readonly ISalesDocumentAppService _salesDocuments;
    private readonly IPurchaseDocumentAppService _purchaseDocuments;
    private readonly ICustomerAppService _customers;
    private readonly IVendorAppService _vendors;
    private readonly IRepository<GLEntry, Guid> _glEntries;
    private readonly IRepository<GenJournalLine, Guid> _journalLines;

    public FinanceCompletion_Tests()
    {
        _journals = GetRequiredService<IGeneralJournalAppService>();
        _salesDocuments = GetRequiredService<ISalesDocumentAppService>();
        _purchaseDocuments = GetRequiredService<IPurchaseDocumentAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
        _vendors = GetRequiredService<IVendorAppService>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
        _journalLines = GetRequiredService<IRepository<GenJournalLine, Guid>>();
    }

    [Fact]
    public async Task Vendor_Payments_Are_Suggested_Exported_To_A_Credit_Transfer_And_Paid()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var invoice = await PurchaseInvoiceAsync("PI-PAY-1", "VI-PAY-1", 500m);
            await GetRequiredService<IRepository<VendorBankAccount, Guid>>().InsertAsync(VendorBank("V00010", "MAIN", "DE89370400440532013000", "COBADEFFXXX"), autoSave: true);

            var batch = await BatchAsync("PAYMENTS");
            var payments = GetRequiredService<IPaymentJournalAppService>();
            var suggested = await payments.SuggestVendorPaymentsAsync(batch.Id, new SuggestVendorPaymentsInput
            {
                PostingDate = PostingDate.AddDays(60),
                LastDueDate = PostingDate.AddDays(60),
                BankAccountNo = "WWB-OPERATING",
                DocumentNo = "PMT-0001",
                VendorNos = "V00010",
            });
            suggested.LinesAdded.ShouldBe(1);
            var line = (await _journalLines.GetListAsync(l => l.GenJournalBatchId == batch.Id)).ShouldHaveSingleItem();
            line.AppliesToDocNo.ShouldBe(invoice.InvoiceNo);
            line.Amount.ShouldBe(invoice.Document.TotalAmountIncludingVat);

            // Suggesting again does not pay the same invoice twice.
            (await payments.SuggestVendorPaymentsAsync(batch.Id, new SuggestVendorPaymentsInput { PostingDate = PostingDate.AddDays(60), BankAccountNo = "WWB-OPERATING", VendorNos = "V00010" }))
                .LinesAdded.ShouldBe(0);

            // Without the vendor's bank account the payment cannot go in a file.
            (await Should.ThrowAsync<BusinessException>(() => payments.ExportAsync(batch.Id, new ExportPaymentsInput())))
                .Code.ShouldBe(ErpErrorCodes.CashManagement.PaymentBankAccountMissing);
            line.SetRecipientBankAccount("MAIN");
            await _journalLines.UpdateAsync(line, autoSave: true);

            var file = await payments.ExportAsync(batch.Id, new ExportPaymentsInput { Format = PaymentFileFormat.Iso20022CreditTransfer });
            file.TransactionCount.ShouldBe(1);
            file.Content.ShouldContain("pain.001.001.03");
            file.Content.ShouldContain("<IBAN>DE89370400440532013000</IBAN>");
            (await Should.ThrowAsync<BusinessException>(() => payments.ExportAsync(batch.Id, new ExportPaymentsInput())))
                .Code.ShouldBe(ErpErrorCodes.CashManagement.NothingToExport);

            await _journals.RunPostingAsync(batch.Id);
            var entry = (await GetRequiredService<IRepository<VendorLedgerEntry, Guid>>().GetListAsync(e => e.DocumentNo == invoice.InvoiceNo)).ShouldHaveSingleItem();
            entry.Open.ShouldBeFalse();
        });
    }

    [Fact]
    public async Task A_Direct_Debit_Collects_The_Invoices_Of_Customers_With_A_Mandate()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var invoice = await SalesInvoiceAsync("SI-DD-1", 300m);
            await GetRequiredService<IRepository<CustomerBankAccount, Guid>>().InsertAsync(CustomerBank("C00010", "DD", "GB29NWBK60161331926819", "NWBKGB2L"), autoSave: true);

            var mandates = GetRequiredService<IDirectDebitMandateAppService>();
            await mandates.CreateAsync(new CreateUpdateDirectDebitMandateDto { Code = "MANDATE-1", CustomerNo = "C00010", CustomerBankAccountCode = "DD", ValidFrom = PostingDate, Type = DirectDebitMandateType.Recurrent });

            var collections = GetRequiredService<IDirectDebitCollectionAppService>();
            var collection = await collections.MakeAsync(new MakeDirectDebitCollectionInput { FromDueDate = PostingDate, ToDueDate = PostingDate.AddDays(90), BankAccountNo = "WWB-OPERATING", CreditorIdentifier = "DE98ZZZ09999999999" });
            collection.EntryCount.ShouldBe(1);
            collection.TotalAmount.ShouldBe(invoice.Document.TotalAmountIncludingVat);

            var file = await collections.ExportAsync(collection.Id);
            file.Content.ShouldContain("pain.008.001.02");
            file.Content.ShouldContain("<SeqTp>FRST</SeqTp>");
            file.Content.ShouldContain("<MndtId>MANDATE-1</MndtId>");

            var batch = await BatchAsync("DIRDEBIT");
            (await collections.MakeReceiptLinesAsync(collection.Id, new MakeDirectDebitReceiptLinesInput { GenJournalBatchId = batch.Id, PostingDate = PostingDate.AddDays(30) })).LinesAdded.ShouldBe(1);
            await _journals.RunPostingAsync(batch.Id);

            var entry = (await GetRequiredService<IRepository<CustomerLedgerEntry, Guid>>().GetListAsync(e => e.DocumentNo == invoice.InvoiceNo)).ShouldHaveSingleItem();
            entry.Open.ShouldBeFalse();
            (await mandates.GetListAsync(new GetDirectDebitMandateListInput { CustomerNo = "C00010", MaxResultCount = 10 })).Items.Single().UsedCount.ShouldBe(1);
        });
    }

    [Fact]
    public async Task The_Cost_Adjustment_Recosts_What_Left_Before_The_Purchase_Was_Invoiced()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await NewItemAsync("ADJC-1");
            var vendor = await _vendors.GetByNoAsync("V00010");
            var order = await _purchaseDocuments.CreateAsync(new CreateUpdatePurchaseHeaderDto
            {
                DocumentType = PurchaseDocumentType.Order,
                No = "PO-ADJC-1",
                VendorId = vendor.Id,
                PostingDate = PostingDate,
                VendorInvoiceNo = "VI-ADJC-1",
                Lines = [new PurchaseLineInputDto { Type = DocumentLineType.Item, No = "ADJC-1", Description = "Cost item", Quantity = 10, DirectUnitCost = 10m, LocationCode = "MAIN" }],
            });
            await _purchaseDocuments.RunOrderPostingAsync(order.Id, new PurchaseOrderPostingInput { Receive = true, Invoice = false });
            await SalesInvoiceAsync("SI-ADJC-1", 25m, "ADJC-1", 4);

            // The invoice says the goods cost 12, not 10. Posting released the order: reopen it to change it.
            await _purchaseDocuments.ReopenAsync(order.Id);
            var received = await _purchaseDocuments.GetAsync(order.Id);
            var line = received.Lines.Single();
            await _purchaseDocuments.UpdateAsync(order.Id, new CreateUpdatePurchaseHeaderDto
            {
                DocumentType = PurchaseDocumentType.Order,
                No = order.No,
                VendorId = vendor.Id,
                PostingDate = PostingDate,
                VendorInvoiceNo = "VI-ADJC-1",
                Lines = [new PurchaseLineInputDto { Id = line.Id, Type = DocumentLineType.Item, No = "ADJC-1", Description = "Cost item", Quantity = 10, DirectUnitCost = 12m, LocationCode = "MAIN" }],
            });
            await _purchaseDocuments.RunOrderPostingAsync(order.Id, new PurchaseOrderPostingInput { Receive = false, Invoice = true });

            var sold = (await GetRequiredService<IRepository<ItemLedgerEntry, Guid>>().GetListAsync(e => e.ItemNo == "ADJC-1" && !e.Positive)).ShouldHaveSingleItem();
            Math.Round(sold.UnitCost * 4, 2).ShouldBe(40m);

            var result = await GetRequiredService<ICostAdjustmentAppService>().AdjustCostAsync(new AdjustCostInput { ItemNos = "ADJC-1", PostingDate = PostingDate.AddDays(5) });
            result.AdjustedEntries.ShouldBe(1);
            result.Amount.ShouldBe(-8m);

            sold = await GetRequiredService<IRepository<ItemLedgerEntry, Guid>>().GetAsync(sold.Id);
            sold.UnitCost.ShouldBe(12m);
            // The sale posted 40 to cost of goods sold; the adjustment adds the 8 still missing.
            var adjustment = (await _glEntries.GetListAsync(e => e.SourceNo == "ADJC-1" && e.GLAccountNo == "5000")).Sum(e => e.Amount);
            adjustment.ShouldBe(8m);

            // Nothing is left to adjust.
            (await GetRequiredService<ICostAdjustmentAppService>().AdjustCostAsync(new AdjustCostInput { ItemNos = "ADJC-1" })).AdjustedEntries.ShouldBe(0);
        });
    }

    [Fact]
    public async Task Inventory_Cost_Not_Posted_Automatically_Is_Posted_By_The_Batch()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var setups = GetRequiredService<IInventorySetupAppService>();
            var setup = await setups.GetAsync();
            setup.AutomaticCostPosting = false;
            await setups.UpdateAsync(setup);

            await NewItemAsync("PCOST-1");
            await AdjustAsync("PCOST-1", 5, 4m, "PCOST-ADJ-1");
            (await _glEntries.GetListAsync(e => e.DocumentNo == "PCOST-ADJ-1")).ShouldBeEmpty();

            var result = await GetRequiredService<ICostAdjustmentAppService>().RunInventoryCostPostingAsync(new PostInventoryCostInput { PostingDate = PostingDate });
            result.PostedValueEntries.ShouldBeGreaterThanOrEqualTo(1);
            (await _glEntries.GetListAsync(e => e.SourceNo == "PCOST-1" && e.GLAccountNo == "1400")).Sum(e => e.Amount).ShouldBe(20m);

            // Posted once only.
            (await GetRequiredService<ICostAdjustmentAppService>().RunInventoryCostPostingAsync(new PostInventoryCostInput { PostingDate = PostingDate })).PostedValueEntries.ShouldBe(0);
        });
    }

    [Fact]
    public async Task A_Consolidation_Takes_In_The_Subsidiary_General_Ledger()
    {
        var subsidiary = await GetCompanyAsync(SecondCompanyName);
        await InCompanyAsync(SecondCompanyName, async () =>
        {
            var batch = await BatchAsync("CONSOLSUB");
            await _journals.CreateLineAsync(new CreateUpdateGenJournalLineDto
            {
                GenJournalBatchId = batch.Id,
                PostingDate = PostingDate,
                DocumentNo = "SUB-1",
                AccountType = GenJournalAccountType.GLAccount,
                AccountNo = "6100",
                Description = "Subsidiary travel",
                Amount = 800m,
                BalAccountType = GenJournalAccountType.GLAccount,
                BalAccountNo = "1010",
            });
            await _journals.RunPostingAsync(batch.Id);
        });

        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var units = GetRequiredService<IBusinessUnitAppService>();
            var unit = await units.CreateAsync(new CreateUpdateBusinessUnitDto { Code = "US", Description = "US subsidiary", SubsidiaryCompanyId = subsidiary.Id, ConsolidationPct = 50m });

            // Half of what the subsidiary booked on the account (net of the VAT the account posts).
            var preview = await units.GetPreviewAsync(unit.Id, new ConsolidationPeriodInput { StartingDate = PostingDate, EndingDate = PostingDate.AddMonths(1) });
            var travel = preview.Single(l => l.SubsidiaryAccountNo == "6100");
            travel.SubsidiaryAmount.ShouldBeGreaterThan(0m);
            travel.Amount.ShouldBe(Math.Round(travel.SubsidiaryAmount / 2m, 2, MidpointRounding.AwayFromZero));

            await units.RunConsolidationAsync(new RunConsolidationInput { StartingDate = PostingDate, EndingDate = PostingDate.AddMonths(1), DocumentNo = "CONSOL-1" });
            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == "CONSOL-1");
            entries.Where(e => e.GLAccountNo == "6100").Sum(e => e.Amount).ShouldBe(travel.Amount);
            entries.Sum(e => e.Amount).ShouldBe(0m);

            // The same period is not consolidated twice.
            (await Should.ThrowAsync<BusinessException>(() => units.RunConsolidationAsync(new RunConsolidationInput { StartingDate = PostingDate, EndingDate = PostingDate.AddMonths(1), DocumentNo = "CONSOL-2" })))
                .Code.ShouldBe(ErpErrorCodes.Consolidation.PeriodAlreadyConsolidated);
        });
    }

    [Fact]
    public async Task A_Sales_Invoice_To_A_Group_Company_Becomes_Its_Purchase_Invoice()
    {
        var parent = await GetCompanyAsync(DefaultCompanyName);
        var subsidiary = await GetCompanyAsync(SecondCompanyName);
        await InCompanyAsync(SecondCompanyName, async () =>
        {
            // Only the default company has demo vendors: the parent is a vendor of its own here.
            var vendor = new Vendor(Guid.NewGuid(), "V-PARENT", "Parent company");
            vendor.SetPostingGroups("DOMESTIC", "DOMESTIC");
            vendor.SetVatBusPostingGroup("DOMESTIC");
            await GetRequiredService<IRepository<Vendor, Guid>>().InsertAsync(vendor, autoSave: true);
            await GetRequiredService<IICPartnerAppService>().CreateAsync(new CreateUpdateICPartnerDto { Code = "PARENT", Description = "Parent", PartnerCompanyId = parent.Id, VendorNo = "V-PARENT", DefaultGLAccountNo = "6100" });
        });

        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await GetRequiredService<IICPartnerAppService>().CreateAsync(new CreateUpdateICPartnerDto { Code = "US", Description = "US subsidiary", PartnerCompanyId = subsidiary.Id, CustomerNo = "C00010" });
            var invoice = await SalesInvoiceAsync("SI-IC-1", 150m, glAccountNo: "4000");

            var outbox = GetRequiredService<IICOutboxTransactionAppService>();
            var queued = (await outbox.GetListAsync(new GetICTransactionListInput { PartnerCode = "US", MaxResultCount = 10 })).Items.Single(t => t.DocumentNo == invoice.InvoiceNo);
            queued.SourceType.ShouldBe(ICSourceType.SalesInvoice);
            (await outbox.SendAllAsync()).SentCount.ShouldBe(1);
        });

        await InCompanyAsync(SecondCompanyName, async () =>
        {
            var inbox = GetRequiredService<IICInboxTransactionAppService>();
            var received = (await inbox.GetListAsync(new GetICTransactionListInput { MaxResultCount = 10 })).Items.ShouldHaveSingleItem();
            received.PartnerCode.ShouldBe("PARENT");
            received.DocumentType.ShouldBe(ICDocumentType.PurchaseInvoice);

            var accepted = await inbox.AcceptAsync(received.Id, new AcceptICInboxInput { PostingDate = PostingDate });
            var documents = await _purchaseDocuments.GetListAsync(new GetPurchaseDocumentListInput { Filter = accepted.DocumentNo, MaxResultCount = 10 });
            var purchase = await _purchaseDocuments.GetAsync(documents.Items.Single().Id);
            purchase.BuyFromVendorNo.ShouldBe("V-PARENT");
            purchase.Lines.Single().No.ShouldBe("4000"); // the revenue account exists here too
            purchase.Lines.Single().DirectUnitCost.ShouldBe(150m);
            (await Should.ThrowAsync<BusinessException>(() => inbox.AcceptAsync(received.Id, new AcceptICInboxInput()))).Code.ShouldBe(ErpErrorCodes.Intercompany.InboxTransactionHandled);
        });
    }

    [Fact]
    public async Task Cost_Accounting_Takes_The_Ledger_By_Cost_Center_And_Allocates_It()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var centers = GetRequiredService<ICostCenterAppService>();
            foreach (var code in new[] { "ADMIN", "SALES", "PROD" })
            {
                await centers.CreateAsync(new CreateUpdateCostCenterDto { Code = code, Description = code });
            }

            await GetRequiredService<ICostTypeAppService>().CreateAsync(new CreateUpdateCostTypeDto { No = "CT-TRAVEL", Name = "Travel", Type = CostTypeKind.CostType, GLAccountRange = "6100", CostCenterCode = "ADMIN" });
            var costing = GetRequiredService<ICostAccountingAppService>();
            await costing.TransferFromGeneralLedgerAsync(); // whatever the seed and other runs left

            var batch = await BatchAsync("COSTACC");
            await _journals.CreateLineAsync(new CreateUpdateGenJournalLineDto
            {
                GenJournalBatchId = batch.Id,
                PostingDate = PostingDate,
                DocumentNo = "TRAVEL-1",
                AccountType = GenJournalAccountType.GLAccount,
                AccountNo = "6100",
                Description = "Travel",
                Amount = 500m,
                BalAccountType = GenJournalAccountType.GLAccount,
                BalAccountNo = "1010",
            });
            await _journals.RunPostingAsync(batch.Id);

            var transferred = await costing.TransferFromGeneralLedgerAsync();
            transferred.EntryCount.ShouldBe(1);
            var travel = (await GetRequiredService<IRepository<CostEntry, Guid>>().GetListAsync(e => e.DocumentNo == "TRAVEL-1")).ShouldHaveSingleItem();
            travel.CostCenterCode.ShouldBe("ADMIN");

            var sources = GetRequiredService<ICostAllocationSourceAppService>();
            var source = await sources.CreateAsync(new CreateUpdateCostAllocationSourceDto { Code = "ALLOC-ADMIN", Description = "Admin to the floor", CostTypeRange = "CT-TRAVEL", CostCenterCode = "ADMIN", CreditToCostTypeNo = "CT-TRAVEL" });
            var targets = GetRequiredService<ICostAllocationTargetAppService>();
            await targets.CreateAsync(new CreateUpdateCostAllocationTargetDto { CostAllocationSourceId = source.Id, CostTypeNo = "CT-TRAVEL", CostCenterCode = "SALES", Share = 60m });
            await targets.CreateAsync(new CreateUpdateCostAllocationTargetDto { CostAllocationSourceId = source.Id, CostTypeNo = "CT-TRAVEL", CostCenterCode = "PROD", Share = 40m });

            await costing.AllocateAsync(new AllocateCostsInput { PostingDate = PostingDate, DocumentNo = "ALLOC-1" });
            var balances = await costing.GetBalancesAsync(new GetCostBalancesInput { FromDate = PostingDate, ToDate = PostingDate });
            balances.Where(b => b.CostCenterCode == "ADMIN" && b.CostTypeNo == "CT-TRAVEL").Sum(b => b.Amount).ShouldBe(0m);
            var sales = Math.Round(travel.Amount * 0.6m, 2, MidpointRounding.AwayFromZero);
            balances.Single(b => b.CostCenterCode == "SALES").Amount.ShouldBe(sales);
            balances.Single(b => b.CostCenterCode == "PROD").Amount.ShouldBe(travel.Amount - sales);
        });
    }

    [Fact]
    public async Task A_User_Works_Only_With_The_Documents_Of_Their_Responsibility_Center()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var centers = GetRequiredService<IRepository<ResponsibilityCenter, Guid>>();
            await centers.InsertAsync(new ResponsibilityCenter(Guid.NewGuid(), "EAST", "East"), autoSave: true);
            await centers.InsertAsync(new ResponsibilityCenter(Guid.NewGuid(), "WEST", "West"), autoSave: true);

            var west = await _salesDocuments.CreateAsync(await SalesHeaderAsync("SI-RC-WEST", 10m, responsibilityCenter: "WEST"));
            west.ResponsibilityCenter.ShouldBe("WEST");

            var userSetups = GetRequiredService<IRepository<UserSetup, Guid>>();
            var setup = new UserSetup(Guid.NewGuid(), "admin");
            setup.Set(null, null, false, null, null, null, null, 0, 0, false, false, null, null, null, 0, false, false, null, null, default, default, null, null, "EAST", null);
            await userSetups.InsertAsync(setup, autoSave: true);

            var east = await _salesDocuments.CreateAsync(await SalesHeaderAsync("SI-RC-EAST", 10m));
            east.ResponsibilityCenter.ShouldBe("EAST");

            var listed = await _salesDocuments.GetListAsync(new GetSalesDocumentListInput { Filter = "SI-RC", MaxResultCount = 10 });
            listed.Items.Select(d => d.No).ShouldBe(["SI-RC-EAST"]);
            (await Should.ThrowAsync<BusinessException>(() => _salesDocuments.GetAsync(west.Id))).Code.ShouldBe(ErpErrorCodes.ResponsibilityCenters.DocumentOfOtherCenter);
        });
    }

    [Fact]
    public async Task Standard_Lines_Go_On_New_Invoices_Of_The_Customer_And_On_Request()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var codes = GetRequiredService<IStandardSalesCodeAppService>();
            var code = await codes.CreateAsync(new CreateUpdateStandardSalesCodeDto { Code = "MONTHLY", Description = "Monthly fee" });
            await GetRequiredService<IStandardSalesLineAppService>().CreateAsync(new CreateUpdateStandardSalesLineDto
            {
                StandardSalesCodeId = code.Id,
                Type = DocumentLineType.GLAccount,
                No = "4300",
                Description = "Monthly service fee",
                Quantity = 1,
                UnitAmount = 75m,
            });

            // Asked for: on any document.
            var manual = await _salesDocuments.CreateAsync(await SalesHeaderAsync("SI-STD-1", 10m));
            var withLines = await _salesDocuments.InsertStandardLinesAsync(manual.Id, new InsertStandardLinesInput { Code = "MONTHLY" });
            withLines.Lines.Count.ShouldBe(2);
            withLines.Lines.Single(l => l.No == "4300").UnitPrice.ShouldBe(75m);

            // Automatic: on every new invoice of the customer.
            await GetRequiredService<IStandardCustomerSalesCodeAppService>().CreateAsync(new CreateUpdateStandardCustomerSalesCodeDto
            {
                CustomerNo = "C00010",
                Code = "MONTHLY",
                InsertOnInvoices = StandardCodeInsertMode.Automatic,
            });
            var automatic = await _salesDocuments.CreateAsync(await SalesHeaderAsync("SI-STD-2", 10m));
            automatic.Lines.Select(l => l.No).ShouldContain("4300");
        });
    }

    private async Task<SalesPostingResultDto> SalesInvoiceAsync(string no, decimal unitPrice, string itemNo = null, decimal quantity = 1, string glAccountNo = null)
    {
        var header = await SalesHeaderAsync(no, unitPrice, itemNo, quantity, glAccountNo);
        var invoice = await _salesDocuments.CreateAsync(header);
        var posted = await _salesDocuments.RunPostingAsync(invoice.Id);
        return new SalesPostingResultDto { InvoiceNo = posted.PostedDocumentNo, Document = posted };
    }

    private async Task<CreateUpdateSalesHeaderDto> SalesHeaderAsync(string no, decimal unitPrice, string itemNo = null, decimal quantity = 1, string glAccountNo = null, string responsibilityCenter = null)
    {
        var customer = await _customers.GetByNoAsync("C00010");
        return new CreateUpdateSalesHeaderDto
        {
            DocumentType = SalesDocumentType.Invoice,
            No = no,
            CustomerId = customer.Id,
            PostingDate = PostingDate,
            ResponsibilityCenter = responsibilityCenter,
            Lines =
            [
                itemNo != null
                    ? new SalesLineInputDto { Type = DocumentLineType.Item, No = itemNo, Description = itemNo, Quantity = quantity, UnitPrice = unitPrice, LocationCode = "MAIN" }
                    : new SalesLineInputDto { Type = DocumentLineType.GLAccount, No = glAccountNo ?? "4000", Description = "Services", Quantity = quantity, UnitPrice = unitPrice },
            ],
        };
    }

    private async Task<PurchasePostingResultDto> PurchaseInvoiceAsync(string no, string vendorInvoiceNo, decimal amount)
    {
        var vendor = await _vendors.GetByNoAsync("V00010");
        var invoice = await _purchaseDocuments.CreateAsync(new CreateUpdatePurchaseHeaderDto
        {
            DocumentType = PurchaseDocumentType.Invoice,
            No = no,
            VendorId = vendor.Id,
            PostingDate = PostingDate,
            VendorInvoiceNo = vendorInvoiceNo,
            Lines = [new PurchaseLineInputDto { Type = DocumentLineType.GLAccount, No = "6100", Description = "Supplies", Quantity = 1, DirectUnitCost = amount }],
        });
        var posted = await _purchaseDocuments.RunPostingAsync(invoice.Id);
        return new PurchasePostingResultDto { InvoiceNo = posted.PostedDocumentNo, Document = posted };
    }

    private async Task<GenJournalBatchDto> BatchAsync(string name) =>
        await _journals.CreateBatchAsync(new CreateGenJournalBatchDto { JournalTemplateName = "GENERAL", Name = name });

    private async Task NewItemAsync(string no)
    {
        await GetRequiredService<IItemAppService>().CreateAsync(new CreateUpdateItemDto
        {
            No = no,
            Description = no,
            Type = ItemType.Inventory,
            BaseUnitOfMeasureCode = "PCS",
            GenProdPostingGroup = "RETAIL",
            InventoryPostingGroup = "RETAIL",
            VatProdPostingGroup = "STANDARD",
            CostingMethod = CostingMethod.Fifo,
        });
    }

    private async Task AdjustAsync(string itemNo, decimal quantity, decimal unitCost, string documentNo)
    {
        var batches = GetRequiredService<IItemJournalBatchAppService>();
        var batch = (await batches.GetListAsync(new GetItemJournalBatchListInput { TemplateType = ItemJournalTemplateType.Item, MaxResultCount = 10 })).Items.First();
        await GetRequiredService<IItemJournalLineAppService>().CreateAsync(new CreateUpdateItemJournalLineDto
        {
            ItemJournalBatchId = batch.Id,
            PostingDate = PostingDate,
            EntryType = ItemLedgerEntryType.PositiveAdjmt,
            DocumentNo = documentNo,
            ItemNo = itemNo,
            LocationCode = "MAIN",
            Quantity = quantity,
            UnitCost = unitCost,
        });
        await batches.RunPostingAsync(batch.Id);
    }

    private static VendorBankAccount VendorBank(string vendorNo, string code, string iban, string swift)
    {
        var account = new VendorBankAccount(Guid.NewGuid(), vendorNo, code);
        account.Set("Vendor bank", null, null, null, null, null, null, null, null, "0532013000", null, null, null, null, null, null, iban, swift, null, null);
        return account;
    }

    private static CustomerBankAccount CustomerBank(string customerNo, string code, string iban, string swift)
    {
        var account = new CustomerBankAccount(Guid.NewGuid(), customerNo, code);
        account.Set("Customer bank", null, null, null, null, null, null, null, null, "31926819", null, null, null, null, null, null, iban, swift, null, null);
        return account;
    }
}
