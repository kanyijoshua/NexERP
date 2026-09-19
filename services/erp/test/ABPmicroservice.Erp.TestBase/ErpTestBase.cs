using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Volo.Abp.Uow;

namespace ABPmicroservice.Erp;

/* All test classes are derived from this class, directly or indirectly. */
public abstract class ErpTestBase<TStartupModule> : AbpIntegratedTest<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected const string DefaultCompanyName = "CRONUS International Ltd.";
    protected const string SecondCompanyName = "CRONUS US";

    protected ICurrentCompany CurrentCompany => GetRequiredService<ICurrentCompany>();

    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }

    protected async Task<Company> GetCompanyAsync(string name)
    {
        var companies = await WithUnitOfWorkAsync(
            () => GetRequiredService<IRepository<Company, Guid>>().GetListAsync()
        );
        return companies.Single(c => c.Name == name);
    }

    /// <summary>Runs the action in a unit of work with the named seeded company as the ambient company.</summary>
    protected async Task InCompanyAsync(string companyName, Func<Task> action)
    {
        var company = await GetCompanyAsync(companyName);
        using (CurrentCompany.Change(company.Id, company.Name))
        {
            await WithUnitOfWorkAsync(action);
        }
    }

    protected async Task<TResult> InCompanyAsync<TResult>(string companyName, Func<Task<TResult>> func)
    {
        var company = await GetCompanyAsync(companyName);
        using (CurrentCompany.Change(company.Id, company.Name))
        {
            return await WithUnitOfWorkAsync(func);
        }
    }

    protected virtual Task WithUnitOfWorkAsync(Func<Task> func)
    {
        return WithUnitOfWorkAsync(new AbpUnitOfWorkOptions(), func);
    }

    protected virtual async Task WithUnitOfWorkAsync(AbpUnitOfWorkOptions options, Func<Task> action)
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

            using (var uow = uowManager.Begin(options))
            {
                await action();

                await uow.CompleteAsync();
            }
        }
    }

    protected virtual Task<TResult> WithUnitOfWorkAsync<TResult>(Func<Task<TResult>> func)
    {
        return WithUnitOfWorkAsync(new AbpUnitOfWorkOptions(), func);
    }

    protected virtual async Task<TResult> WithUnitOfWorkAsync<TResult>(
        AbpUnitOfWorkOptions options,
        Func<Task<TResult>> func
    )
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

            using (var uow = uowManager.Begin(options))
            {
                var result = await func();
                await uow.CompleteAsync();
                return result;
            }
        }
    }
}
