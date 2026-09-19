using System;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace ABPmicroservice.Erp.Companies;

/// <summary>
/// Marks data that belongs to one company within a tenant.
/// CompanyId is stamped on insert and filtered on read by ErpDbContext.
/// </summary>
public interface ICompanyScoped
{
    Guid CompanyId { get; }
}

/// <summary>
/// Marks posted ledger rows. They are never deleted, and only their
/// open/remaining/closed bookkeeping may change after insert.
/// </summary>
public interface ILedgerEntry
{
    long EntryNo { get; }
}

public abstract class CompanyAggregateRoot : FullAuditedAggregateRoot<Guid>, IMultiTenant, ICompanyScoped
{
    public virtual Guid? TenantId { get; protected set; }
    public virtual Guid CompanyId { get; protected set; }

    protected CompanyAggregateRoot() { }

    protected CompanyAggregateRoot(Guid id)
        : base(id) { }
}

public abstract class CompanyEntity : FullAuditedEntity<Guid>, IMultiTenant, ICompanyScoped
{
    public virtual Guid? TenantId { get; protected set; }
    public virtual Guid CompanyId { get; protected set; }

    protected CompanyEntity() { }

    protected CompanyEntity(Guid id)
        : base(id) { }
}

/// <summary>Company-scoped entity without audit columns (posted documents, dimension sets).</summary>
public abstract class CompanyBasicEntity : Entity<Guid>, IMultiTenant, ICompanyScoped
{
    public virtual Guid? TenantId { get; protected set; }
    public virtual Guid CompanyId { get; protected set; }

    protected CompanyBasicEntity() { }

    protected CompanyBasicEntity(Guid id)
        : base(id) { }
}

public abstract class LedgerEntryBase : CompanyBasicEntity, ILedgerEntry, IHasCreationTime
{
    /// <summary>Sequential entry number, assigned by the posting routine.</summary>
    public virtual long EntryNo { get; internal set; }

    public virtual DateTime CreationTime { get; protected set; }

    protected LedgerEntryBase() { }

    protected LedgerEntryBase(Guid id)
        : base(id) { }
}
