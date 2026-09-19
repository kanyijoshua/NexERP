using System;
using System.Threading;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace ABPmicroservice.Erp.Companies;

/// <summary>
/// Ambient Company Context, shaped like ABP's ICurrentTenant.
/// Business Central scopes every table by company; this is the equivalent selector.
/// </summary>
public interface ICurrentCompany
{
    Guid? Id { get; }
    string Name { get; }

    /// <summary>Switches the ambient company until the returned handle is disposed.</summary>
    IDisposable Change(Guid? id, string name = null);
}

public class BasicCompanyInfo
{
    public Guid? Id { get; }
    public string Name { get; }

    public BasicCompanyInfo(Guid? id, string name = null)
    {
        Id = id;
        Name = name;
    }
}

public interface ICurrentCompanyAccessor
{
    BasicCompanyInfo Current { get; set; }
}

public class AsyncLocalCurrentCompanyAccessor : ICurrentCompanyAccessor, ISingletonDependency
{
    private readonly AsyncLocal<BasicCompanyInfo> _current = new();

    public BasicCompanyInfo Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}

public class CurrentCompany : ICurrentCompany, ITransientDependency
{
    private readonly ICurrentCompanyAccessor _accessor;

    public CurrentCompany(ICurrentCompanyAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid? Id => _accessor.Current?.Id;
    public string Name => _accessor.Current?.Name;

    public IDisposable Change(Guid? id, string name = null)
    {
        var parent = _accessor.Current;
        _accessor.Current = new BasicCompanyInfo(id, name);
        return new DisposeAction(() => _accessor.Current = parent);
    }
}
