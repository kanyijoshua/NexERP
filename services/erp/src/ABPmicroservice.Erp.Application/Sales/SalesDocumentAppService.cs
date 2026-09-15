using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Sales;

public class CreateSalesHeaderDto
{
    public SalesDocumentType DocumentType { get; set; }
    public string No { get; set; }
    public Guid CustomerId { get; set; }
    public string SellToCustomerNo { get; set; }
    public string SellToCustomerName { get; set; }
    public DateTime PostingDate { get; set; }
}

public class CreateSalesLineDto
{
    public DocumentLineType Type { get; set; }
    public string No { get; set; }
    public string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class SalesDocumentAppService : ApplicationService
{
    private readonly IRepository<SalesHeader, Guid> _salesHeaderRepository;
    private readonly SalesPostingEngine _salesPostingEngine;

    public SalesDocumentAppService(
        IRepository<SalesHeader, Guid> salesHeaderRepository,
        SalesPostingEngine salesPostingEngine
    )
    {
        _salesHeaderRepository = salesHeaderRepository;
        _salesPostingEngine = salesPostingEngine;
    }

    [Authorize(ErpPermissions.SalesDocuments.Default)]
    public async Task<List<SalesHeader>> GetListAsync()
    {
        return await _salesHeaderRepository.GetListAsync(includeDetails: true);
    }

    [Authorize(ErpPermissions.SalesDocuments.Default)]
    public async Task<SalesHeader> GetAsync(Guid id)
    {
        return await _salesHeaderRepository.GetAsync(id, includeDetails: true);
    }

    [Authorize(ErpPermissions.SalesDocuments.Create)]
    public async Task<SalesHeader> CreateAsync(CreateSalesHeaderDto input)
    {
        var header = new SalesHeader(
            GuidGenerator.Create(),
            input.DocumentType,
            input.No,
            input.CustomerId,
            input.SellToCustomerNo,
            input.SellToCustomerName,
            input.PostingDate
        );
        return await _salesHeaderRepository.InsertAsync(header, autoSave: true);
    }

    [Authorize(ErpPermissions.SalesDocuments.Update)]
    public async Task<SalesHeader> AddLineAsync(Guid headerId, CreateSalesLineDto input)
    {
        var header = await _salesHeaderRepository.GetAsync(headerId, includeDetails: true);
        header.AddLine(
            GuidGenerator.Create(),
            input.Type,
            input.No,
            input.Description,
            input.Quantity,
            input.UnitPrice
        );
        return await _salesHeaderRepository.UpdateAsync(header, autoSave: true);
    }

    [Authorize(ErpPermissions.SalesDocuments.Post)]
    public async Task<PostedSalesHeader> PostAsync(Guid id)
    {
        return await _salesPostingEngine.PostAsync(id);
    }
}
