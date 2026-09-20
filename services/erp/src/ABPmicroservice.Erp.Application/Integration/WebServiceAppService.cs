using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Integration;

/// <summary>
/// Which tables this company exposes to other systems.
/// Mirrors Business Central's Web Services page, where a page is invisible to OData until it is
/// published. Nothing is reachable through the integration API by default.
/// </summary>
[Authorize(ErpPermissions.WebServices.Default)]
public class WebServiceAppService : ErpAppService, IWebServiceAppService
{
    private readonly IRepository<PublishedWebService, Guid> _webServiceRepository;
    private readonly ErpEntityRegistry _registry;

    public WebServiceAppService(
        IRepository<PublishedWebService, Guid> webServiceRepository,
        ErpEntityRegistry registry
    )
    {
        _webServiceRepository = webServiceRepository;
        _registry = registry;
    }

    public async Task<ListResultDto<PublishedWebServiceDto>> GetListAsync()
    {
        var services = (await _webServiceRepository.GetListAsync()).OrderBy(s => s.ServiceName).ToList();

        return new ListResultDto<PublishedWebServiceDto>(ToDtos(services));
    }

    [Authorize(ErpPermissions.WebServices.Manage)]
    public async Task<PublishedWebServiceDto> CreateAsync(CreateUpdateWebServiceDto input)
    {
        // Only a table the registry knows can be published; anything else would be a dead endpoint.
        var definition = _registry.Get(input.EntityName);
        await EnsureServiceNameIsFreeAsync(input.ServiceName, null);

        var service = new PublishedWebService(
            GuidGenerator.Create(),
            input.ServiceName,
            definition.Name,
            input.ObjectType,
            published: false,
            input.ExcludedFields
        );

        await _webServiceRepository.InsertAsync(service, autoSave: true);
        return ToDto(service);
    }

    [Authorize(ErpPermissions.WebServices.Manage)]
    public async Task<PublishedWebServiceDto> UpdateAsync(Guid id, CreateUpdateWebServiceDto input)
    {
        var service = await _webServiceRepository.GetAsync(id);
        await EnsureServiceNameIsFreeAsync(input.ServiceName, id);

        service.Update(input.ServiceName, input.ObjectType, input.ExcludedFields);

        await _webServiceRepository.UpdateAsync(service, autoSave: true);
        return ToDto(service);
    }

    [Authorize(ErpPermissions.WebServices.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        await _webServiceRepository.DeleteAsync(id);
    }

    [Authorize(ErpPermissions.WebServices.Manage)]
    public async Task<PublishedWebServiceDto> SetPublishedAsync(SetWebServicePublishedInput input)
    {
        var service = await _webServiceRepository.GetAsync(input.Id);

        if (input.Published)
        {
            service.Publish();
        }
        else
        {
            service.Unpublish();
        }

        await _webServiceRepository.UpdateAsync(service, autoSave: true);
        return ToDto(service);
    }

    private async Task EnsureServiceNameIsFreeAsync(string serviceName, Guid? exceptId)
    {
        var clash = await _webServiceRepository.FirstOrDefaultAsync(s => s.ServiceName == serviceName);

        if (clash != null && clash.Id != exceptId)
        {
            throw new BusinessException(ErpErrorCodes.Integration.ServiceNameAlreadyExists)
                .WithData("serviceName", serviceName);
        }
    }

    private List<PublishedWebServiceDto> ToDtos(List<PublishedWebService> services)
    {
        return services.Select(ToDto).ToList();
    }

    private PublishedWebServiceDto ToDto(PublishedWebService service)
    {
        var dto = ObjectMapper.Map<PublishedWebService, PublishedWebServiceDto>(service);

        // Shown so the address can be copied straight into the calling system.
        dto.Url = $"/api/erp/integration-data/query (serviceName: {service.ServiceName})";
        return dto;
    }
}
