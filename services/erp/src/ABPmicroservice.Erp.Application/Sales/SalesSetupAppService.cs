using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Sales;

[Authorize(ErpPermissions.SalesSetup.Default)]
public class SalesSetupAppService : ErpAppService, ISalesSetupAppService
{
    private readonly SalesReceivablesSetupManager _setupManager;
    private readonly IRepository<SalesReceivablesSetup, Guid> _repository;
    private readonly NoSeriesCodeValidator _codeValidator;

    public SalesSetupAppService(
        SalesReceivablesSetupManager setupManager,
        IRepository<SalesReceivablesSetup, Guid> repository,
        NoSeriesCodeValidator codeValidator
    )
    {
        _setupManager = setupManager;
        _repository = repository;
        _codeValidator = codeValidator;
    }

    public async Task<SalesReceivablesSetupDto> GetAsync()
    {
        return ObjectMapper.Map<SalesReceivablesSetup, SalesReceivablesSetupDto>(await _setupManager.GetAsync());
    }

    [Authorize(ErpPermissions.SalesSetup.Update)]
    public async Task<SalesReceivablesSetupDto> UpdateAsync(SalesReceivablesSetupDto input)
    {
        await _codeValidator.EnsureExistAsync(
            input.CustomerNos, input.QuoteNos, input.OrderNos, input.InvoiceNos,
            input.CreditMemoNos, input.PostedInvoiceNos, input.PostedCreditMemoNos);

        var setup = await _setupManager.GetAsync();
        setup.SetNumberSeries(
            input.CustomerNos, input.QuoteNos, input.OrderNos, input.InvoiceNos,
            input.CreditMemoNos, input.PostedInvoiceNos, input.PostedCreditMemoNos);

        await _repository.UpdateAsync(setup, autoSave: true);
        return ObjectMapper.Map<SalesReceivablesSetup, SalesReceivablesSetupDto>(setup);
    }
}
