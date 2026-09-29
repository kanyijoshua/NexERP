using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ABPmicroservice.Erp.Dimensions;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Kanban;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Reporting;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// A field that points at a record of another table. Mirrors a Business Central TableRelation:
/// the value must exist in the related table before the record can be applied.
/// </summary>
public sealed class ConfigTableRelation
{
    internal ConfigTableRelation(string fieldName, string targetEntity, string targetKeyField, bool isIdReference, string codeFieldName)
    {
        FieldName = fieldName;
        TargetEntity = targetEntity;
        TargetKeyField = targetKeyField;
        IsIdReference = isIdReference;
        CodeFieldName = codeFieldName;
    }

    public string FieldName { get; }

    public string TargetEntity { get; }

    /// <summary>The related table's key, e.g. "Code". The file always carries this, never an id.</summary>
    public string TargetKeyField { get; }

    /// <summary>
    /// True when the field stores the related row's Guid. Ids differ from one company to the next,
    /// so a package carries the related row's key instead and the id is looked up on the way in.
    /// This is what makes a package portable between companies, as BC's code-keyed tables are.
    /// </summary>
    public bool IsIdReference { get; }

    /// <summary>
    /// For an id reference, a field on the same table that repeats the related key in plain text
    /// (Item.ItemCategoryCode beside Item.ItemCategoryId). When the file has only the code, the id
    /// is taken from it.
    /// </summary>
    public string CodeFieldName { get; }
}

/// <summary>
/// How a configuration package may read and write one table: its primary key, its relations, and
/// the checks a value must pass. This is the metadata Business Central keeps on every table and
/// RapidStart reads from it; here it is declared once per table that may be imported.
/// </summary>
public sealed class ConfigTableProfile
{
    private static readonly HashSet<string> SystemFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id",
        "CompanyId",
        "CreationTime",
        "CreatorId",
        "LastModificationTime",
        "LastModifierId",
    };

    private readonly Dictionary<string, int> _maxLengths;
    private readonly HashSet<string> _required;
    private readonly HashSet<string> _upperCase;
    private readonly Dictionary<string, ConfigTableRelation> _relations;
    private readonly Dictionary<string, ErpEntityField> _importable;

    internal ConfigTableProfile(ConfigTableBuilder builder)
    {
        Definition = builder.Definition;
        Area = builder.Area;
        KeyFields = builder.KeyFields;
        ParentEntity = builder.ParentEntity;

        _relations = builder.Relations.ToDictionary(r => r.FieldName, StringComparer.OrdinalIgnoreCase);
        _required = new HashSet<string>(builder.Required.Concat(builder.KeyFields), StringComparer.OrdinalIgnoreCase);
        _upperCase = new HashSet<string>(builder.UpperCase, StringComparer.OrdinalIgnoreCase);
        _maxLengths = new Dictionary<string, int>(builder.MaxLengths, StringComparer.OrdinalIgnoreCase);

        ImportableFields = Definition
            .Fields.Where(f => !SystemFields.Contains(f.Name))
            .Where(f => !builder.ReadOnly.Contains(f.Name, StringComparer.OrdinalIgnoreCase))
            .Where(f => HasSetter(Definition.EntityType, f.Name))
            .ToList();

        _importable = ImportableFields.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
        Relations = builder.Relations;
    }

    /// <summary>Stable table name, the same one the export and integration APIs use.</summary>
    public string Name => Definition.Name;

    public ErpEntityDefinition Definition { get; }

    /// <summary>Functional area the table is listed under on the configuration worksheet.</summary>
    public string Area { get; }

    /// <summary>
    /// Fields that identify a record, as BC's primary key does. A record in a file whose key
    /// matches an existing one updates it; otherwise it is inserted. No key fields means the table
    /// holds one record per company (a setup table).
    /// </summary>
    public IReadOnlyList<string> KeyFields { get; }

    public bool IsSingleton => KeyFields.Count == 0;

    /// <summary>For a line table, the header table it belongs to; offered alongside it.</summary>
    public string ParentEntity { get; }

    public IReadOnlyList<ConfigTableRelation> Relations { get; }

    /// <summary>
    /// Fields a package may carry: everything but ids and audit columns, and the totals that only
    /// posting may change (a customer's balance is the sum of its ledger, not a number to type in).
    /// </summary>
    public IReadOnlyList<ErpEntityField> ImportableFields { get; }

    public ErpEntityField FindImportableField(string name) => _importable.GetValueOrDefault(name ?? string.Empty);

    public ConfigTableRelation FindRelation(string fieldName) => _relations.GetValueOrDefault(fieldName ?? string.Empty);

    public bool IsKeyField(string fieldName) => KeyFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase);

    public bool IsRequired(string fieldName) => _required.Contains(fieldName);

    public bool IsUpperCase(string fieldName) => _upperCase.Contains(fieldName);

    public int? MaxLength(string fieldName)
    {
        if (_maxLengths.TryGetValue(fieldName, out var length))
        {
            return length;
        }

        var field = FindImportableField(fieldName);
        return field?.ClrType == typeof(string) ? ConventionalLength(fieldName) : null;
    }

    /// <summary>
    /// The lengths the entities check, keyed by the naming convention they share, so that a
    /// profile needs to spell out only the exceptions.
    /// </summary>
    private static int? ConventionalLength(string fieldName)
    {
        return fieldName switch
        {
            "No" => ErpDomainConsts.MaxNoLength,
            "Name" => ErpDomainConsts.MaxNameLength,
            "Description" => ErpDomainConsts.MaxDescriptionLength,
            "Address" => ErpDomainConsts.MaxAddressLength,
            "City" => ErpDomainConsts.MaxCityLength,
            "PostCode" => ErpDomainConsts.MaxPostCodeLength,
            "CountryRegionCode" => ErpDomainConsts.MaxCountryRegionCodeLength,
            "PhoneNo" => ErpDomainConsts.MaxPhoneLength,
            "Email" => ErpDomainConsts.MaxEmailLength,
            "CurrencyCode" => ErpDomainConsts.MaxCurrencyCodeLength,
            "PaymentTermsCode" => ErpDomainConsts.MaxPaymentTermsCodeLength,
            "NoSeriesCode" => ErpDomainConsts.MaxNoSeriesCodeLength,
            _ when fieldName.EndsWith("PostingGroup", StringComparison.Ordinal) => ErpDomainConsts.MaxPostingGroupLength,
            _ when fieldName.EndsWith("AccountNo", StringComparison.Ordinal) => ErpDomainConsts.MaxNoLength,
            _ when fieldName.EndsWith("Nos", StringComparison.Ordinal) => ErpDomainConsts.MaxNoSeriesCodeLength,
            _ when fieldName.EndsWith("No", StringComparison.Ordinal) => ErpDomainConsts.MaxDocumentNoLength,
            _ when fieldName.EndsWith("Code", StringComparison.Ordinal) => ErpDomainConsts.MaxCodeLength,
            _ => ErpDomainConsts.MaxConfigValueLength,
        };
    }

    /// <summary>A field with no setter at all is computed, and there is nothing to write it to.</summary>
    private static bool HasSetter(Type entityType, string propertyName)
    {
        var property = entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        var declared = property?.DeclaringType?.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        return declared?.GetSetMethod(nonPublic: true) != null;
    }
}

/// <summary>Collects one table's profile. Used only while the registry is being built.</summary>
internal sealed class ConfigTableBuilder
{
    public ConfigTableBuilder(ErpEntityDefinition definition, string area, string[] keyFields)
    {
        Definition = definition;
        Area = area;
        KeyFields = keyFields;
    }

    public ErpEntityDefinition Definition { get; }
    public string Area { get; }
    public IReadOnlyList<string> KeyFields { get; }
    public string ParentEntity { get; private set; }
    public List<string> Required { get; } = [];
    public List<string> ReadOnly { get; } = [];
    public List<string> UpperCase { get; } = [];
    public Dictionary<string, int> MaxLengths { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<ConfigTableRelation> Relations { get; } = [];

    public ConfigTableBuilder Requires(params string[] fields)
    {
        Required.AddRange(fields);
        return this;
    }

    /// <summary>Kept out of packages: maintained by posting, never typed in.</summary>
    public ConfigTableBuilder Calculated(params string[] fields)
    {
        ReadOnly.AddRange(fields);
        return this;
    }

    /// <summary>Stored in capitals, as BC stores every Code field.</summary>
    public ConfigTableBuilder Capitals(params string[] fields)
    {
        UpperCase.AddRange(fields);
        return this;
    }

    public ConfigTableBuilder Length(string field, int maxLength)
    {
        MaxLengths[field] = maxLength;
        return this;
    }

    /// <summary>A code field that must name an existing record of <paramref name="targetEntity"/>.</summary>
    public ConfigTableBuilder RelatesTo(string field, string targetEntity, string targetKeyField)
    {
        Relations.Add(new ConfigTableRelation(field, targetEntity, targetKeyField, isIdReference: false, codeFieldName: null));
        return this;
    }

    /// <summary>A Guid field that points at a record of <paramref name="targetEntity"/>, carried in files by its key.</summary>
    public ConfigTableBuilder References(string field, string targetEntity, string targetKeyField, string codeField = null)
    {
        Relations.Add(new ConfigTableRelation(field, targetEntity, targetKeyField, isIdReference: true, codeFieldName: codeField));
        return this;
    }

    public ConfigTableBuilder LinesOf(string parentEntity)
    {
        ParentEntity = parentEntity;
        return this;
    }
}

/// <summary>
/// The tables a configuration package or a data import may write to.
/// <para>
/// Business Central's RapidStart can reach any table, and relies on the table's own triggers and
/// relations to keep bad data out. Here writing is opt-in: master data and setup are listed, and
/// ledgers, posted documents and registers are not — those come only from posting, exactly as the
/// append-only guard in the database context insists. A table in the entity registry that is not
/// listed here can still be exported; it just cannot be imported.
/// </para>
/// </summary>
public class ConfigTableRegistry : ISingletonDependency
{
    public const string AreaFinance = "Finance";
    public const string AreaSales = "Sales";
    public const string AreaPurchasing = "Purchasing";
    public const string AreaInventory = "Inventory";
    public const string AreaDimensions = "Dimensions";
    public const string AreaSetup = "Setup";
    public const string AreaReporting = "Reporting";

    /// <summary>The order the worksheet lists the areas in: what everything else depends on first.</summary>
    public static readonly IReadOnlyList<string> Areas =
    [
        AreaSetup,
        AreaFinance,
        AreaDimensions,
        AreaSales,
        AreaPurchasing,
        AreaInventory,
        AreaReporting,
    ];

    private readonly Dictionary<string, ConfigTableProfile> _byName;

    public ConfigTableRegistry(ErpEntityRegistry entities)
    {
        ConfigTableBuilder Table<TEntity>(string area, params string[] keyFields) =>
            new(entities.Get(typeof(TEntity).Name), area, keyFields);

        var builders = new List<ConfigTableBuilder>
        {
            // Setup: number series come first, since nearly every other table can name one.
            Table<NoSeries>(AreaSetup, "Code").Length("Code", ErpDomainConsts.MaxNoSeriesCodeLength),
            Table<NoSeriesLine>(AreaSetup, "NoSeriesId", "LineNo")
                .LinesOf(nameof(NoSeries))
                .References("NoSeriesId", nameof(NoSeries), "Code")
                .Requires("StartingNo"),
            Table<SalesReceivablesSetup>(AreaSetup)
                .RelatesTo("CustomerNos", nameof(NoSeries), "Code")
                .RelatesTo("QuoteNos", nameof(NoSeries), "Code")
                .RelatesTo("OrderNos", nameof(NoSeries), "Code")
                .RelatesTo("InvoiceNos", nameof(NoSeries), "Code")
                .RelatesTo("CreditMemoNos", nameof(NoSeries), "Code")
                .RelatesTo("PostedInvoiceNos", nameof(NoSeries), "Code")
                .RelatesTo("PostedCreditMemoNos", nameof(NoSeries), "Code"),
            Table<PurchasesPayablesSetup>(AreaSetup)
                .RelatesTo("VendorNos", nameof(NoSeries), "Code")
                .RelatesTo("QuoteNos", nameof(NoSeries), "Code")
                .RelatesTo("OrderNos", nameof(NoSeries), "Code")
                .RelatesTo("InvoiceNos", nameof(NoSeries), "Code")
                .RelatesTo("CreditMemoNos", nameof(NoSeries), "Code")
                .RelatesTo("PostedInvoiceNos", nameof(NoSeries), "Code")
                .RelatesTo("PostedCreditMemoNos", nameof(NoSeries), "Code"),
            Table<KanbanStage>(AreaSetup, "PipelineType", "Name"),

            // Finance
            Table<GLAccount>(AreaFinance, "No").Requires("Name").Calculated("NetChange", "Balance"),
            Table<GeneralPostingSetup>(AreaFinance, "GenBusPostingGroup", "GenProdPostingGroup")
                .Requires("SalesAccountNo", "PurchAccountNo", "COGSAccountNo", "InventoryAdjmtAccountNo")
                .Length("GenBusPostingGroup", ErpDomainConsts.MaxGeneralBusPostingGroupLength)
                .RelatesTo("SalesAccountNo", nameof(GLAccount), "No")
                .RelatesTo("SalesCreditMemoAccountNo", nameof(GLAccount), "No")
                .RelatesTo("SalesDiscountAccountNo", nameof(GLAccount), "No")
                .RelatesTo("PurchAccountNo", nameof(GLAccount), "No")
                .RelatesTo("PurchCreditMemoAccountNo", nameof(GLAccount), "No")
                .RelatesTo("PurchDiscountAccountNo", nameof(GLAccount), "No")
                .RelatesTo("COGSAccountNo", nameof(GLAccount), "No")
                .RelatesTo("InventoryAdjmtAccountNo", nameof(GLAccount), "No"),
            Table<GenJournalTemplate>(AreaFinance, "Name")
                .Length("Name", ErpDomainConsts.MaxJournalTemplateNameLength)
                .Length("SourceCode", ErpDomainConsts.MaxSourceCodeLength)
                .Capitals("Name", "SourceCode", "NoSeriesCode")
                .RelatesTo("NoSeriesCode", nameof(NoSeries), "Code"),
            Table<GenJournalBatch>(AreaFinance, "JournalTemplateName", "Name")
                .LinesOf(nameof(GenJournalTemplate))
                .Length("JournalTemplateName", ErpDomainConsts.MaxJournalTemplateNameLength)
                .Length("Name", ErpDomainConsts.MaxJournalTemplateNameLength)
                .Length("ReasonCode", ErpDomainConsts.MaxReasonCodeLength)
                .Length("BalAccountNo", ErpDomainConsts.MaxNoLength)
                .Capitals("JournalTemplateName", "Name", "ReasonCode", "NoSeriesCode")
                .RelatesTo("JournalTemplateName", nameof(GenJournalTemplate), "Name")
                .RelatesTo("NoSeriesCode", nameof(NoSeries), "Code"),

            // Dimensions
            Table<Dimension>(AreaDimensions, "Code").Requires("Name").Length("Code", ErpDomainConsts.MaxDimensionCodeLength),
            Table<DimensionValue>(AreaDimensions, "DimensionCode", "Code")
                .LinesOf(nameof(Dimension))
                .Requires("Name")
                .Length("Code", ErpDomainConsts.MaxDimensionValueCodeLength)
                .Length("DimensionCode", ErpDomainConsts.MaxDimensionCodeLength)
                .RelatesTo("DimensionCode", nameof(Dimension), "Code")
                .References("DimensionId", nameof(Dimension), "Code", codeField: "DimensionCode"),

            // Sales
            Table<CustomerPostingGroup>(AreaSales, "Code")
                .Requires("ReceivablesAccountNo")
                .Length("Code", ErpDomainConsts.MaxPostingGroupLength)
                .RelatesTo("ReceivablesAccountNo", nameof(GLAccount), "No"),
            Table<Customer>(AreaSales, "No")
                .Requires("Name")
                .Calculated("Balance")
                .RelatesTo("CustomerPostingGroup", nameof(CustomerPostingGroup), "Code"),

            // Purchasing
            Table<VendorPostingGroup>(AreaPurchasing, "Code")
                .Requires("PayablesAccountNo")
                .Length("Code", ErpDomainConsts.MaxPostingGroupLength)
                .RelatesTo("PayablesAccountNo", nameof(GLAccount), "No"),
            Table<Vendor>(AreaPurchasing, "No")
                .Requires("Name")
                .Calculated("Balance")
                .RelatesTo("VendorPostingGroup", nameof(VendorPostingGroup), "Code"),

            // Inventory
            Table<UnitOfMeasure>(AreaInventory, "Code").Length("Code", ErpDomainConsts.MaxUnitOfMeasureCodeLength),
            Table<ItemCategory>(AreaInventory, "Code")
                .References("ParentCategoryId", nameof(ItemCategory), "Code", codeField: "ParentCategoryCode")
                .RelatesTo("ParentCategoryCode", nameof(ItemCategory), "Code"),
            Table<Item>(AreaInventory, "No")
                .Requires("Description")
                .Calculated("Inventory")
                .Length("BaseUnitOfMeasureCode", ErpDomainConsts.MaxUnitOfMeasureCodeLength)
                .RelatesTo("BaseUnitOfMeasureCode", nameof(UnitOfMeasure), "Code")
                .RelatesTo("ItemCategoryCode", nameof(ItemCategory), "Code")
                .References("ItemCategoryId", nameof(ItemCategory), "Code", codeField: "ItemCategoryCode"),

            // Reporting
            Table<ColumnLayout>(AreaReporting, "Name"),
            Table<ColumnLayoutLine>(AreaReporting, "ColumnLayoutId", "LineNo")
                .LinesOf(nameof(ColumnLayout))
                .References("ColumnLayoutId", nameof(ColumnLayout), "Name")
                .Requires("ColumnNo")
                .Length("ColumnNo", ErpDomainConsts.MaxRowNoLength)
                .Length("ColumnHeader", ErpDomainConsts.MaxColumnHeaderLength)
                .Length("ComparisonDateFormula", ErpDomainConsts.MaxDateFormulaLength)
                .Capitals("ColumnNo"),
            Table<AccountSchedule>(AreaReporting, "Name").RelatesTo("DefaultColumnLayoutName", nameof(ColumnLayout), "Name"),
            Table<AccountScheduleLine>(AreaReporting, "AccountScheduleId", "LineNo")
                .LinesOf(nameof(AccountSchedule))
                .References("AccountScheduleId", nameof(AccountSchedule), "Name")
                .Requires("RowNo")
                .Length("RowNo", ErpDomainConsts.MaxRowNoLength)
                .Length("Totaling", ErpDomainConsts.MaxTotalingLength)
                .Capitals("RowNo"),
        };

        _byName = builders.Select(b => new ConfigTableProfile(b)).ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<ConfigTableProfile> GetAll() => _byName.Values.ToList();

    public ConfigTableProfile Find(string name) => _byName.GetValueOrDefault(name ?? string.Empty);

    public ConfigTableProfile Get(string name)
    {
        return Find(name)
            ?? throw new BusinessException(ErpErrorCodes.RapidStart.TableNotImportable).WithData("entityName", name ?? string.Empty);
    }

    /// <summary>
    /// Tables the given ones refer to, directly or through other tables, and their line tables:
    /// what BC's "Get Related Tables" adds, so a package does not arrive with customers whose
    /// posting group it forgot to bring.
    /// </summary>
    public IReadOnlyList<string> GetRelatedTables(IEnumerable<string> names)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var pending = new Queue<string>(names.Where(n => Find(n) != null));

        while (pending.Count > 0)
        {
            var profile = Get(pending.Dequeue());
            if (!result.Add(profile.Name))
            {
                continue;
            }

            foreach (var relation in profile.Relations)
            {
                pending.Enqueue(relation.TargetEntity);
            }

            foreach (var child in _byName.Values.Where(p => string.Equals(p.ParentEntity, profile.Name, StringComparison.OrdinalIgnoreCase)))
            {
                pending.Enqueue(child.Name);
            }
        }

        return result.ToList();
    }

    /// <summary>
    /// Orders tables so that every table comes after the tables it relates to. BC leaves the
    /// processing order to the person building the package; working it out from the relations
    /// means a posting group is always in place before the customer that names it.
    /// </summary>
    public IReadOnlyList<string> SortByDependencies(IEnumerable<string> names)
    {
        var wanted = names.Select(Get).GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList();
        var inSet = new HashSet<string>(wanted.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);
        var sorted = new List<string>();
        var state = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase); // false = visiting, true = done

        void Visit(ConfigTableProfile profile)
        {
            if (state.TryGetValue(profile.Name, out var done))
            {
                // A cycle (a table relating to itself) is not a dependency to wait for.
                return;
            }

            state[profile.Name] = false;

            foreach (var target in profile.Relations.Select(r => r.TargetEntity).Where(inSet.Contains))
            {
                Visit(Get(target));
            }

            state[profile.Name] = true;
            sorted.Add(profile.Name);
        }

        // The registry's own order breaks ties, so the result is stable from one call to the next.
        foreach (var profile in _byName.Values.Where(p => inSet.Contains(p.Name)))
        {
            Visit(profile);
        }

        return sorted;
    }
}
