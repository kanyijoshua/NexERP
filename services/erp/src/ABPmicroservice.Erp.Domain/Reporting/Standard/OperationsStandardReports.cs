using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.FixedAssets;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Purchasing;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Reporting;

// ---------------------------------------------------------------------------- Purchasing

/// <summary>
/// Vendor - Order Summary: the purchase documents not yet
/// posted, by vendor, with what each is worth.
/// </summary>
public class VendorOrderSummaryReport : StandardReportBase
{
    private readonly IRepository<PurchaseHeader, Guid> _headers;

    public VendorOrderSummaryReport(IRepository<PurchaseHeader, Guid> headers)
    {
        _headers = headers;
    }

    public override StandardReportDefinition Definition { get; } =
        new(307, "VendorOrderSummary", "Vendor - Order Summary", StandardReportAreas.Purchasing, StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "documentType", "Document Type");
        Text(result, "no", "No.");
        Date(result, "orderDate", "Order Date");
        Date(result, "expectedReceiptDate", "Expected Receipt Date");
        Text(result, "status", "Status");
        Number(result, "amount", "Amount");
        Number(result, "amountIncludingVat", "Amount Including VAT");

        var filter = Filter(request);
        var documents = (await _headers.GetListAsync(h => !h.Posted && h.OrderDate >= request.From && h.OrderDate <= request.To))
            .Where(h => filter.Matches(h.BuyFromVendorNo ?? string.Empty))
            .ToList();

        foreach (var vendor in documents.GroupBy(h => h.BuyFromVendorNo).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            BoldRow(
                result,
                ("documentType", vendor.Key),
                ("no", vendor.First().BuyFromVendorName),
                ("amount", vendor.Sum(h => h.TotalAmount)),
                ("amountIncludingVat", vendor.Sum(h => h.TotalAmountIncludingVat))
            );

            foreach (var document in vendor.OrderBy(h => h.OrderDate).ThenBy(h => h.No, StringComparer.Ordinal))
            {
                var row = Row(
                    result,
                    ("documentType", document.DocumentType.ToString()),
                    ("no", document.No),
                    ("orderDate", document.OrderDate),
                    ("expectedReceiptDate", document.ExpectedReceiptDate),
                    ("status", document.Status.ToString()),
                    ("amount", document.TotalAmount),
                    ("amountIncludingVat", document.TotalAmountIncludingVat)
                );
                row.Indentation = 1;
            }
        }

        BoldRow(
            result,
            ("no", "Total"),
            ("amount", documents.Sum(h => h.TotalAmount)),
            ("amountIncludingVat", documents.Sum(h => h.TotalAmountIncludingVat))
        );
        return result;
    }
}

/// <summary>
/// Purchase Document - Test style register of posted purchase invoices.
/// "Purchase - Invoice" run as a list: every posted invoice of the period.
/// </summary>
public class PurchaseInvoiceRegisterReport : StandardReportBase
{
    private readonly IRepository<PostedPurchaseHeader, Guid> _headers;

    public PurchaseInvoiceRegisterReport(IRepository<PostedPurchaseHeader, Guid> headers)
    {
        _headers = headers;
    }

    public override StandardReportDefinition Definition { get; } =
        new(406, "PurchaseInvoiceRegister", "Purchase - Invoice Register", StandardReportAreas.Purchasing, StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Date(result, "postingDate", "Posting Date");
        Text(result, "vendorNo", "Buy-from Vendor No.");
        Text(result, "vendorName", "Buy-from Vendor Name");
        Date(result, "dueDate", "Due Date");
        Text(result, "currencyCode", "Currency Code");
        Number(result, "amount", "Amount");
        Number(result, "amountIncludingVat", "Amount Including VAT");

        var filter = Filter(request);
        var invoices = (await _headers.GetListAsync(h => h.PostingDate >= request.From && h.PostingDate <= request.To))
            .Where(h => filter.Matches(h.BuyFromVendorNo ?? string.Empty))
            .OrderBy(h => h.PostingDate)
            .ThenBy(h => h.No, StringComparer.Ordinal)
            .ToList();

        foreach (var invoice in invoices)
        {
            Row(
                result,
                ("no", invoice.No),
                ("postingDate", invoice.PostingDate),
                ("vendorNo", invoice.BuyFromVendorNo),
                ("vendorName", invoice.BuyFromVendorName),
                ("dueDate", invoice.DueDate),
                ("currencyCode", invoice.CurrencyCode),
                ("amount", invoice.TotalAmount),
                ("amountIncludingVat", invoice.TotalAmountIncludingVat)
            );
        }

        BoldRow(
            result,
            ("vendorName", "Total"),
            ("amount", invoices.Sum(h => h.TotalAmount)),
            ("amountIncludingVat", invoices.Sum(h => h.TotalAmountIncludingVat))
        );
        return result;
    }
}

/// <summary>Vendor/Item Purchases.by way of the item vendor catalog: who supplies what.</summary>
public class VendorItemCatalogReport : StandardReportBase
{
    private readonly IRepository<ItemVendor, Guid> _itemVendors;
    private readonly IRepository<Vendor, Guid> _vendors;

    public VendorItemCatalogReport(IRepository<ItemVendor, Guid> itemVendors, IRepository<Vendor, Guid> vendors)
    {
        _itemVendors = itemVendors;
        _vendors = vendors;
    }

    public override StandardReportDefinition Definition { get; } =
        new(320, "VendorItemCatalog", "Vendor Item Catalog", StandardReportAreas.Purchasing, StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "vendorNo", "Vendor No.");
        Text(result, "vendorName", "Name");
        Text(result, "itemNo", "Item No.");
        Text(result, "vendorItemNo", "Vendor Item No.");
        Text(result, "leadTimeCalculation", "Lead Time Calculation");

        var filter = Filter(request);
        var names = (await _vendors.GetListAsync()).ToDictionary(v => v.No.ToUpperInvariant(), v => v.Name, StringComparer.Ordinal);

        foreach (var line in (await _itemVendors.GetListAsync())
            .Where(l => filter.Matches(l.VendorNo))
            .OrderBy(l => l.VendorNo, StringComparer.Ordinal)
            .ThenBy(l => l.ItemNo, StringComparer.Ordinal))
        {
            Row(
                result,
                ("vendorNo", line.VendorNo),
                ("vendorName", names.GetValueOrDefault(line.VendorNo, string.Empty)),
                ("itemNo", line.ItemNo),
                ("vendorItemNo", line.VendorItemNo),
                ("leadTimeCalculation", line.LeadTimeCalculation)
            );
        }

        return result;
    }
}

// ---------------------------------------------------------------------------- Human resources

/// <summary>What the employee reports share: the employees the request's filter lets through.</summary>
public abstract class EmployeeReportBase : StandardReportBase
{
    protected IRepository<Employee, Guid> Employees => LazyServiceProvider.LazyGetRequiredService<IRepository<Employee, Guid>>();

    protected async Task<List<Employee>> GetEmployeesAsync(StandardReportRequest request)
    {
        var filter = Filter(request);
        return (await Employees.GetListAsync()).Where(e => filter.Matches(e.No.ToUpperInvariant())).OrderBy(e => e.No, StringComparer.Ordinal).ToList();
    }

    protected static StandardReportDefinition Define(int id, string code, string name, StandardReportParameters parameters = StandardReportParameters.NoFilter) =>
        new(id, code, name, StandardReportAreas.HumanResources, parameters);

    protected static string NameOf(Employee employee) => employee == null ? string.Empty : $"{employee.FirstName} {employee.MiddleName} {employee.LastName}".Replace("  ", " ").Trim();
}

/// <summary>Employee - List.</summary>
public class EmployeeListReport : EmployeeReportBase
{
    public override StandardReportDefinition Definition { get; } = Define(5201, "EmployeeList", "Employee - List");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Text(result, "name", "Full Name");
        Text(result, "jobTitle", "Job Title");
        Text(result, "status", "Status");
        Date(result, "employmentDate", "Employment Date");
        Text(result, "contractCode", "Emplymt. Contract Code");
        Text(result, "statisticsGroupCode", "Statistics Group Code");
        Text(result, "dimension1", "Global Dimension 1 Code");

        foreach (var employee in await GetEmployeesAsync(request))
        {
            Row(
                result,
                ("no", employee.No),
                ("name", NameOf(employee)),
                ("jobTitle", employee.JobTitle),
                ("status", employee.Status.ToString()),
                ("employmentDate", employee.EmploymentDate),
                ("contractCode", employee.EmplymtContractCode),
                ("statisticsGroupCode", employee.StatisticsGroupCode),
                ("dimension1", employee.GlobalDimension1Code)
            );
        }

        return result;
    }
}

/// <summary>Employee - Addresses.</summary>
public class EmployeeAddressesReport : EmployeeReportBase
{
    public override StandardReportDefinition Definition { get; } = Define(5207, "EmployeeAddresses", "Employee - Addresses");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Text(result, "name", "Full Name");
        Text(result, "address", "Address");
        Text(result, "address2", "Address 2");
        Text(result, "city", "City");
        Text(result, "postCode", "Post Code");
        Text(result, "county", "County");
        Text(result, "countryRegionCode", "Country/Region Code");

        foreach (var employee in await GetEmployeesAsync(request))
        {
            Row(
                result,
                ("no", employee.No),
                ("name", NameOf(employee)),
                ("address", employee.Address),
                ("address2", employee.Address2),
                ("city", employee.City),
                ("postCode", employee.PostCode),
                ("county", employee.County),
                ("countryRegionCode", employee.CountryRegionCode)
            );
        }

        return result;
    }
}

/// <summary>Employee - Phone Nos.</summary>
public class EmployeePhoneNosReport : EmployeeReportBase
{
    public override StandardReportDefinition Definition { get; } = Define(5210, "EmployeePhoneNos", "Employee - Phone Nos.");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Text(result, "name", "Full Name");
        Text(result, "phoneNo", "Phone No.");
        Text(result, "mobilePhoneNo", "Mobile Phone No.");
        Text(result, "extension", "Extension");
        Text(result, "email", "E-Mail");
        Text(result, "companyEmail", "Company E-Mail");

        foreach (var employee in await GetEmployeesAsync(request))
        {
            Row(
                result,
                ("no", employee.No),
                ("name", NameOf(employee)),
                ("phoneNo", employee.PhoneNo),
                ("mobilePhoneNo", employee.MobilePhoneNo),
                ("extension", employee.Extension),
                ("email", employee.Email),
                ("companyEmail", employee.CompanyEmail)
            );
        }

        return result;
    }
}

/// <summary>Employee - Birthdays: employees in order of the day they were born on.</summary>
public class EmployeeBirthdaysReport : EmployeeReportBase
{
    public override StandardReportDefinition Definition { get; } = Define(5209, "EmployeeBirthdays", "Employee - Birthdays");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Text(result, "name", "Full Name");
        Date(result, "birthDate", "Birth Date");
        Number(result, "age", "Age");

        var today = Clock.Now.Date;

        foreach (var employee in (await GetEmployeesAsync(request))
            .Where(e => e.BirthDate.HasValue)
            .OrderBy(e => e.BirthDate.Value.Month)
            .ThenBy(e => e.BirthDate.Value.Day))
        {
            var born = employee.BirthDate.Value;
            var age = today.Year - born.Year - (today < born.AddYears(today.Year - born.Year) ? 1 : 0);

            Row(result, ("no", employee.No), ("name", NameOf(employee)), ("birthDate", born), ("age", (decimal)age));
        }

        return result;
    }
}

/// <summary>Employee - Unions: the members of each union.</summary>
public class EmployeeUnionsReport : EmployeeReportBase
{
    public override StandardReportDefinition Definition { get; } = Define(5212, "EmployeeUnions", "Employee - Unions");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "unionCode", "Union Code");
        Text(result, "no", "No.");
        Text(result, "name", "Full Name");
        Text(result, "membershipNo", "Union Membership No.");

        foreach (var union in (await GetEmployeesAsync(request)).Where(e => e.UnionCode != null).GroupBy(e => e.UnionCode).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            BoldRow(result, ("unionCode", union.Key), ("name", $"{union.Count()} member(s)"));
            foreach (var employee in union)
            {
                Row(result, ("unionCode", string.Empty), ("no", employee.No), ("name", NameOf(employee)), ("membershipNo", employee.UnionMembershipNo)).Indentation = 1;
            }
        }

        return result;
    }
}

/// <summary>Employee - Contracts: the employees on each employment contract.</summary>
public class EmployeeContractsReport : EmployeeReportBase
{
    public override StandardReportDefinition Definition { get; } = Define(5213, "EmployeeContracts", "Employee - Contracts");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "contractCode", "Emplymt. Contract Code");
        Text(result, "no", "No.");
        Text(result, "name", "Full Name");
        Date(result, "employmentDate", "Employment Date");

        foreach (var contract in (await GetEmployeesAsync(request)).Where(e => e.EmplymtContractCode != null).GroupBy(e => e.EmplymtContractCode).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            BoldRow(result, ("contractCode", contract.Key), ("name", $"{contract.Count()} employee(s)"));
            foreach (var employee in contract)
            {
                Row(result, ("contractCode", string.Empty), ("no", employee.No), ("name", NameOf(employee)), ("employmentDate", employee.EmploymentDate)).Indentation = 1;
            }
        }

        return result;
    }
}

/// <summary>Employee - Absences by Causes: the absences of the period under each cause.</summary>
public class EmployeeAbsencesByCausesReport : EmployeeReportBase
{
    private readonly IRepository<EmployeeAbsence, Guid> _absences;

    public EmployeeAbsencesByCausesReport(IRepository<EmployeeAbsence, Guid> absences)
    {
        _absences = absences;
    }

    public override StandardReportDefinition Definition { get; } =
        Define(5205, "EmployeeAbsencesByCauses", "Employee - Absences by Causes", StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "cause", "Cause of Absence Code");
        Text(result, "employeeNo", "Employee No.");
        Text(result, "name", "Full Name");
        Date(result, "fromDate", "From Date");
        Date(result, "toDate", "To Date");
        Text(result, "description", "Description");
        Number(result, "quantity", "Quantity");
        Text(result, "unit", "Unit of Measure Code");

        var employees = (await GetEmployeesAsync(request)).ToDictionary(e => e.No, StringComparer.Ordinal);
        var absences = (await _absences.GetListAsync(a => a.FromDate >= request.From && a.FromDate <= request.To))
            .Where(a => employees.ContainsKey(a.EmployeeNo))
            .ToList();

        foreach (var cause in absences.GroupBy(a => a.CauseOfAbsenceCode ?? string.Empty).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            BoldRow(result, ("cause", cause.Key), ("quantity", cause.Sum(a => a.Quantity)));

            foreach (var absence in cause.OrderBy(a => a.EmployeeNo, StringComparer.Ordinal).ThenBy(a => a.FromDate))
            {
                Row(
                    result,
                    ("cause", string.Empty),
                    ("employeeNo", absence.EmployeeNo),
                    ("name", NameOf(employees[absence.EmployeeNo])),
                    ("fromDate", absence.FromDate),
                    ("toDate", absence.ToDate),
                    ("description", absence.Description),
                    ("quantity", absence.Quantity),
                    ("unit", absence.UnitOfMeasureCode)
                ).Indentation = 1;
            }
        }

        return result;
    }
}

/// <summary>Employee - Staff Absences: each employee's absences in the period.</summary>
public class EmployeeStaffAbsencesReport : EmployeeReportBase
{
    private readonly IRepository<EmployeeAbsence, Guid> _absences;

    public EmployeeStaffAbsencesReport(IRepository<EmployeeAbsence, Guid> absences)
    {
        _absences = absences;
    }

    public override StandardReportDefinition Definition { get; } =
        Define(5204, "EmployeeStaffAbsences", "Employee - Staff Absences", StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "employeeNo", "Employee No.");
        Text(result, "name", "Full Name");
        Date(result, "fromDate", "From Date");
        Date(result, "toDate", "To Date");
        Text(result, "cause", "Cause of Absence Code");
        Text(result, "description", "Description");
        Number(result, "quantity", "Quantity");

        var absences = (await _absences.GetListAsync(a => a.FromDate >= request.From && a.FromDate <= request.To)).ToLookup(a => a.EmployeeNo, StringComparer.Ordinal);

        foreach (var employee in await GetEmployeesAsync(request))
        {
            if (!absences[employee.No].Any())
            {
                continue;
            }

            BoldRow(result, ("employeeNo", employee.No), ("name", NameOf(employee)), ("quantity", absences[employee.No].Sum(a => a.Quantity)));

            foreach (var absence in absences[employee.No].OrderBy(a => a.FromDate))
            {
                Row(
                    result,
                    ("fromDate", absence.FromDate),
                    ("toDate", absence.ToDate),
                    ("cause", absence.CauseOfAbsenceCode),
                    ("description", absence.Description),
                    ("quantity", absence.Quantity)
                ).Indentation = 1;
            }
        }

        return result;
    }
}

/// <summary>Employee - Qualifications.</summary>
public class EmployeeQualificationsReport : EmployeeReportBase
{
    private readonly IRepository<EmployeeQualification, Guid> _qualifications;

    public EmployeeQualificationsReport(IRepository<EmployeeQualification, Guid> qualifications)
    {
        _qualifications = qualifications;
    }

    public override StandardReportDefinition Definition { get; } = Define(5206, "EmployeeQualifications", "Employee - Qualifications");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "employeeNo", "Employee No.");
        Text(result, "name", "Full Name");
        Text(result, "qualificationCode", "Qualification Code");
        Text(result, "description", "Description");
        Text(result, "institution", "Institution/Company");
        Date(result, "fromDate", "From Date");
        Date(result, "toDate", "To Date");
        Date(result, "expirationDate", "Expiration Date");

        var lines = (await _qualifications.GetListAsync()).ToLookup(q => q.EmployeeNo, StringComparer.Ordinal);

        foreach (var employee in await GetEmployeesAsync(request))
        {
            foreach (var line in lines[employee.No.ToUpperInvariant()].OrderBy(q => q.LineNo))
            {
                Row(
                    result,
                    ("employeeNo", employee.No),
                    ("name", NameOf(employee)),
                    ("qualificationCode", line.QualificationCode),
                    ("description", line.Description),
                    ("institution", line.InstitutionCompany),
                    ("fromDate", line.FromDate),
                    ("toDate", line.ToDate),
                    ("expirationDate", line.ExpirationDate)
                );
            }
        }

        return result;
    }
}

/// <summary>Employee - Relatives.</summary>
public class EmployeeRelativesReport : EmployeeReportBase
{
    private readonly IRepository<EmployeeRelative, Guid> _relatives;

    public EmployeeRelativesReport(IRepository<EmployeeRelative, Guid> relatives)
    {
        _relatives = relatives;
    }

    public override StandardReportDefinition Definition { get; } = Define(5208, "EmployeeRelatives", "Employee - Relatives");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "employeeNo", "Employee No.");
        Text(result, "name", "Full Name");
        Text(result, "relativeCode", "Relative Code");
        Text(result, "relativeName", "Relative's Name");
        Date(result, "birthDate", "Birth Date");
        Text(result, "phoneNo", "Phone No.");

        var lines = (await _relatives.GetListAsync()).ToLookup(r => r.EmployeeNo, StringComparer.Ordinal);

        foreach (var employee in await GetEmployeesAsync(request))
        {
            foreach (var line in lines[employee.No.ToUpperInvariant()].OrderBy(r => r.LineNo))
            {
                Row(
                    result,
                    ("employeeNo", employee.No),
                    ("name", NameOf(employee)),
                    ("relativeCode", line.RelativeCode),
                    ("relativeName", $"{line.FirstName} {line.MiddleName} {line.LastName}".Replace("  ", " ").Trim()),
                    ("birthDate", line.BirthDate),
                    ("phoneNo", line.PhoneNo)
                );
            }
        }

        return result;
    }
}

/// <summary>Employee - Misc. Article Info: what the company has issued to each employee.</summary>
public class EmployeeMiscArticleInfoReport : EmployeeReportBase
{
    private readonly IRepository<MiscArticleInformation, Guid> _articles;

    public EmployeeMiscArticleInfoReport(IRepository<MiscArticleInformation, Guid> articles)
    {
        _articles = articles;
    }

    public override StandardReportDefinition Definition { get; } = Define(5202, "EmployeeMiscArticleInfo", "Employee - Misc. Article Info.");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "employeeNo", "Employee No.");
        Text(result, "name", "Full Name");
        Text(result, "articleCode", "Misc. Article Code");
        Text(result, "description", "Description");
        Text(result, "serialNo", "Serial No.");
        Date(result, "fromDate", "From Date");
        Date(result, "toDate", "To Date");
        Text(result, "inUse", "In Use");

        var lines = (await _articles.GetListAsync()).ToLookup(a => a.EmployeeNo, StringComparer.Ordinal);

        foreach (var employee in await GetEmployeesAsync(request))
        {
            foreach (var line in lines[employee.No.ToUpperInvariant()].OrderBy(a => a.MiscArticleCode, StringComparer.Ordinal).ThenBy(a => a.LineNo))
            {
                Row(
                    result,
                    ("employeeNo", employee.No),
                    ("name", NameOf(employee)),
                    ("articleCode", line.MiscArticleCode),
                    ("description", line.Description),
                    ("serialNo", line.SerialNo),
                    ("fromDate", line.FromDate),
                    ("toDate", line.ToDate),
                    ("inUse", line.InUse ? "Yes" : string.Empty)
                );
            }
        }

        return result;
    }
}

/// <summary>Employee - Confidential Info.</summary>
public class EmployeeConfidentialInfoReport : EmployeeReportBase
{
    private readonly IRepository<ConfidentialInformation, Guid> _confidential;

    public EmployeeConfidentialInfoReport(IRepository<ConfidentialInformation, Guid> confidential)
    {
        _confidential = confidential;
    }

    public override StandardReportDefinition Definition { get; } = Define(5203, "EmployeeConfidentialInfo", "Employee - Confidential Info.");

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "employeeNo", "Employee No.");
        Text(result, "name", "Full Name");
        Text(result, "confidentialCode", "Confidential Code");
        Text(result, "description", "Description");

        var lines = (await _confidential.GetListAsync()).ToLookup(c => c.EmployeeNo, StringComparer.Ordinal);

        foreach (var employee in await GetEmployeesAsync(request))
        {
            foreach (var line in lines[employee.No.ToUpperInvariant()].OrderBy(c => c.ConfidentialCode, StringComparer.Ordinal).ThenBy(c => c.LineNo))
            {
                Row(
                    result,
                    ("employeeNo", employee.No),
                    ("name", NameOf(employee)),
                    ("confidentialCode", line.ConfidentialCode),
                    ("description", line.Description)
                );
            }
        }

        return result;
    }
}

// ---------------------------------------------------------------------------- Fixed assets

/// <summary>Fixed Asset - List: the asset register with each asset's depreciation setup.</summary>
public class FixedAssetListReport : StandardReportBase
{
    private readonly IRepository<FixedAsset, Guid> _assets;
    private readonly IRepository<FADepreciationBook, Guid> _books;

    public FixedAssetListReport(IRepository<FixedAsset, Guid> assets, IRepository<FADepreciationBook, Guid> books)
    {
        _assets = assets;
        _books = books;
    }

    public override StandardReportDefinition Definition { get; } =
        new(5601, "FixedAssetList", "Fixed Asset - List", StandardReportAreas.FixedAssets, StandardReportParameters.NoFilter | StandardReportParameters.DepreciationBook);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Text(result, "description", "Description");
        Text(result, "classCode", "FA Class Code");
        Text(result, "subclassCode", "FA Subclass Code");
        Text(result, "locationCode", "FA Location Code");
        Text(result, "responsibleEmployee", "Responsible Employee");
        Text(result, "serialNo", "Serial No.");
        Text(result, "depreciationMethod", "Depreciation Method");
        Date(result, "depreciationStartingDate", "Depreciation Starting Date");
        Number(result, "years", "No. of Depreciation Years");
        Text(result, "postingGroup", "FA Posting Group");

        var filter = Filter(request);
        var bookCode = request.DepreciationBookCode?.Trim().ToUpperInvariant();
        var books = (await _books.GetListAsync())
            .Where(b => bookCode.IsNullOrEmpty() || b.DepreciationBookCode == bookCode)
            .GroupBy(b => b.FANo)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(b => b.DefaultFADepreciationBook).First(), StringComparer.Ordinal);

        foreach (var asset in (await _assets.GetListAsync()).Where(a => filter.Matches(a.No)).OrderBy(a => a.No, StringComparer.Ordinal))
        {
            var book = books.GetValueOrDefault(asset.No);
            Row(
                result,
                ("no", asset.No),
                ("description", asset.Description),
                ("classCode", asset.FAClassCode),
                ("subclassCode", asset.FASubclassCode),
                ("locationCode", asset.FALocationCode),
                ("responsibleEmployee", asset.ResponsibleEmployee),
                ("serialNo", asset.SerialNo),
                ("depreciationMethod", book?.DepreciationMethod.ToString()),
                ("depreciationStartingDate", book?.DepreciationStartingDate),
                ("years", book?.NoOfDepreciationYears),
                ("postingGroup", book?.FAPostingGroup ?? asset.FAPostingGroup)
            );
        }

        return result;
    }
}

/// <summary>
/// Fixed Asset - Book Value 01. for each asset, its
/// acquisition cost and accumulated depreciation at the start of the period, what the period
/// added to each, and the book value at the end.
/// </summary>
public class FixedAssetBookValueReport : StandardReportBase
{
    private readonly IRepository<FixedAsset, Guid> _assets;
    private readonly IRepository<FALedgerEntry, Guid> _entries;

    public FixedAssetBookValueReport(IRepository<FixedAsset, Guid> assets, IRepository<FALedgerEntry, Guid> entries)
    {
        _assets = assets;
        _entries = entries;
    }

    public override StandardReportDefinition Definition { get; } =
        new(
            5605,
            "FixedAssetBookValue",
            "Fixed Asset - Book Value 01",
            StandardReportAreas.FixedAssets,
            StandardReportParameters.Period | StandardReportParameters.NoFilter | StandardReportParameters.DepreciationBook
        );

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "no", "No.");
        Text(result, "description", "Description");
        Number(result, "acquisitionStart", "Acquisition Cost at Start");
        Number(result, "additions", "Additions in Period");
        Number(result, "disposals", "Disposals in Period");
        Number(result, "acquisitionEnd", "Acquisition Cost at End");
        Number(result, "depreciationStart", "Depreciation at Start");
        Number(result, "depreciationPeriod", "Depreciation in Period");
        Number(result, "depreciationEnd", "Depreciation at End");
        Number(result, "bookValue", "Book Value at End");

        var filter = Filter(request);
        var bookCode = request.DepreciationBookCode?.Trim().ToUpperInvariant();
        var entries = (await _entries.GetListAsync(e => e.FAPostingDate <= request.To && !e.Reversed))
            .Where(e => bookCode.IsNullOrEmpty() || e.DepreciationBookCode == bookCode)
            .ToLookup(e => e.FANo, StringComparer.Ordinal);

        var totals = new decimal[8];

        foreach (var asset in (await _assets.GetListAsync()).Where(a => filter.Matches(a.No)).OrderBy(a => a.No, StringComparer.Ordinal))
        {
            var own = entries[asset.No].ToList();
            if (own.Count == 0)
            {
                continue;
            }

            decimal Sum(FALedgerEntryFAPostingType type, bool before) =>
                own.Where(e => e.FAPostingType == type && (e.FAPostingDate < request.From) == before).Sum(e => e.Amount);

            var acquisitionStart = Sum(FALedgerEntryFAPostingType.AcquisitionCost, true);
            var additions = own
                .Where(e => e.FAPostingType == FALedgerEntryFAPostingType.AcquisitionCost && e.FAPostingDate >= request.From && e.Amount > 0)
                .Sum(e => e.Amount);
            var disposals = own
                .Where(e => e.FAPostingType == FALedgerEntryFAPostingType.AcquisitionCost && e.FAPostingDate >= request.From && e.Amount < 0)
                .Sum(e => e.Amount);
            var depreciationStart = Sum(FALedgerEntryFAPostingType.Depreciation, true);
            var depreciationPeriod = Sum(FALedgerEntryFAPostingType.Depreciation, false);

            var values = new[]
            {
                acquisitionStart,
                additions,
                disposals,
                acquisitionStart + additions + disposals,
                depreciationStart,
                depreciationPeriod,
                depreciationStart + depreciationPeriod,
                own.Where(e => e.PartOfBookValue).Sum(e => e.Amount),
            };

            Row(
                result,
                ("no", asset.No),
                ("description", asset.Description),
                ("acquisitionStart", values[0]),
                ("additions", values[1]),
                ("disposals", values[2]),
                ("acquisitionEnd", values[3]),
                ("depreciationStart", values[4]),
                ("depreciationPeriod", values[5]),
                ("depreciationEnd", values[6]),
                ("bookValue", values[7])
            );

            for (var i = 0; i < values.Length; i++)
            {
                totals[i] += values[i];
            }
        }

        BoldRow(
            result,
            ("no", string.Empty),
            ("description", "Total"),
            ("acquisitionStart", totals[0]),
            ("additions", totals[1]),
            ("disposals", totals[2]),
            ("acquisitionEnd", totals[3]),
            ("depreciationStart", totals[4]),
            ("depreciationPeriod", totals[5]),
            ("depreciationEnd", totals[6]),
            ("bookValue", totals[7])
        );
        return result;
    }
}

/// <summary>Fixed Asset - Details: each asset's FA ledger entries in the period.</summary>
public class FixedAssetDetailsReport : StandardReportBase
{
    private readonly IRepository<FixedAsset, Guid> _assets;
    private readonly IRepository<FALedgerEntry, Guid> _entries;

    public FixedAssetDetailsReport(IRepository<FixedAsset, Guid> assets, IRepository<FALedgerEntry, Guid> entries)
    {
        _assets = assets;
        _entries = entries;
    }

    public override StandardReportDefinition Definition { get; } =
        new(
            5604,
            "FixedAssetDetails",
            "Fixed Asset - Details",
            StandardReportAreas.FixedAssets,
            StandardReportParameters.Period | StandardReportParameters.NoFilter | StandardReportParameters.DepreciationBook
        );

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Date(result, "postingDate", "FA Posting Date");
        Text(result, "postingType", "FA Posting Type");
        Text(result, "documentNo", "Document No.");
        Text(result, "description", "Description");
        Number(result, "amount", "Amount");
        Number(result, "bookValue", "Book Value");

        var filter = Filter(request);
        var bookCode = request.DepreciationBookCode?.Trim().ToUpperInvariant();
        var entries = (await _entries.GetListAsync(e => e.FAPostingDate <= request.To))
            .Where(e => bookCode.IsNullOrEmpty() || e.DepreciationBookCode == bookCode)
            .ToLookup(e => e.FANo, StringComparer.Ordinal);

        foreach (var asset in (await _assets.GetListAsync()).Where(a => filter.Matches(a.No)).OrderBy(a => a.No, StringComparer.Ordinal))
        {
            var bookValue = entries[asset.No].Where(e => e.FAPostingDate < request.From && e.PartOfBookValue && !e.Reversed).Sum(e => e.Amount);
            var inPeriod = entries[asset.No].Where(e => e.FAPostingDate >= request.From).OrderBy(e => e.FAPostingDate).ThenBy(e => e.EntryNo).ToList();
            if (bookValue == 0m && inPeriod.Count == 0)
            {
                continue;
            }

            BoldRow(result, ("documentNo", asset.No), ("description", asset.Description), ("bookValue", bookValue));

            foreach (var entry in inPeriod)
            {
                if (entry.PartOfBookValue && !entry.Reversed)
                {
                    bookValue += entry.Amount;
                }

                var row = Row(
                    result,
                    ("postingDate", entry.FAPostingDate),
                    ("postingType", entry.FAPostingType.ToString()),
                    ("documentNo", entry.DocumentNo),
                    ("description", entry.Description),
                    ("amount", entry.Amount),
                    ("bookValue", bookValue)
                );
                row.Indentation = 1;
                row.Italic = entry.Reversed;
            }
        }

        return result;
    }
}

/// <summary>Maintenance - Details.by way of the maintenance registrations of each asset.</summary>
public class MaintenanceRegisterReport : StandardReportBase
{
    private readonly IRepository<MaintenanceRegistration, Guid> _registrations;
    private readonly IRepository<FixedAsset, Guid> _assets;

    public MaintenanceRegisterReport(IRepository<MaintenanceRegistration, Guid> registrations, IRepository<FixedAsset, Guid> assets)
    {
        _registrations = registrations;
        _assets = assets;
    }

    public override StandardReportDefinition Definition { get; } =
        new(5634, "MaintenanceRegister", "Maintenance - Register", StandardReportAreas.FixedAssets, StandardReportParameters.Period | StandardReportParameters.NoFilter);

    public override async Task<ReportResult> RunAsync(StandardReportRequest request)
    {
        var result = NewResult(request);
        Text(result, "faNo", "FA No.");
        Text(result, "description", "Description");
        Date(result, "serviceDate", "Service Date");
        Text(result, "vendorNo", "Maintenance Vendor No.");
        Text(result, "agent", "Service Agent Name");
        Text(result, "phoneNo", "Service Agent Phone No.");
        Text(result, "comment", "Comment");

        var filter = Filter(request);
        var names = (await _assets.GetListAsync()).ToDictionary(a => a.No, a => a.Description, StringComparer.Ordinal);

        foreach (var line in (await _registrations.GetListAsync())
            .Where(r => filter.Matches(r.FANo) && (!r.ServiceDate.HasValue || (r.ServiceDate >= request.From && r.ServiceDate <= request.To)))
            .OrderBy(r => r.FANo, StringComparer.Ordinal)
            .ThenBy(r => r.ServiceDate))
        {
            Row(
                result,
                ("faNo", line.FANo),
                ("description", names.GetValueOrDefault(line.FANo, string.Empty)),
                ("serviceDate", line.ServiceDate),
                ("vendorNo", line.MaintenanceVendorNo),
                ("agent", line.ServiceAgentName),
                ("phoneNo", line.ServiceAgentPhoneNo),
                ("comment", line.Comment)
            );
        }

        return result;
    }
}
