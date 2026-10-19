using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Sales;

public interface ICustomerAppService
    : ICrudAppService<
        CustomerDto,
        Guid,
        GetCustomerListInput,
        CreateUpdateCustomerDto,
        CreateUpdateCustomerDto
    >
{
    Task<CustomerDto> GetByNoAsync(string no);

    Task BlockAsync(Guid id);

    Task UnblockAsync(Guid id);
}

public interface ISalesDocumentAppService
    : ICrudAppService<
        SalesHeaderDto,
        Guid,
        GetSalesDocumentListInput,
        CreateUpdateSalesHeaderDto,
        CreateUpdateSalesHeaderDto
    >
{
    Task<SalesHeaderDto> ReleaseAsync(Guid id);

    Task<SalesHeaderDto> ReopenAsync(Guid id);

    /// <summary>
    /// Posts the document: creates G/L entries, item ledger entries and
    /// updates the customer balance.
    /// Routed as POST /api/erp/sales-document/{id}/run-posting.
    /// </summary>
    Task<SalesHeaderDto> RunPostingAsync(Guid id);

    /// <summary>
    /// Asks for approval under the workflow in force: the document becomes Pending Approval and is
    /// released by the last approval. Routed as POST .../{id}/send-approval-request.
    /// </summary>
    Task<ABPmicroservice.Erp.Workflows.ApprovalRequestResultDto> SendApprovalRequestAsync(Guid id);

    /// <summary>Withdraws a pending request and reopens the document. Routed as POST .../{id}/cancel-approval-request.</summary>
    Task CancelApprovalRequestAsync(Guid id);

    /// <summary>
    /// Ships and/or invoices an order or return order, with optional quantities per line.
    /// Routed as POST .../{id}/run-order-posting.
    /// </summary>
    Task<SalesPostingResultDto> RunOrderPostingAsync(Guid id, SalesOrderPostingInput input);

    /// <summary>Makes an order from a quote or blanket order. Routed as POST .../{id}/make-order.</summary>
    Task<SalesHeaderDto> MakeOrderAsync(Guid id, MakeOrderInput input);

    /// <summary>The customer's posted shipment (or return receipt) lines not invoiced yet. Routed as GET .../{id}/open-shipment-lines.</summary>
    Task<List<OpenShipmentLineDto>> GetOpenShipmentLinesAsync(Guid id);

    /// <summary>Adds posted shipment lines to an invoice. Routed as POST .../{id}/collect-shipment-lines.</summary>
    Task<SalesHeaderDto> CollectShipmentLinesAsync(Guid id, GetShipmentLinesInput input);

    /// <summary>Appends the lines of another sales document. Routed as POST .../{id}/copy-lines-from.</summary>
    Task<SalesHeaderDto> CopyLinesFromAsync(Guid id, CopyDocumentLinesInput input);
}

public interface IPostedSalesShipmentAppService
    : IReadOnlyAppService<PostedSalesShipmentDto, PostedSalesShipmentDto, Guid, GetPostedSalesDocumentListInput>
{
    /// <summary>
    /// Undoes a shipment line not invoiced yet; <paramref name="id"/> is the shipment line's id.
    /// Routed as POST .../{id}/undo-line.
    /// </summary>
    Task UndoLineAsync(Guid id);
}

public interface IPostedSalesInvoiceAppService
    : IReadOnlyAppService<PostedSalesInvoiceDto, PostedSalesInvoiceDto, Guid, GetPostedSalesDocumentListInput> { }
