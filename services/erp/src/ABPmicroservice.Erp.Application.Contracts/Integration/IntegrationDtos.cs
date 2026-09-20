using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Integration;

public class PublishedWebServiceDto : EntityDto<Guid>
{
    public string ServiceName { get; set; }

    public string EntityName { get; set; }

    public WebServiceObjectType ObjectType { get; set; }

    public bool Published { get; set; }

    public string ExcludedFields { get; set; }

    /// <summary>Path other systems call, filled in by the service so the UI can show it.</summary>
    public string Url { get; set; }
}

public class CreateUpdateWebServiceDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string ServiceName { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxEntityNameLength)]
    public string EntityName { get; set; }

    public WebServiceObjectType ObjectType { get; set; } = WebServiceObjectType.Page;

    [StringLength(ErpDomainConsts.MaxTotalingLength)]
    public string ExcludedFields { get; set; }
}

public class SetWebServicePublishedInput
{
    public Guid Id { get; set; }

    public bool Published { get; set; }
}

/// <summary>What another system asks the integration API for.</summary>
public class IntegrationQueryInput
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string ServiceName { get; set; }

    public List<string> Fields { get; set; } = new();

    public List<EntityFilterDto> Filters { get; set; } = new();

    public string OrderBy { get; set; }

    public bool Descending { get; set; }

    public int SkipCount { get; set; }

    public int MaxResultCount { get; set; } = 100;
}

public class IntegrationQueryResultDto
{
    public string ServiceName { get; set; }

    public string EntityName { get; set; }

    public long TotalCount { get; set; }

    public List<string> Fields { get; set; } = new();

    /// <summary>
    /// Each item is a field-name to value map, serialized as a plain JSON object. See the note on
    /// <see cref="Exporting.DataPreviewDto.Items"/> for why the type is <c>object</c>.
    /// </summary>
    public List<object> Items { get; set; } = new();
}

public class WebhookSubscriptionDto : EntityDto<Guid>
{
    public string Name { get; set; }

    public string EntityName { get; set; }

    public string EndpointUrl { get; set; }

    public EntityChangeKind ChangeKinds { get; set; }

    public bool Active { get; set; }

    public int FailureCount { get; set; }

    public string LastError { get; set; }

    public DateTime? LastDeliveryTime { get; set; }

    /// <summary>
    /// Only returned when the secret has just been created or regenerated. It is the one moment
    /// it can be copied; afterwards the subscription is shown without it.
    /// </summary>
    public string Secret { get; set; }
}

public class CreateUpdateWebhookSubscriptionDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string Name { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxEntityNameLength)]
    public string EntityName { get; set; }

    [Required]
    [StringLength(ErpDomainConsts.MaxUrlLength)]
    public string EndpointUrl { get; set; }

    public EntityChangeKind ChangeKinds { get; set; } = EntityChangeKind.All;

    public bool Active { get; set; } = true;
}

public class WebhookDeliveryDto : EntityDto<Guid>
{
    public Guid SubscriptionId { get; set; }

    public string EntityName { get; set; }

    public EntityChangeKind ChangeKind { get; set; }

    public Guid EntityId { get; set; }

    public WebhookDeliveryStatus Status { get; set; }

    public int AttemptCount { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? LastAttemptTime { get; set; }

    public DateTime NextAttemptTime { get; set; }

    public int? ResponseStatusCode { get; set; }

    public string Error { get; set; }

    public string Payload { get; set; }
}

public class GetWebhookDeliveriesInput : PagedAndSortedResultRequestDto
{
    public Guid? SubscriptionId { get; set; }

    public WebhookDeliveryStatus? Status { get; set; }
}

/// <summary>
/// Which tables are exposed to other systems. Mirrors Business Central's Web Services page (7700).
/// </summary>
public interface IWebServiceAppService : IApplicationService
{
    Task<ListResultDto<PublishedWebServiceDto>> GetListAsync();

    Task<PublishedWebServiceDto> CreateAsync(CreateUpdateWebServiceDto input);

    Task<PublishedWebServiceDto> UpdateAsync(Guid id, CreateUpdateWebServiceDto input);

    Task DeleteAsync(Guid id);

    /// <summary>Routed as POST /api/erp/web-service/set-published.</summary>
    Task<PublishedWebServiceDto> SetPublishedAsync(SetWebServicePublishedInput input);
}

/// <summary>
/// The read endpoint other systems call. It is the counterpart of Business Central's OData
/// endpoint: one address per published web service, with field selection, filters and paging.
/// </summary>
public interface IIntegrationDataAppService : IApplicationService
{
    /// <summary>Services published in this company. Routed as GET /api/erp/integration-data/services.</summary>
    Task<ListResultDto<PublishedWebServiceDto>> GetServicesAsync();

    /// <summary>The columns a published service returns.</summary>
    Task<ListResultDto<ExportableFieldDto>> GetFieldsAsync(string serviceName);

    /// <summary>Routed as POST /api/erp/integration-data/query.</summary>
    Task<IntegrationQueryResultDto> QueryAsync(IntegrationQueryInput input);
}

/// <summary>
/// Outbound notifications. Mirrors Business Central's webhook subscriptions and takes the place
/// of Odoo's automated actions that call another system.
/// </summary>
public interface IWebhookSubscriptionAppService : IApplicationService
{
    Task<ListResultDto<WebhookSubscriptionDto>> GetListAsync();

    Task<WebhookSubscriptionDto> CreateAsync(CreateUpdateWebhookSubscriptionDto input);

    Task<WebhookSubscriptionDto> UpdateAsync(Guid id, CreateUpdateWebhookSubscriptionDto input);

    Task DeleteAsync(Guid id);

    /// <summary>Issues a new signing secret and returns it once. Routed as POST /{id}/regenerate-secret.</summary>
    Task<WebhookSubscriptionDto> RegenerateSecretAsync(Guid id);

    /// <summary>Queues a test notification so an endpoint can be proved before it is relied on.</summary>
    Task<WebhookDeliveryDto> SendTestAsync(Guid id);

    Task<PagedResultDto<WebhookDeliveryDto>> GetDeliveriesAsync(GetWebhookDeliveriesInput input);

    /// <summary>Puts a failed or abandoned delivery back in the queue.</summary>
    Task<WebhookDeliveryDto> RetryDeliveryAsync(Guid id);
}
