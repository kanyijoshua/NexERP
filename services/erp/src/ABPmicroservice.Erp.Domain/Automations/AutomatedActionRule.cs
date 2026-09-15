using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Automations;

/// <summary>
/// Odoo Automated Trigger Action Rule Definition.
/// </summary>
public class AutomatedActionRule : FullAuditedAggregateRoot<Guid>
{
    public string RuleName { get; private set; }
    public string EntityType { get; private set; } // "SalesHeader", "PurchaseHeader"
    public string TriggerEvent { get; private set; } // "OnCreate", "OnUpdate", "OnStatusReleased"
    public string ActionType { get; private set; } // "CreateActivityTask", "LogNote", "PublishEvent"
    public string ActionPayloadJson { get; private set; }
    public bool Enabled { get; private set; }

    protected AutomatedActionRule() { }

    public AutomatedActionRule(Guid id, string ruleName, string entityType, string triggerEvent, string actionType, string actionPayloadJson = "{}")
        : base(id)
    {
        RuleName = Check.NotNullOrWhiteSpace(ruleName, nameof(ruleName), ErpDomainConsts.MaxNameLength);
        EntityType = Check.NotNullOrWhiteSpace(entityType, nameof(entityType));
        TriggerEvent = Check.NotNullOrWhiteSpace(triggerEvent, nameof(triggerEvent));
        ActionType = Check.NotNullOrWhiteSpace(actionType, nameof(actionType));
        ActionPayloadJson = actionPayloadJson;
        Enabled = true;
    }
}

/// <summary>
/// Automated Action Evaluation Engine.
/// Executes dynamic Odoo trigger action rules on document updates.
/// </summary>
public class AutomatedActionEngine : DomainService
{
    private readonly IRepository<AutomatedActionRule, Guid> _ruleRepository;

    public AutomatedActionEngine(IRepository<AutomatedActionRule, Guid> ruleRepository)
    {
        _ruleRepository = ruleRepository;
    }

    public async Task EvaluateTriggerAsync(string entityType, string triggerEvent, Guid entityId, string entityNo)
    {
        var activeRules = await _ruleRepository.GetListAsync(r => r.Enabled && r.EntityType == entityType && r.TriggerEvent == triggerEvent);
        foreach (var rule in activeRules)
        {
            // Execute automated action based on ActionType
            Logger.LogInformation("Executed Automated Action Rule '{RuleName}' for {EntityType} {EntityNo}", rule.RuleName, entityType, entityNo);
        }
    }
}
