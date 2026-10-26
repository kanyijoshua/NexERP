using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Sales;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Purchasing;

public interface IVendorAppService
    : ICrudAppService<
        VendorDto,
        Guid,
        GetVendorListInput,
        CreateUpdateVendorDto,
        CreateUpdateVendorDto
    >
{
    Task<VendorDto> GetByNoAsync(string no);

    Task BlockAsync(Guid id);

    Task UnblockAsync(Guid id);
}

public interface IPurchaseDocumentAppService
    : ICrudAppService<
        PurchaseHeaderDto,
        Guid,
        GetPurchaseDocumentListInput,
        CreateUpdatePurchaseHeaderDto,
        CreateUpdatePurchaseHeaderDto
    >
{
    Task<PurchaseHeaderDto> ReleaseAsync(Guid id);

    Task<PurchaseHeaderDto> ReopenAsync(Guid id);

    /// <summary>
    /// Posts the document: creates G/L entries, item ledger entries and
    /// updates the vendor balance.
    /// Routed as POST /api/erp/purchase-document/{id}/run-posting.
    /// </summary>
    Task<PurchaseHeaderDto> RunPostingAsync(Guid id);

    /// <summary>
    /// Asks for approval under the workflow in force: the document becomes Pending Approval and is
    /// released by the last approval. Routed as POST .../{id}/send-approval-request.
    /// </summary>
    Task<ABPmicroservice.Erp.Workflows.ApprovalRequestResultDto> SendApprovalRequestAsync(Guid id);

    /// <summary>Withdraws a pending request and reopens the document. Routed as POST .../{id}/cancel-approval-request.</summary>
    Task CancelApprovalRequestAsync(Guid id);

    /// <summary>
    /// Receives and/or invoices an order or return order, with optional quantities per line.
    /// Routed as POST .../{id}/run-order-posting.
    /// </summary>
    Task<PurchasePostingResultDto> RunOrderPostingAsync(Guid id, PurchaseOrderPostingInput input);

    /// <summary>Makes an order from a quote or blanket order. Routed as POST .../{id}/make-order.</summary>
    Task<PurchaseHeaderDto> MakeOrderAsync(Guid id, MakeOrderInput input);

    /// <summary>The vendor's posted receipt (or return shipment) lines not invoiced yet. Routed as GET .../{id}/open-receipt-lines.</summary>
    Task<List<OpenReceiptLineDto>> GetOpenReceiptLinesAsync(Guid id);

    /// <summary>Adds posted receipt lines to an invoice. Routed as POST .../{id}/collect-receipt-lines.</summary>
    Task<PurchaseHeaderDto> CollectReceiptLinesAsync(Guid id, GetReceiptLinesInput input);

    /// <summary>Appends the lines of another purchase document. Routed as POST .../{id}/copy-lines-from.</summary>
    Task<PurchaseHeaderDto> CopyLinesFromAsync(Guid id, CopyDocumentLinesInput input);

    /// <summary>Closes an order (LPO) that will not be received or invoiced any further.</summary>
    Task<PurchaseHeaderDto> CloseOrderAsync(Guid id, CloseCancelPurchaseOrderInput input);

    /// <summary>Cancels an order (LPO) before anything has been received or invoiced on it.</summary>
    Task<PurchaseHeaderDto> CancelOrderAsync(Guid id, CloseCancelPurchaseOrderInput input);

    /// <summary>Posts a Prepayment Invoice from a Purchase Order. Routed as POST .../{id}/run-prepayment-posting.</summary>
    Task<PurchasePostingResultDto> RunPrepaymentPostingAsync(Guid id);

    /// <summary>Posts a Prepayment Credit Memo from a Purchase Order. Routed as POST .../{id}/run-prepayment-credit-memo-posting.</summary>
    Task<PurchasePostingResultDto> RunPrepaymentCreditMemoPostingAsync(Guid id);
}

public interface IPostedPurchaseReceiptAppService
    : IReadOnlyAppService<PostedPurchaseReceiptDto, PostedPurchaseReceiptDto, Guid, GetPostedPurchaseDocumentListInput>
{
    /// <summary>
    /// Undoes a receipt line not invoiced yet; <paramref name="id"/> is the receipt line's id.
    /// Routed as POST .../{id}/undo-line.
    /// </summary>
    Task UndoLineAsync(Guid id);
}

public interface IPostedPurchaseInvoiceAppService
    : IReadOnlyAppService<PostedPurchaseInvoiceDto, PostedPurchaseInvoiceDto, Guid, GetPostedPurchaseDocumentListInput> { }
