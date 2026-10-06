using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ABPmicroservice.Erp.Academics;
using ABPmicroservice.Erp.Payroll;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Chatter;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.FixedAssets;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Kanban;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Pensions;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Workflows;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>One column of a table as the export and integration APIs see it.</summary>
public sealed class ErpEntityField
{
    internal ErpEntityField(PropertyInfo property)
    {
        Property = property;
        Name = property.Name;
        DisplayName = Humanize(property.Name);
        ClrType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        IsNullable = !property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) != null;
    }

    public string Name { get; }

    /// <summary>The name a person reads: "GLAccountNo" becomes "GL Account No".</summary>
    public string DisplayName { get; }

    /// <summary>Underlying type, with any Nullable&lt;&gt; peeled off.</summary>
    public Type ClrType { get; }

    public bool IsNullable { get; }

    internal PropertyInfo Property { get; }

    public object GetValue(object entity) => Property.GetValue(entity);

    /// <summary>
    /// Splits a property name into words, keeping runs of capitals together so acronyms survive.
    /// </summary>
    public static string Humanize(string name)
    {
        var builder = new StringBuilder(name.Length + 8);

        for (var i = 0; i < name.Length; i++)
        {
            var current = name[i];
            var startsWord =
                i > 0
                && char.IsUpper(current)
                && (!char.IsUpper(name[i - 1]) || (i + 1 < name.Length && char.IsLower(name[i + 1])));

            if (startsWord)
            {
                builder.Append(' ');
            }

            builder.Append(current);
        }

        return builder.ToString();
    }
}

/// <summary>A table that may be exported, queried over the integration API, or published.</summary>
public sealed class ErpEntityDefinition
{
    internal ErpEntityDefinition(string name, Type entityType)
    {
        Name = name;
        EntityType = entityType;
        IsCompanyScoped = typeof(ICompanyScoped).IsAssignableFrom(entityType);
        Fields = DiscoverFields(entityType);
        _byName = Fields.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
    }

    private readonly Dictionary<string, ErpEntityField> _byName;

    /// <summary>Stable name the API addresses the table by, e.g. "Customer".</summary>
    public string Name { get; }

    public Type EntityType { get; }

    /// <summary>True when rows belong to one company and the company filter applies.</summary>
    public bool IsCompanyScoped { get; }

    public IReadOnlyList<ErpEntityField> Fields { get; }

    public ErpEntityField FindField(string name) => _byName.GetValueOrDefault(name ?? string.Empty);

    public ErpEntityField GetField(string name)
    {
        return FindField(name)
            ?? throw new BusinessException(ErpErrorCodes.Exporting.UnknownField)
                .WithData("entityName", Name)
                .WithData("fieldName", name);
    }

    /// <summary>Columns offered when the caller names none: everything but the plumbing.</summary>
    public IReadOnlyList<ErpEntityField> DefaultFields =>
        Fields.Where(f => !HiddenByDefault.Contains(f.Name)).ToList();

    private static readonly HashSet<string> HiddenByDefault = new(StringComparer.OrdinalIgnoreCase)
    {
        "CreationTime",
        "CreatorId",
        "LastModificationTime",
        "LastModifierId",
        "CompanyId",
    };

    /// <summary>Columns that say nothing about the business and would only clutter an export.</summary>
    private static readonly HashSet<string> Excluded = new(StringComparer.OrdinalIgnoreCase)
    {
        "ExtraProperties",
        "ConcurrencyStamp",
        "TenantId",
        "IsDeleted",
        "DeleterId",
        "DeletionTime",
    };

    private static IReadOnlyList<ErpEntityField> DiscoverFields(Type entityType)
    {
        return entityType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .Where(p => !Excluded.Contains(p.Name))
            .Where(p => IsScalar(p.PropertyType))
            .Select(p => new ErpEntityField(p))
            .ToList();
    }

    /// <summary>
    /// Only values that fit in a cell are exported. Child collections are their own table and are
    /// exported separately, which is also how a configuration package treats them.
    /// </summary>
    private static bool IsScalar(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        return underlying.IsEnum
            || underlying == typeof(string)
            || underlying == typeof(Guid)
            || underlying == typeof(bool)
            || underlying == typeof(DateTime)
            || underlying == typeof(DateTimeOffset)
            || underlying == typeof(TimeSpan)
            || underlying == typeof(decimal)
            || underlying == typeof(double)
            || underlying == typeof(float)
            || underlying == typeof(byte)
            || underlying == typeof(short)
            || underlying == typeof(int)
            || underlying == typeof(long);
    }
}

/// <summary>
/// Every table the ERP is willing to hand out, with the permission that guards it.
/// <para>
/// What leaves the system is decided in one place: one list of tables, each with its
/// fields. This registry is that list, and
/// it is the single source for exports, the integration API and webhook subscriptions, so a table
/// cannot be reachable through one of them and not the others.
/// </para>
/// </summary>
public class ErpEntityRegistry : ISingletonDependency
{
    private readonly Dictionary<string, ErpEntityDefinition> _byName;

    public ErpEntityRegistry()
    {
        var definitions = new List<ErpEntityDefinition>
        {
            // Finance
            Define<GLAccount>(),
            Define<GLEntry>(),
            Define<GLRegister>(),
            Define<GeneralLedgerSetup>(),
            Define<GenBusinessPostingGroup>(),
            Define<GenProductPostingGroup>(),
            Define<GeneralPostingSetup>(),
            Define<GenJournalTemplate>(),
            Define<GenJournalBatch>(),
            Define<GenJournalLine>(),
            Define<StandardGeneralJournal>(),
            Define<StandardGeneralJournalLine>(),

            // Sales
            Define<Customer>(),
            Define<CustomerPostingGroup>(),
            Define<CustomerLedgerEntry>(),
            Define<SalesHeader>(),
            Define<SalesLine>(),
            Define<PostedSalesHeader>(),
            Define<PostedSalesLine>(),
            Define<SalesReceivablesSetup>(),

            // Purchasing
            Define<Vendor>(),
            Define<VendorPostingGroup>(),
            Define<VendorLedgerEntry>(),
            Define<PurchaseHeader>(),
            Define<PurchaseLine>(),
            Define<PostedPurchaseHeader>(),
            Define<PostedPurchaseLine>(),
            Define<PurchasesPayablesSetup>(),

            // Inventory
            Define<Item>(),
            Define<ItemCategory>(),
            Define<UnitOfMeasure>(),
            Define<InventoryPostingGroup>(),
            Define<InventoryPostingSetup>(),
            Define<ItemLedgerEntry>(),
            Define<ValueEntry>(),

            // Dimensions
            Define<Dimension>(),
            Define<DimensionValue>(),
            Define<DefaultDimension>(),

            // Numbering and approvals
            Define<NoSeries>(),
            Define<NoSeriesLine>(),
            Define<Workflow>(),
            Define<ApprovalEntry>(),
            Define<ApprovalUserSetup>(),

            // Reporting setup
            Define<AccountSchedule>(),
            Define<AccountScheduleLine>(),
            Define<ColumnLayout>(),
            Define<ColumnLayoutLine>(),

            // Collaboration
            Define<KanbanStage>(),
            Define<DocumentNote>(),
            Define<ActivityStreamEntry>(),
            Define<DocumentActivityTask>(),

            // Tax, cash management, finance and human resources setup
            Define<PaymentTerms>(),
            Define<Currency>(),
            Define<CurrencyExchangeRate>(),
            Define<AccountingPeriod>(),
            Define<VatBusinessPostingGroup>(),
            Define<VatProductPostingGroup>(),
            Define<VatPostingSetup>(),
            Define<VatEntry>(),
            Define<Location>(),
            Define<InventorySetup>(),
            Define<SalespersonPurchaser>(),
            Define<BankAccountPostingGroup>(),
            Define<BankAccount>(),
            Define<BankAccountLedgerEntry>(),
            Define<EmployeeLedgerEntry>(),
            Define<ExchRateAdjmtRegister>(),
            Define<PaymentMethod>(),
            Define<HumanResourcesSetup>(),
            Define<HumanResourceUnitOfMeasure>(),
            Define<EmployeePostingGroup>(),
            Define<CauseOfAbsence>(),
            Define<Qualification>(),
            Define<Union>(),
            Define<EmploymentContract>(),
            Define<GroundsForTermination>(),
            Define<Employee>(),
            Define<EmployeeAbsence>(),

            // Base tables: setup, sub-ledgers and registers
            Define<SourceCode>(),
            Define<ReasonCode>(),
            Define<CountryRegion>(),
            Define<PostCode>(),
            Define<ShipmentMethod>(),
            Define<ResponsibilityCenter>(),
            Define<UserSetup>(),
            Define<GLBudgetName>(),
            Define<GLBudgetEntry>(),
            Define<CommentLine>(),
            Define<CustomerBankAccount>(),
            Define<DetailedCustLedgEntry>(),
            Define<VendorBankAccount>(),
            Define<DetailedVendorLedgEntry>(),
            Define<OrderAddress>(),
            Define<ItemVendor>(),
            Define<PurchCommentLine>(),
            Define<BankAccReconciliation>(),
            Define<BankAccReconciliationLine>(),
            Define<BankAccountStatement>(),
            Define<BankAccountStatementLine>(),
            Define<CheckLedgerEntry>(),
            Define<Relative>(),
            Define<MiscArticle>(),
            Define<Confidential>(),
            Define<EmployeeStatisticsGroup>(),
            Define<EmployeeRelative>(),
            Define<EmployeeQualification>(),
            Define<MiscArticleInformation>(),
            Define<ConfidentialInformation>(),
            Define<AlternativeAddress>(),
            Define<HumanResourceCommentLine>(),
            Define<FAClass>(),
            Define<FASubclass>(),
            Define<FALocation>(),
            Define<Maintenance>(),
            Define<DepreciationBook>(),
            Define<FAPostingGroup>(),
            Define<FASetup>(),
            Define<FixedAsset>(),
            Define<FADepreciationBook>(),
            Define<FALedgerEntry>(),
            Define<MaintenanceRegistration>(),
            Define<MainAssetComponent>(),
            Define<ReportSelection>(),
            Define<CustomReportSelection>(),
            Define<WorkflowUserGroup>(),
            Define<WorkflowUserGroupMember>(),
            Define<ApprovalCommentLine>(),

            // Pensions
            Define<PensionScheme>(),
            Define<PensionSponsor>(),
            Define<PensionMember>(),
            Define<MemberLedgerEntry>(),
            Define<PensionContributionHeader>(),
            Define<PensionContributionLine>(),
            Define<PensionInterestRate>(),
            Define<ExitReason>(),
            Define<LumpsumTaxTable>(),
            Define<LumpsumTaxBand>(),
            Define<MemberExit>(),

            // Academics
            Define<AcademicYear>(),
            Define<Semester>(),
            Define<Intake>(),
            Define<ExamCategory>(),
            Define<GradingBand>(),
            Define<ExamComponent>(),
            Define<Programme>(),
            Define<ProgrammeStage>(),
            Define<CourseUnit>(),
            Define<FeeItem>(),
            Define<FeeStructureLine>(),
            Define<StudentApplication>(),
            Define<Student>(),
            Define<SemesterRegistration>(),
            Define<StudentUnit>(),
            Define<StudentBillHeader>(),
            Define<StudentBillLine>(),
            Define<ExamResultHeader>(),
            Define<ExamResultLine>(),
            Define<StudentReceipt>(),
            Define<StudentRefund>(),
            Define<StudentStatusChange>(),

            // Pensioners and pension payroll
            Define<Pensioner>(),
            Define<PensionPayrollHeader>(),
            Define<PensionPayrollLine>(),
            Define<PensionBenefitCalculation>(),
            Define<PensionBeneficiary>(),
            Define<PensionContributionRate>(),
            Define<PensionVestingScale>(),
            Define<PensionTaxReliefLimit>(),
            Define<MemberStatusEntry>(),
            Define<MemberSalaryEntry>(),
            Define<PensionIncrement>(),
            Define<PensionerChangeEntry>(),
            Define<PensionBank>(),
            Define<PensionBankBranch>(),
            Define<PensionerPayMode>(),
            Define<PensionerSuspensionReason>(),
            Define<PensionRevisionReason>(),
            Define<OtherPensionScheme>(),
            Define<PensionerPayItem>(),
            Define<PensionerPayItemAssignment>(),
            Define<PensionPayrollLineItem>(),
            Define<ExitReasonDocument>(),
            Define<MemberExitDocument>(),
            Define<PensionAgeFactor>(),

            // Payroll
            Define<PayrollEarning>(),
            Define<PayrollDeduction>(),
            Define<PayrollTaxBand>(),
            Define<EmployeePayItem>(),
            Define<PayrollRun>(),
            Define<Payslip>(),
            Define<PayslipLine>(),

            // Campus
            Define<LectureRoom>(),
            Define<TimetableEntry>(),
            Define<AttendanceRegister>(),
            Define<AttendanceLine>(),
            Define<Hostel>(),
            Define<HostelRoom>(),
            Define<HostelAllocation>(),
            Define<ClinicVisit>(),
            Define<ClinicPrescription>(),
            Define<LaundryItem>(),
            Define<LaundryOrder>(),
            Define<LaundryOrderLine>(),
            Define<ShortCourse>(),
            Define<ShortCourseApplication>(),
            Define<ShortCourseParticipant>(),

            // Payment vouchers
            Define<PaymentDeductionCode>(),
            Define<PaymentType>(),
            Define<PaymentVoucherHeader>(),
            Define<PaymentVoucherLine>(),

            Define<Company>(),
        };

        _byName = definitions.ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ErpEntityDefinition> GetAll() => _byName.Values.OrderBy(d => d.Name, StringComparer.Ordinal).ToList();

    public ErpEntityDefinition Find(string name) => _byName.GetValueOrDefault(name ?? string.Empty);

    public ErpEntityDefinition Get(string name)
    {
        return Find(name)
            ?? throw new BusinessException(ErpErrorCodes.Exporting.UnknownEntity).WithData("entityName", name);
    }

    private static ErpEntityDefinition Define<TEntity>()
    {
        return new ErpEntityDefinition(typeof(TEntity).Name, typeof(TEntity));
    }
}
