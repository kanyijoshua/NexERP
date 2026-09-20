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
/// The endpoint other systems read from. It plays the part of Business Central's OData service:
/// one named service per published table, with field selection, filters, ordering and paging.
/// <para>
/// Callers use the same OAuth tokens as the rest of the API. Three things have to line up before
/// a row is returned: the service is published, the caller holds the ERP integration permission,
/// and the caller holds the permission that guards the table itself.
/// </para>
/// </summary>
[Authorize(ErpPermissions.Integration.Default)]
public class IntegrationDataAppService : ErpAppService, IIntegrationDataAppService
{
    private readonly IRepository<PublishedWebService, Guid> _webServiceRepository;
    private readonly ErpEntityRegistry _registry;
    private readonly IEntityQueryExecutor _queryExecutor;

    public IntegrationDataAppService(
        IRepository<PublishedWebService, Guid> webServiceRepository,
        ErpEntityRegistry registry,
        IEntityQueryExecutor queryExecutor
    )
    {
        _webServiceRepository = webServiceRepository;
        _registry = registry;
        _queryExecutor = queryExecutor;
    }

    public async Task<ListResultDto<PublishedWebServiceDto>> GetServicesAsync()
    {
        var services = (await _webServiceRepository.GetListAsync(s => s.Published))
            .OrderBy(s => s.ServiceName)
            .ToList();

        var dtos = ObjectMapper.Map<List<PublishedWebService>, List<PublishedWebServiceDto>>(services);

        foreach (var dto in dtos)
        {
            dto.Url = $"/api/erp/integration-data/query (serviceName: {dto.ServiceName})";
        }

        return new ListResultDto<PublishedWebServiceDto>(dtos);
    }

    public async Task<ListResultDto<ExportableFieldDto>> GetFieldsAsync(string serviceName)
    {
        var (service, definition) = await ResolveAsync(serviceName);

        var fields = ExportFieldMapper
            .ToDtos(definition)
            .Where(f => !service.IsFieldExcluded(f.Name))
            .ToList();

        return new ListResultDto<ExportableFieldDto>(fields);
    }

    public async Task<IntegrationQueryResultDto> QueryAsync(IntegrationQueryInput input)
    {
        var (service, definition) = await ResolveAsync(input.ServiceName);

        var fields = ResolveFields(service, definition, input.Fields);
        EnsureFiltersAreAllowed(service, input.Filters, input.OrderBy);

        var result = await _queryExecutor.QueryAsync(
            new EntityQueryRequest
            {
                EntityName = definition.Name,
                Fields = fields,
                Filters = ExportFieldMapper.ToFilters(input.Filters),
                OrderBy = input.OrderBy,
                Descending = input.Descending,
                SkipCount = input.SkipCount,
                MaxResultCount = Math.Clamp(input.MaxResultCount, 1, 1000),
            }
        );

        return new IntegrationQueryResultDto
        {
            ServiceName = service.ServiceName,
            EntityName = definition.Name,
            TotalCount = result.TotalCount,
            Fields = result.Fields.Select(f => f.Name).ToList(),
            Items = result.Items.Cast<object>().ToList(),
        };
    }

    /// <summary>
    /// Finds the published service and checks the caller may read the table behind it.
    /// A service that exists but is not published is reported as not published, not as missing:
    /// the caller is an authenticated integration, and the distinction is what it needs to act on.
    /// </summary>
    private async Task<(PublishedWebService Service, ErpEntityDefinition Definition)> ResolveAsync(string serviceName)
    {
        var service = await _webServiceRepository.FirstOrDefaultAsync(s => s.ServiceName == serviceName);

        if (service == null || !service.Published)
        {
            throw new BusinessException(ErpErrorCodes.Integration.EntityNotPublished)
                .WithData("serviceName", serviceName ?? string.Empty);
        }

        var definition = _registry.Get(service.EntityName);

        var permission = ExportFieldMapper.PermissionOf(definition);
        if (permission == null)
        {
            throw new BusinessException(ErpErrorCodes.Integration.EntityNotPublished)
                .WithData("serviceName", serviceName);
        }

        await AuthorizationService.CheckAsync(permission);

        return (service, definition);
    }

    private static List<string> ResolveFields(
        PublishedWebService service,
        ErpEntityDefinition definition,
        List<string> requested
    )
    {
        if (requested == null || requested.Count == 0)
        {
            return definition.DefaultFields.Where(f => !service.IsFieldExcluded(f.Name)).Select(f => f.Name).ToList();
        }

        foreach (var field in requested)
        {
            // An excluded field is refused rather than quietly dropped, so the caller is not left
            // wondering why a column it asked for never arrives.
            if (service.IsFieldExcluded(field))
            {
                throw new BusinessException(ErpErrorCodes.Exporting.UnknownField)
                    .WithData("entityName", definition.Name)
                    .WithData("fieldName", field);
            }

            definition.GetField(field);
        }

        return requested;
    }

    /// <summary>Filtering or ordering by an excluded field would leak what it hides.</summary>
    private static void EnsureFiltersAreAllowed(
        PublishedWebService service,
        IEnumerable<EntityFilterDto> filters,
        string orderBy
    )
    {
        foreach (var filter in filters ?? Enumerable.Empty<EntityFilterDto>())
        {
            if (service.IsFieldExcluded(filter.Field))
            {
                throw new BusinessException(ErpErrorCodes.Exporting.UnknownField)
                    .WithData("entityName", service.EntityName)
                    .WithData("fieldName", filter.Field);
            }
        }

        if (!orderBy.IsNullOrWhiteSpace() && service.IsFieldExcluded(orderBy))
        {
            throw new BusinessException(ErpErrorCodes.Exporting.UnknownField)
                .WithData("entityName", service.EntityName)
                .WithData("fieldName", orderBy);
        }
    }
}
