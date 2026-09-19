using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace ABPmicroservice.Erp.WebServices;

/// <summary>
/// Published Web Service. Mirrors Business Central Table 7700 "Web Service".
/// </summary>
public class PublishedWebService : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string ObjectType { get; private set; } // "Page", "Codeunit", "Query"
    public string ServiceName { get; private set; }
    public bool Published { get; private set; }

    protected PublishedWebService() { }

    public PublishedWebService(Guid id, string objectType, string serviceName, bool published = true)
        : base(id)
    {
        ObjectType = Check.NotNullOrWhiteSpace(objectType, nameof(objectType));
        ServiceName = Check.NotNullOrWhiteSpace(serviceName, nameof(serviceName), ErpDomainConsts.MaxNameLength);
        Published = published;
    }

    public void Publish() => Published = true;
    public void Unpublish() => Published = false;
}
