using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace ABPmicroservice.Erp.Integration;

/// <summary>What changed, as a webhook subscriber sees it.</summary>
public readonly record struct EntityChangeNotification(string EntityName, Guid EntityId, EntityChangeKind Kind);

/// <summary>
/// Collects the changes of the current unit of work so they can be turned into webhook
/// deliveries once it has committed.
/// </summary>
public interface IEntityChangeBuffer
{
    void Add(EntityChangeNotification change);
}

/// <summary>
/// Buffers changes during a unit of work and writes the deliveries after it commits.
/// <para>
/// Waiting for the commit matters: a change that is rolled back must not notify anybody, and a
/// notification written before the commit could otherwise describe data that never existed.
/// </para>
/// </summary>
public class EntityChangeBuffer : IEntityChangeBuffer, IScopedDependency
{
    private readonly List<EntityChangeNotification> _changes = new();
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EntityChangeBuffer> _logger;
    private bool _hooked;

    public EntityChangeBuffer(
        IUnitOfWorkManager unitOfWorkManager,
        IServiceProvider serviceProvider,
        ILogger<EntityChangeBuffer> logger = null
    )
    {
        _unitOfWorkManager = unitOfWorkManager;
        _serviceProvider = serviceProvider;
        _logger = logger ?? NullLogger<EntityChangeBuffer>.Instance;
    }

    public void Add(EntityChangeNotification change)
    {
        _changes.Add(change);

        var unitOfWork = _unitOfWorkManager.Current;
        if (unitOfWork == null || _hooked)
        {
            return;
        }

        _hooked = true;
        unitOfWork.OnCompleted(FlushAsync);
    }

    private async System.Threading.Tasks.Task FlushAsync()
    {
        var pending = _changes.Distinct().ToList();
        _changes.Clear();
        _hooked = false;

        if (pending.Count == 0)
        {
            return;
        }

        try
        {
            // A fresh unit of work: the one that produced these changes has already committed.
            using var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true);

            var dispatcher = _serviceProvider.GetRequiredService<WebhookDispatcher>();
            await dispatcher.RecordAsync(pending);

            await unitOfWork.CompleteAsync();
        }
        catch (Exception exception)
        {
            // A webhook that cannot be queued must not undo the business change that caused it.
            _logger.LogError(exception, "Could not queue webhook deliveries for {Count} changes.", pending.Count);
        }
    }
}
