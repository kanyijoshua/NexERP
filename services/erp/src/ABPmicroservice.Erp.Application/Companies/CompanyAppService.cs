using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Companies;

public class CreateCompanyDto
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public bool EvaluationCompany { get; set; }
}

public class CompanyAppService : ApplicationService
{
    private readonly IRepository<Company, Guid> _companyRepository;
    private readonly CompanyCopyEngine _companyCopyEngine;

    public CompanyAppService(
        IRepository<Company, Guid> companyRepository,
        CompanyCopyEngine companyCopyEngine
    )
    {
        _companyRepository = companyRepository;
        _companyCopyEngine = companyCopyEngine;
    }

    public async Task<List<Company>> GetListAsync()
    {
        return await _companyRepository.GetListAsync();
    }

    public async Task<Company> CreateAsync(CreateCompanyDto input)
    {
        var company = new Company(GuidGenerator.Create(), input.Name, input.DisplayName, CurrentTenant.Id, input.EvaluationCompany);
        return await _companyRepository.InsertAsync(company, autoSave: true);
    }

    public async Task<Company> CopyCompanyAsync(Guid sourceCompanyId, string newCompanyName, string newDisplayName)
    {
        return await _companyCopyEngine.CopyCompanyAsync(sourceCompanyId, newCompanyName, newDisplayName);
    }
}
