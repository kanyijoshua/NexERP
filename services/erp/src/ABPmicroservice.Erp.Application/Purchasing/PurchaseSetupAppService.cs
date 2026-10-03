using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Purchasing;

[Authorize(ErpPermissions.PurchaseSetup.Default)]
public class PurchaseSetupAppService : ErpAppService, IPurchaseSetupAppService
{
    private readonly PurchasesPayablesSetupManager _setupManager;
    private readonly IRepository<PurchasesPayablesSetup, Guid> _repository;
    private readonly NoSeriesCodeValidator _codeValidator;

    public PurchaseSetupAppService(
        PurchasesPayablesSetupManager setupManager,
        IRepository<PurchasesPayablesSetup, Guid> repository,
        NoSeriesCodeValidator codeValidator
    )
    {
        _setupManager = setupManager;
        _repository = repository;
        _codeValidator = codeValidator;
    }

    public async Task<PurchasesPayablesSetupDto> GetAsync()
    {
        return ObjectMapper.Map<PurchasesPayablesSetup, PurchasesPayablesSetupDto>(await _setupManager.GetAsync());
    }

    [Authorize(ErpPermissions.PurchaseSetup.Update)]
    public async Task<PurchasesPayablesSetupDto> UpdateAsync(PurchasesPayablesSetupDto input)
    {
        await _codeValidator.EnsureExistAsync(
            input.VendorNos, input.QuoteNos, input.OrderNos, input.InvoiceNos,
            input.CreditMemoNos, input.PostedInvoiceNos, input.PostedCreditMemoNos,
            input.BlanketOrderNos, input.PostedReceiptNos, input.PostedReturnShptNos, input.ReturnOrderNos);

        var setup = await _setupManager.GetAsync();
        setup.SetNumberSeries(
            input.VendorNos, input.QuoteNos, input.OrderNos, input.InvoiceNos,
            input.CreditMemoNos, input.PostedInvoiceNos, input.PostedCreditMemoNos,
            input.BlanketOrderNos, input.PostedReceiptNos, input.PostedReturnShptNos, input.ReturnOrderNos);
        setup.SetGeneral(
            input.ExtDocNoMandatory,
            input.DiscountPosting,
            input.ReceiptOnInvoice,
            input.InvoiceRounding,
            input.CalcInvDiscount,
            input.AllowVATDifference,
            input.CalcInvDiscPerVATID,
            input.ExactCostReversingMandatory,
            input.AllowMultiplePostingGroups);
        setup.SetJobQueuePosting(input.PostWithJobQueue, input.JobQueueCategoryCode, input.NotifyOnSuccess);
        setup.SetPostingOptions(
            input.CopyCommentsBlanketToOrder,
            input.CopyCommentsOrderToInvoice,
            input.CopyCommentsOrderToReceipt,
            input.CopyCommentsRetOrderToRetShpt,
            input.CopyCommentsRetOrderToCrMemo,
            input.ReturnShipmentOnCreditMemo,
            input.CopyVendorNameToEntries,
            input.CopyLineDescrToGLEntry);

        await _repository.UpdateAsync(setup, autoSave: true);
        return ObjectMapper.Map<PurchasesPayablesSetup, PurchasesPayablesSetupDto>(setup);
    }
}
