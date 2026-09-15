using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Purchasing;

public class CreatePurchaseHeaderDto
{
    public PurchaseDocumentType DocumentType { get; set; }
    public string No { get; set; }
    public Guid VendorId { get; set; }
    public string BuyFromVendorNo { get; set; }
    public string BuyFromVendorName { get; set; }
    public DateTime PostingDate { get; set; }
}

public class CreatePurchaseLineDto
{
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal DirectUnitCost { get; set; }
}

public class PurchaseDocumentAppService : ApplicationService
{
    private readonly IRepository<PurchaseHeader, Guid> _purchaseHeaderRepository;
    private readonly PurchasePostingEngine _purchasePostingEngine;

    public PurchaseDocumentAppService(
        IRepository<PurchaseHeader, Guid> purchaseHeaderRepository,
        PurchasePostingEngine purchasePostingEngine
    )
    {
        _purchaseHeaderRepository = purchaseHeaderRepository;
        _purchasePostingEngine = purchasePostingEngine;
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Default)]
    public async Task<List<PurchaseHeader>> GetListAsync()
    {
        return await _purchaseHeaderRepository.GetListAsync(includeDetails: true);
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Default)]
    public async Task<PurchaseHeader> GetAsync(Guid id)
    {
        return await _purchaseHeaderRepository.GetAsync(id, includeDetails: true);
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Create)]
    public async Task<PurchaseHeader> CreateAsync(CreatePurchaseHeaderDto input)
    {
        var header = new PurchaseHeader(
            GuidGenerator.Create(),
            input.DocumentType,
            input.No,
            input.VendorId,
            input.BuyFromVendorNo,
            input.BuyFromVendorName,
            input.PostingDate
        );
        return await _purchaseHeaderRepository.InsertAsync(header, autoSave: true);
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Update)]
    public async Task<PurchaseHeader> AddLineAsync(Guid headerId, CreatePurchaseLineDto input)
    {
        var header = await _purchaseHeaderRepository.GetAsync(headerId, includeDetails: true);
        header.AddLine(
            GuidGenerator.Create(),
            input.Type,
            input.No,
            input.Description,
            input.Quantity,
            input.DirectUnitCost
        );
        return await _purchaseHeaderRepository.UpdateAsync(header, autoSave: true);
    }

    [Authorize(ErpPermissions.PurchaseDocuments.Post)]
    public async Task<PostedPurchaseHeader> PostAsync(Guid id)
    {
        return await _purchasePostingEngine.PostAsync(id);
    }
}
