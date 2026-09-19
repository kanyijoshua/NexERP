using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Companies;

// Listing needs no ERP permission beyond being signed in: every ERP screen shows the company switcher.
[Authorize]
public class CompanyAppService : ErpAppService, ICompanyAppService
{
    private readonly IRepository<Company, Guid> _companyRepository;
    private readonly CompanyCopyEngine _companyCopyEngine;
    private readonly CompanyResolver _companyResolver;

    public CompanyAppService(
        IRepository<Company, Guid> companyRepository,
        CompanyCopyEngine companyCopyEngine,
        CompanyResolver companyResolver
    )
    {
        _companyRepository = companyRepository;
        _companyCopyEngine = companyCopyEngine;
        _companyResolver = companyResolver;
    }

    public async Task<ListResultDto<CompanyDto>> GetListAsync()
    {
        var companies = await _companyRepository.GetListAsync();

        return new ListResultDto<CompanyDto>(
            ObjectMapper.Map<List<Company>, List<CompanyDto>>(
                companies.OrderByDescending(c => c.IsDefault).ThenBy(c => c.Name).ToList()
            )
        );
    }

    [Authorize(ErpPermissions.Companies.Create)]
    public async Task<CompanyDto> CreateAsync(CreateCompanyDto input)
    {
        if (await _companyRepository.AnyAsync(c => c.Name == input.Name))
        {
            throw new BusinessException(ErpErrorCodes.Companies.CompanyNameAlreadyExists).WithData("Name", input.Name);
        }

        // The first company of a tenant becomes its default.
        var isFirst = !await _companyRepository.AnyAsync();

        var company = new Company(
            GuidGenerator.Create(),
            input.Name,
            input.DisplayName,
            CurrentTenant.Id,
            input.EvaluationCompany,
            isDefault: isFirst
        );

        await _companyRepository.InsertAsync(company, autoSave: true);
        await _companyResolver.InvalidateAsync();

        return ObjectMapper.Map<Company, CompanyDto>(company);
    }

    [Authorize(ErpPermissions.Companies.Copy)]
    public async Task<CompanyDto> CopyAsync(CopyCompanyInput input)
    {
        var company = await _companyCopyEngine.CopyCompanyAsync(
            input.SourceCompanyId,
            input.NewCompanyName,
            input.NewDisplayName.IsNullOrWhiteSpace() ? input.NewCompanyName : input.NewDisplayName
        );
        await _companyResolver.InvalidateAsync();

        return ObjectMapper.Map<Company, CompanyDto>(company);
    }

    [Authorize(ErpPermissions.Companies.Update)]
    public async Task SetAsDefaultAsync(Guid id)
    {
        var companies = await _companyRepository.GetListAsync();
        if (companies.All(c => c.Id != id))
        {
            throw new BusinessException(ErpErrorCodes.Companies.CompanyNotFound).WithData("CompanyId", id);
        }

        foreach (var company in companies.Where(c => c.IsDefault != (c.Id == id)))
        {
            company.SetDefault(company.Id == id);
            await _companyRepository.UpdateAsync(company);
        }

        await _companyResolver.InvalidateAsync();
    }
}
