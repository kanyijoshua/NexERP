using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Integration;

/// <summary>
/// Outbound notifications to other systems, and the log of every call made.
/// Mirrors Business Central's webhook subscriptions.
/// </summary>
[Authorize(ErpPermissions.Webhooks.Default)]
public class WebhookSubscriptionAppService : ErpAppService, IWebhookSubscriptionAppService
{
    private readonly IRepository<WebhookSubscription, Guid> _subscriptionRepository;
    private readonly IRepository<WebhookDelivery, Guid> _deliveryRepository;
    private readonly ErpEntityRegistry _registry;
    private readonly WebhookDispatcher _dispatcher;

    public WebhookSubscriptionAppService(
        IRepository<WebhookSubscription, Guid> subscriptionRepository,
        IRepository<WebhookDelivery, Guid> deliveryRepository,
        ErpEntityRegistry registry,
        WebhookDispatcher dispatcher
    )
    {
        _subscriptionRepository = subscriptionRepository;
        _deliveryRepository = deliveryRepository;
        _registry = registry;
        _dispatcher = dispatcher;
    }

    public async Task<ListResultDto<WebhookSubscriptionDto>> GetListAsync()
    {
        var subscriptions = (await _subscriptionRepository.GetListAsync()).OrderBy(s => s.Name).ToList();

        return new ListResultDto<WebhookSubscriptionDto>(
            ObjectMapper.Map<List<WebhookSubscription>, List<WebhookSubscriptionDto>>(subscriptions)
        );
    }

    [Authorize(ErpPermissions.Webhooks.Manage)]
    public async Task<WebhookSubscriptionDto> CreateAsync(CreateUpdateWebhookSubscriptionDto input)
    {
        var definition = _registry.Get(input.EntityName);
        await EnsureCallerMaySubscribeAsync(definition);

        var secret = WebhookSignature.NewSecret();

        var subscription = new WebhookSubscription(
            GuidGenerator.Create(),
            input.Name,
            definition.Name,
            input.EndpointUrl,
            input.ChangeKinds,
            secret,
            input.Active
        );

        await _subscriptionRepository.InsertAsync(subscription, autoSave: true);

        var dto = ObjectMapper.Map<WebhookSubscription, WebhookSubscriptionDto>(subscription);

        // The only moment the secret is returned; it is never read back afterwards.
        dto.Secret = secret;
        return dto;
    }

    [Authorize(ErpPermissions.Webhooks.Manage)]
    public async Task<WebhookSubscriptionDto> UpdateAsync(Guid id, CreateUpdateWebhookSubscriptionDto input)
    {
        var subscription = await _subscriptionRepository.GetAsync(id);
        await EnsureCallerMaySubscribeAsync(_registry.Get(subscription.EntityName));

        subscription.Update(input.Name, input.EndpointUrl, input.ChangeKinds, input.Active);

        await _subscriptionRepository.UpdateAsync(subscription, autoSave: true);
        return ObjectMapper.Map<WebhookSubscription, WebhookSubscriptionDto>(subscription);
    }

    [Authorize(ErpPermissions.Webhooks.Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var deliveries = await _deliveryRepository.GetListAsync(d => d.SubscriptionId == id);
        if (deliveries.Count > 0)
        {
            await _deliveryRepository.DeleteManyAsync(deliveries);
        }

        await _subscriptionRepository.DeleteAsync(id);
    }

    [Authorize(ErpPermissions.Webhooks.Manage)]
    public async Task<WebhookSubscriptionDto> RegenerateSecretAsync(Guid id)
    {
        var subscription = await _subscriptionRepository.GetAsync(id);

        var secret = WebhookSignature.NewSecret();
        subscription.SetSecret(secret);

        await _subscriptionRepository.UpdateAsync(subscription, autoSave: true);

        var dto = ObjectMapper.Map<WebhookSubscription, WebhookSubscriptionDto>(subscription);
        dto.Secret = secret;
        return dto;
    }

    [Authorize(ErpPermissions.Webhooks.Manage)]
    public async Task<WebhookDeliveryDto> SendTestAsync(Guid id)
    {
        var delivery = await _dispatcher.RecordTestAsync(id);
        return ObjectMapper.Map<WebhookDelivery, WebhookDeliveryDto>(delivery);
    }

    public async Task<PagedResultDto<WebhookDeliveryDto>> GetDeliveriesAsync(GetWebhookDeliveriesInput input)
    {
        var queryable = await _deliveryRepository.GetQueryableAsync();

        if (input.SubscriptionId.HasValue)
        {
            queryable = queryable.Where(d => d.SubscriptionId == input.SubscriptionId.Value);
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(d => d.Status == input.Status.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var deliveries = await AsyncExecuter.ToListAsync(
            queryable.OrderByDescending(d => d.CreationTime).PageBy(input.SkipCount, input.MaxResultCount)
        );

        return new PagedResultDto<WebhookDeliveryDto>(
            totalCount,
            ObjectMapper.Map<List<WebhookDelivery>, List<WebhookDeliveryDto>>(deliveries)
        );
    }

    [Authorize(ErpPermissions.Webhooks.Manage)]
    public async Task<WebhookDeliveryDto> RetryDeliveryAsync(Guid id)
    {
        var delivery = await _deliveryRepository.GetAsync(id);

        delivery.Requeue(Clock.Now);

        await _deliveryRepository.UpdateAsync(delivery, autoSave: true);
        return ObjectMapper.Map<WebhookDelivery, WebhookDeliveryDto>(delivery);
    }

    /// <summary>
    /// A subscription streams a table to somewhere outside, so it needs the permission that
    /// guards reading that table — otherwise a webhook would be a way to see what you may not.
    /// </summary>
    private async Task EnsureCallerMaySubscribeAsync(ErpEntityDefinition definition)
    {
        var permission = ExportFieldMapper.PermissionOf(definition);

        if (permission == null)
        {
            throw new Volo.Abp.BusinessException(ErpErrorCodes.Exporting.UnknownEntity)
                .WithData("entityName", definition.Name);
        }

        await AuthorizationService.CheckAsync(permission);
    }
}
