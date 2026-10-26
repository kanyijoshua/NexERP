using System.Threading.Tasks;
using ABPmicroservice.Erp.Emailing;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Inventory;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// Leave types and the leave number series, prepayment accounts on every general posting setup,
/// freight and insurance item charges, and an email template for the documents sent most.
/// </summary>
public partial class ErpDataSeederContributor
{
    private async Task SeedEverydayOperationsAsync()
    {
        await EnsureSeriesAsync("LEAVE", "Leave applications", "LV0001");
        var hrSetup = await Repo<HumanResourcesSetup>().FirstOrDefaultAsync();
        if (hrSetup != null && hrSetup.LeaveApplicationNos == null)
        {
            hrSetup.SetLeaveApplicationNos("LEAVE");
            await Repo<HumanResourcesSetup>().UpdateAsync(hrSetup);
        }

        if (await IsEmptyAsync<LeaveType>())
        {
            await Repo<LeaveType>().InsertAsync(new LeaveType(NewId(), "ANNUAL", "Annual leave", 21m, 5m, LeaveAccrualFrequency.Annual, "HOLIDAY"));
            await Repo<LeaveType>().InsertAsync(new LeaveType(NewId(), "SICK", "Sick leave", 14m, 0m, LeaveAccrualFrequency.Annual, "SICK"));
            await Repo<LeaveType>().InsertAsync(new LeaveType(NewId(), "MATERNITY", "Maternity leave", 90m, 0m, LeaveAccrualFrequency.Manual, "HOLIDAY", genderRestriction: EmployeeGender.Female));
            await Repo<LeaveType>().InsertAsync(new LeaveType(NewId(), "PATERNITY", "Paternity leave", 14m, 0m, LeaveAccrualFrequency.Manual, "HOLIDAY", genderRestriction: EmployeeGender.Male));
            await Repo<LeaveType>().InsertAsync(new LeaveType(NewId(), "COMPASSION", "Compassionate leave", 5m, 0m, LeaveAccrualFrequency.Manual, "HOLIDAY"));
            await Repo<LeaveType>().InsertAsync(new LeaveType(NewId(), "STUDY", "Study leave", 10m, 0m, LeaveAccrualFrequency.Manual, "TRAINING"));
        }

        // A prepayment invoice is money taken (or paid) before the goods move: a liability on the
        // sales side and an asset on the purchase side until the final invoice deducts it.
        await EnsureAccountAsync("1550", "Purchase Prepayments", GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("2420", "Customer Prepayments", GLAccountCategory.Liabilities, IncomeBalanceType.BalanceSheet);
        foreach (var setup in await _generalPostingSetupRepository.GetListAsync(s => s.SalesPrepaymentsAccountNo == null && s.PurchPrepaymentsAccountNo == null))
        {
            setup.SetPrepaymentAccounts("2420", "1550");
            await _generalPostingSetupRepository.UpdateAsync(setup);
        }

        if (await IsEmptyAsync<ItemCharge>())
        {
            await Repo<ItemCharge>().InsertAsync(new ItemCharge(NewId(), "FREIGHT", "Freight", "RETAIL"));
            await Repo<ItemCharge>().InsertAsync(new ItemCharge(NewId(), "INSURANCE", "Insurance", "RETAIL"));
            await Repo<ItemCharge>().InsertAsync(new ItemCharge(NewId(), "DUTY", "Customs duty", "RETAIL"));
        }

        if (await IsEmptyAsync<EmailTemplate>())
        {
            await Repo<EmailTemplate>().InsertAsync(new EmailTemplate(
                NewId(),
                "SALES-INVOICE",
                "Posted sales invoice",
                EmailDocumentType.PostedSalesInvoice,
                "Invoice {{DocumentNo}} from {{CompanyName}}",
                "<p>Dear {{PartyName}},</p><p>Thank you for your business. Invoice <strong>{{DocumentNo}}</strong> dated {{PostingDate}} for <strong>{{CurrencyCode}} {{TotalAmountIncludingVat}}</strong> is due on {{DueDate}}.</p><p>Kind regards,<br/>{{CompanyName}}</p>"
            ));
            await Repo<EmailTemplate>().InsertAsync(new EmailTemplate(
                NewId(),
                "SALES-QUOTE",
                "Sales quote",
                EmailDocumentType.SalesQuote,
                "Quote {{DocumentNo}} from {{CompanyName}}",
                "<p>Dear {{PartyName}},</p><p>As requested, here is our quote <strong>{{DocumentNo}}</strong> for <strong>{{CurrencyCode}} {{TotalAmountIncludingVat}}</strong>.</p><p>Kind regards,<br/>{{CompanyName}}</p>"
            ));
            await Repo<EmailTemplate>().InsertAsync(new EmailTemplate(
                NewId(),
                "SALES-ORDER",
                "Order confirmation",
                EmailDocumentType.SalesOrder,
                "Order confirmation {{DocumentNo}}",
                "<p>Dear {{PartyName}},</p><p>We confirm your order <strong>{{DocumentNo}}</strong> for <strong>{{CurrencyCode}} {{TotalAmountIncludingVat}}</strong>.</p><p>Kind regards,<br/>{{CompanyName}}</p>"
            ));
            await Repo<EmailTemplate>().InsertAsync(new EmailTemplate(
                NewId(),
                "PURCH-ORDER",
                "Purchase order",
                EmailDocumentType.PurchaseOrder,
                "Purchase order {{DocumentNo}} from {{CompanyName}}",
                "<p>Dear {{PartyName}},</p><p>Please find our purchase order <strong>{{DocumentNo}}</strong> dated {{PostingDate}} for <strong>{{CurrencyCode}} {{TotalAmountIncludingVat}}</strong>.</p><p>Kind regards,<br/>{{CompanyName}}</p>"
            ));
        }
    }
}
