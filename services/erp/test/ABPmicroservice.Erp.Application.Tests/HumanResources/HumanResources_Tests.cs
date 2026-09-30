using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.HumanResources;

public class HumanResources_Tests : ErpApplicationTestBase
{
    private readonly IEmployeeAppService _employees;
    private readonly IEmployeeAbsenceAppService _absences;
    private readonly IQualificationAppService _qualifications;
    private readonly IHumanResourcesSetupAppService _setup;

    public HumanResources_Tests()
    {
        _employees = GetRequiredService<IEmployeeAppService>();
        _absences = GetRequiredService<IEmployeeAbsenceAppService>();
        _qualifications = GetRequiredService<IQualificationAppService>();
        _setup = GetRequiredService<IHumanResourcesSetupAppService>();
    }

    [Fact]
    public async Task A_New_Employee_Is_Numbered_From_The_Employee_Series()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            (await _setup.GetAsync()).EmployeeNos.ShouldBe("EMP");

            var employee = await _employees.CreateAsync(new CreateUpdateEmployeeDto
            {
                FirstName = "Grace",
                LastName = "Wanjiru",
                JobTitle = "Accountant",
                EmplymtContractCode = "permanent",
                EmployeePostingGroup = "EMPLOYEES",
            });

            employee.No.ShouldBe("E0010");
            employee.FullName.ShouldBe("Grace Wanjiru");
            employee.EmplymtContractCode.ShouldBe("PERMANENT");
            employee.Status.ShouldBe(EmployeeStatus.Active);

            var unknown = await Should.ThrowAsync<BusinessException>(
                () => _employees.CreateAsync(new CreateUpdateEmployeeDto { FirstName = "X", UnionCode = "NOPE" })
            );
            unknown.Code.ShouldBe(ErpErrorCodes.PostingSetup.CodeNotFound);
        });
    }

    [Fact]
    public async Task An_Absence_Takes_Its_Unit_And_Description_From_Its_Cause()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var employee = await _employees.CreateAsync(new CreateUpdateEmployeeDto { FirstName = "Brian", LastName = "Otieno" });

            var absence = await _absences.CreateAsync(new CreateUpdateEmployeeAbsenceDto
            {
                EmployeeNo = employee.No,
                FromDate = new DateTime(2026, 5, 4),
                ToDate = new DateTime(2026, 5, 8),
                CauseOfAbsenceCode = "holiday",
            });

            absence.CauseOfAbsenceCode.ShouldBe("HOLIDAY");
            absence.Description.ShouldBe("Annual leave");
            absence.UnitOfMeasureCode.ShouldBe("DAY");
            absence.Quantity.ShouldBe(5m);

            var backwards = await Should.ThrowAsync<BusinessException>(
                () => _absences.CreateAsync(new CreateUpdateEmployeeAbsenceDto
                {
                    EmployeeNo = employee.No,
                    FromDate = new DateTime(2026, 5, 8),
                    ToDate = new DateTime(2026, 5, 4),
                })
            );
            backwards.Code.ShouldBe(ErpErrorCodes.HumanResources.InvalidAbsencePeriod);
        });
    }

    [Fact]
    public async Task HR_Code_Tables_Keep_Codes_Unique_And_Upper_Case()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            (await _qualifications.CreateAsync(new CreateUpdateCodeTableDto { Code = "mba", Description = "Master of Business Administration" }))
                .Code.ShouldBe("MBA");

            var duplicate = await Should.ThrowAsync<BusinessException>(
                () => _qualifications.CreateAsync(new CreateUpdateCodeTableDto { Code = "MBA" })
            );
            duplicate.Code.ShouldBe(ErpErrorCodes.PostingSetup.CodeAlreadyExists);
        });
    }
}
