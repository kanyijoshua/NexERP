using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Payroll;

// ---------------------------------------------------------------------------- Setup

public class PayrollSetupDto
{
    [StringLength(ErpDomainConsts.MaxNoSeriesCodeLength)]
    public string PayrollRunNos { get; set; }

    [Range(0, double.MaxValue)]
    public decimal PersonalRelief { get; set; }
}

public interface IPayrollSetupAppService : IApplicationService
{
    Task<PayrollSetupDto> GetAsync();

    Task<PayrollSetupDto> UpdateAsync(PayrollSetupDto input);
}

public class PayrollEarningDto : CodeTableDto
{
    public PayCalculationMethod CalculationMethod { get; set; }
    public decimal DefaultValue { get; set; }
    public bool BasicPay { get; set; }
    public bool Taxable { get; set; }
    public string GLAccountNo { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdatePayrollEarningDto : CreateUpdateCodeTableDto
{
    public PayCalculationMethod CalculationMethod { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DefaultValue { get; set; }

    public bool BasicPay { get; set; }
    public bool Taxable { get; set; } = true;

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string GLAccountNo { get; set; }

    public bool Blocked { get; set; }
}

public interface IPayrollEarningAppService
    : ICrudAppService<PayrollEarningDto, Guid, GetCodeTableListInput, CreateUpdatePayrollEarningDto, CreateUpdatePayrollEarningDto> { }

public class PayrollDeductionDto : CodeTableDto
{
    public PayCalculationMethod CalculationMethod { get; set; }
    public decimal DefaultValue { get; set; }
    public decimal MaximumAmount { get; set; }
    public bool TaxDeductible { get; set; }
    public decimal EmployerContributionPct { get; set; }
    public string GLAccountNo { get; set; }
    public string EmployerExpenseAccountNo { get; set; }
    public bool Statutory { get; set; }
    public bool Blocked { get; set; }
}

public class CreateUpdatePayrollDeductionDto : CreateUpdateCodeTableDto
{
    public PayCalculationMethod CalculationMethod { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DefaultValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MaximumAmount { get; set; }

    public bool TaxDeductible { get; set; }

    [Range(0, 1000)]
    public decimal EmployerContributionPct { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string GLAccountNo { get; set; }

    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string EmployerExpenseAccountNo { get; set; }

    public bool Statutory { get; set; }
    public bool Blocked { get; set; }
}

public interface IPayrollDeductionAppService
    : ICrudAppService<PayrollDeductionDto, Guid, GetCodeTableListInput, CreateUpdatePayrollDeductionDto, CreateUpdatePayrollDeductionDto> { }

public class PayrollTaxBandDto : FullAuditedEntityDto<Guid>
{
    public decimal LowerLimit { get; set; }
    public decimal UpperLimit { get; set; }
    public decimal RatePct { get; set; }
}

public class CreateUpdatePayrollTaxBandDto
{
    [Range(0, double.MaxValue)]
    public decimal LowerLimit { get; set; }

    /// <summary>0 is the top band.</summary>
    [Range(0, double.MaxValue)]
    public decimal UpperLimit { get; set; }

    [Range(0, 100)]
    public decimal RatePct { get; set; }
}

public class GetPayrollTaxBandListInput : ErpPagedListInput
{
    public string Filter { get; set; }
}

public interface IPayrollTaxBandAppService
    : ICrudAppService<PayrollTaxBandDto, Guid, GetPayrollTaxBandListInput, CreateUpdatePayrollTaxBandDto, CreateUpdatePayrollTaxBandDto> { }

// ---------------------------------------------------------------------------- Employee pay items

public class EmployeePayItemDto : FullAuditedEntityDto<Guid>
{
    public string EmployeeNo { get; set; }
    public PayItemType ItemType { get; set; }
    public string Code { get; set; }
    public decimal Amount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CreateUpdateEmployeePayItemDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNoLength)]
    public string EmployeeNo { get; set; }

    public PayItemType ItemType { get; set; }

    /// <summary>An earning or a deduction code, as the item type says.</summary>
    [Required]
    [StringLength(ErpDomainConsts.MaxCodeLength)]
    public string Code { get; set; }

    /// <summary>Zero takes the earning's or deduction's own calculation.</summary>
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetEmployeePayItemListInput : ErpPagedListInput
{
    /// <summary>Matches the employee or the code.</summary>
    public string Filter { get; set; }

    public string EmployeeNo { get; set; }
}

public interface IEmployeePayItemAppService
    : ICrudAppService<EmployeePayItemDto, Guid, GetEmployeePayItemListInput, CreateUpdateEmployeePayItemDto, CreateUpdateEmployeePayItemDto> { }

// ---------------------------------------------------------------------------- Payroll runs

public class PayrollRunDto : FullAuditedEntityDto<Guid>
{
    public string No { get; set; }
    public DateTime PayPeriod { get; set; }
    public DateTime PostingDate { get; set; }
    public string Description { get; set; }
    public PayrollRunStatus Status { get; set; }
    public DateTime? PostedDate { get; set; }
    public string PostedBy { get; set; }
    public int NoOfEmployees { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNet { get; set; }
    public decimal TotalEmployerContributions { get; set; }
    public string PaymentVoucherNo { get; set; }
}

public class CreateUpdatePayrollRunDto
{
    /// <summary>Blank takes the next number of the Payroll Setup's Payroll Run Nos.</summary>
    [StringLength(ErpDomainConsts.MaxDocumentNoLength)]
    public string No { get; set; }

    /// <summary>Any day of the month paid.</summary>
    public DateTime PayPeriod { get; set; }

    public DateTime PostingDate { get; set; }

    [StringLength(ErpDomainConsts.MaxDescriptionLength)]
    public string Description { get; set; }
}

public class GetPayrollRunListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public PayrollRunStatus? Status { get; set; }
}

public interface IPayrollRunAppService
    : ICrudAppService<PayrollRunDto, Guid, GetPayrollRunListInput, CreateUpdatePayrollRunDto, CreateUpdatePayrollRunDto>
{
    /// <summary>Works out every employee's payslip. Routed as POST /api/erp/payroll-run/{id}/calculate.</summary>
    Task<PayrollRunDto> CalculateAsync(Guid id);

    /// <summary>Routed as POST /api/erp/payroll-run/{id}/run-posting.</summary>
    Task<PayrollRunDto> RunPostingAsync(Guid id);

    /// <summary>Raises the voucher that pays the net pay. Routed as POST /api/erp/payroll-run/{id}/raise-payment-voucher.</summary>
    Task<PayrollRunDto> RaisePaymentVoucherAsync(Guid id);
}

public class PayslipDto : EntityDto<Guid>
{
    public string PayrollRunNo { get; set; }
    public DateTime PayPeriod { get; set; }
    public string EmployeeNo { get; set; }
    public string EmployeeName { get; set; }
    public string JobTitle { get; set; }
    public string BankAccountNo { get; set; }
    public decimal BasicPay { get; set; }
    public decimal GrossPay { get; set; }
    public decimal TaxablePay { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public decimal EmployerContributions { get; set; }
}

public class GetPayslipListInput : ErpPagedListInput
{
    /// <summary>Matches the run, the employee number or the employee's name.</summary>
    public string Filter { get; set; }

    public string PayrollRunNo { get; set; }
    public string EmployeeNo { get; set; }
}

public interface IPayslipAppService : IReadOnlyAppService<PayslipDto, PayslipDto, Guid, GetPayslipListInput> { }

public class PayslipLineDto : EntityDto<Guid>
{
    public string PayrollRunNo { get; set; }
    public string EmployeeNo { get; set; }
    public PayslipLineType LineType { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
}

public interface IPayslipLineAppService : IReadOnlyAppService<PayslipLineDto, PayslipLineDto, Guid, GetPayslipListInput> { }
