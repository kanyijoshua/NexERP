using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Sales;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace ABPmicroservice.Erp.Companies;

public class CompanyScoping_Tests : ErpDomainTestBase
{
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly IDataFilter _dataFilter;

    public CompanyScoping_Tests()
    {
        _customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
        _glAccountRepository = GetRequiredService<IRepository<GLAccount, Guid>>();
        _dataFilter = GetRequiredService<IDataFilter>();
    }

    [Fact]
    public async Task Each_Company_Sees_Only_Its_Own_Data()
    {
        // The seeder puts the sample customer in the default company only,
        // and a full chart of accounts in both.
        (await InCompanyAsync(DefaultCompanyName, () => _customerRepository.GetCountAsync())).ShouldBe(1);
        (await InCompanyAsync(SecondCompanyName, () => _customerRepository.GetCountAsync())).ShouldBe(0);

        (await InCompanyAsync(DefaultCompanyName, () => _glAccountRepository.GetCountAsync())).ShouldBe(7);
        (await InCompanyAsync(SecondCompanyName, () => _glAccountRepository.GetCountAsync())).ShouldBe(7);
    }

    [Fact]
    public async Task Inserts_Are_Stamped_With_The_Ambient_Company()
    {
        var us = await GetCompanyAsync(SecondCompanyName);

        await InCompanyAsync(
            SecondCompanyName,
            () => _customerRepository.InsertAsync(new Customer(Guid.NewGuid(), "C-US-1", "Contoso US"))
        );

        var inserted = await InCompanyAsync(
            SecondCompanyName,
            () => _customerRepository.SingleAsync(c => c.No == "C-US-1")
        );
        inserted.CompanyId.ShouldBe(us.Id);

        // ...and it is invisible from the other company.
        (await InCompanyAsync(DefaultCompanyName, () => _customerRepository.AnyAsync(c => c.No == "C-US-1")))
            .ShouldBeFalse();
    }

    [Fact]
    public async Task The_Same_No_Can_Exist_In_Two_Companies_But_Not_Twice_In_One()
    {
        // Runs inside a tenant: on the host side TenantId is NULL, and only PostgreSQL
        // (NULLS NOT DISTINCT) can enforce a unique index across NULLs; SQLite cannot.
        var tenantId = Guid.NewGuid();
        await GetRequiredService<IDataSeeder>().SeedAsync(new DataSeedContext(tenantId));

        using (GetRequiredService<ICurrentTenant>().Change(tenantId))
        {
            // The tenant got its own companies and sample data, separate from the host's.
            (await InCompanyAsync(DefaultCompanyName, () => _customerRepository.GetCountAsync())).ShouldBe(1);

            // "C00010" already exists in the tenant's default company.
            await InCompanyAsync(
                SecondCompanyName,
                () => _customerRepository.InsertAsync(new Customer(Guid.NewGuid(), "C00010", "Adatum US"))
            );

            await Should.ThrowAsync<DbUpdateException>(
                () =>
                    InCompanyAsync(
                        DefaultCompanyName,
                        () => _customerRepository.InsertAsync(new Customer(Guid.NewGuid(), "C00010", "Duplicate"))
                    )
            );
        }

        // The host's default company is untouched by the tenant's data.
        (await InCompanyAsync(DefaultCompanyName, () => _customerRepository.GetCountAsync())).ShouldBe(1);
    }

    [Fact]
    public async Task Without_A_Company_Reads_Are_Empty_And_Inserts_Are_Refused()
    {
        (await WithUnitOfWorkAsync(() => _glAccountRepository.GetCountAsync())).ShouldBe(0);

        var ex = await Should.ThrowAsync<BusinessException>(
            () =>
                WithUnitOfWorkAsync(
                    () => _customerRepository.InsertAsync(new Customer(Guid.NewGuid(), "C-NONE", "No company"))
                )
        );
        ex.Code.ShouldBe(ErpErrorCodes.Companies.CompanyRequired);
    }

    [Fact]
    public async Task The_Company_Filter_Can_Be_Disabled_For_Cross_Company_Work()
    {
        var all = await WithUnitOfWorkAsync(async () =>
        {
            using (_dataFilter.Disable<ICompanyScoped>())
            {
                return await _glAccountRepository.GetCountAsync();
            }
        });

        all.ShouldBe(14);
    }

    [Fact]
    public async Task Copy_Company_Copies_Setup_Into_The_New_Company_Only()
    {
        var source = await GetCompanyAsync(DefaultCompanyName);
        var engine = GetRequiredService<CompanyCopyEngine>();

        await WithUnitOfWorkAsync(() => engine.CopyCompanyAsync(source.Id, "CRONUS Copy", "CRONUS Copy"));

        (await InCompanyAsync("CRONUS Copy", () => _glAccountRepository.GetCountAsync())).ShouldBe(7);
        (await InCompanyAsync("CRONUS Copy", () => _customerRepository.GetCountAsync())).ShouldBe(0);
        (await InCompanyAsync(DefaultCompanyName, () => _glAccountRepository.GetCountAsync())).ShouldBe(7);

        var duplicate = await Should.ThrowAsync<BusinessException>(
            () => WithUnitOfWorkAsync(() => engine.CopyCompanyAsync(source.Id, "CRONUS Copy", "Again"))
        );
        duplicate.Code.ShouldBe(ErpErrorCodes.Companies.CompanyNameAlreadyExists);
    }

    [Fact]
    public async Task Resolver_Falls_Back_To_The_Default_Company_And_Rejects_Unknown_Ids()
    {
        var resolver = GetRequiredService<CompanyResolver>();
        var us = await GetCompanyAsync(SecondCompanyName);

        var fallback = await WithUnitOfWorkAsync(() => resolver.ResolveAsync(null));
        fallback.Name.ShouldBe(DefaultCompanyName);

        var requested = await WithUnitOfWorkAsync(() => resolver.ResolveAsync(us.Id));
        requested.Id.ShouldBe(us.Id);

        var ex = await Should.ThrowAsync<BusinessException>(
            () => WithUnitOfWorkAsync(() => resolver.ResolveAsync(Guid.NewGuid()))
        );
        ex.Code.ShouldBe(ErpErrorCodes.Companies.CompanyNotFound);
    }
}
