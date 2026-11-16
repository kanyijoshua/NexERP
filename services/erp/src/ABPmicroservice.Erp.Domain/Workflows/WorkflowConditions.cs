using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Procurement;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>A field of a document a workflow condition can test, e.g. "Buy-From Vendor No.".</summary>
public sealed record WorkflowConditionField(string Field, string Label, WorkflowConditionFieldType Type, IReadOnlyList<string> Options);

/// <summary>
/// The conditions of a workflow's trigger step ("Amount >= 1000; CurrencyCode = USD"). Every scalar
/// field of the document kind can be tested, so the fields offered and the fields evaluated are the
/// same list, read from the document class itself.
/// </summary>
public static class WorkflowConditions
{
    /// <summary>The pseudo field holding the approval amount (excluding VAT) of every document kind.</summary>
    public const string AmountField = "Amount";

    private static readonly string[] Operators = { ">=", "<=", "<>", "!=", ">", "<", "=", " contains " };

    /// <summary>Bookkeeping fields no condition is meant to test.</summary>
    private static readonly HashSet<string> Hidden = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "TenantId", "CompanyId", "ConcurrencyStamp", "ExtraProperties", "IsDeleted",
        "CreatorId", "LastModifierId", "DeleterId", "CreationTime", "LastModificationTime", "DeletionTime",
        "HasLines", "ApprovalAmount",
    };

    public static Type DocumentType(ApprovalDocumentKind kind) => kind switch
    {
        ApprovalDocumentKind.SalesDocument => typeof(SalesHeader),
        ApprovalDocumentKind.PurchaseDocument => typeof(PurchaseHeader),
        ApprovalDocumentKind.PaymentVoucher => typeof(PaymentVoucherHeader),
        ApprovalDocumentKind.PurchaseRequisition => typeof(PurchaseRequisition),
        ApprovalDocumentKind.Tender => typeof(TenderHeader),
        ApprovalDocumentKind.LeaveApplication => typeof(LeaveApplication),
        _ => null,
    };

    /// <summary>Every field a condition on this document kind can test: the amount first, then the rest by name.</summary>
    public static IReadOnlyList<WorkflowConditionField> FieldsOf(ApprovalDocumentKind kind)
    {
        var fields = new List<WorkflowConditionField>
        {
            new(AmountField, "Amount (Excl. VAT)", WorkflowConditionFieldType.Number, Array.Empty<string>()),
        };

        var type = DocumentType(kind);
        if (type == null)
        {
            return fields;
        }

        foreach (var property in ScalarProperties(type).OrderBy(p => p.Name, StringComparer.Ordinal))
        {
            if (property.Name.Equals(AmountField, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            var valueType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            fields.Add(new WorkflowConditionField(
                property.Name,
                Caption(property.Name),
                TypeOf(valueType),
                valueType.IsEnum ? Enum.GetNames(valueType) : Array.Empty<string>()));
        }

        return fields;
    }

    /// <summary>Whether the document meets one rule, "Field operator value". A rule it cannot read passes.</summary>
    public static bool Evaluate(string rule, IApprovalDocument document)
    {
        if (string.IsNullOrWhiteSpace(rule) || document == null)
        {
            return true;
        }

        string op = null, left = null, right = null;
        foreach (var candidate in Operators)
        {
            var index = rule.IndexOf(candidate, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                op = candidate.Trim().ToLowerInvariant();
                left = rule[..index].Trim();
                right = rule[(index + candidate.Length)..].Trim().Trim('\'', '"', ' ');
                break;
            }
        }
        if (op == null || string.IsNullOrEmpty(left))
        {
            return true;
        }

        var key = Normalize(left);
        if (key == Normalize(AmountField))
        {
            return CompareValue(document.ApprovalAmount, op, right);
        }

        var property = ScalarProperties(document.GetType()).FirstOrDefault(p => Normalize(p.Name) == key);
        if (property == null)
        {
            // Older rules named the amount including VAT loosely ("Amount Incl. VAT").
            if (key.Contains("amount") && key.Contains("vat"))
            {
                property = ScalarProperties(document.GetType()).FirstOrDefault(p => Normalize(p.Name) == "totalamountincludingvat");
            }
            if (property == null)
            {
                return true;
            }
        }

        return CompareValue(property.GetValue(document), op, right);
    }

    private static IEnumerable<PropertyInfo> ScalarProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && !Hidden.Contains(p.Name) && IsScalar(p.PropertyType));

    private static bool IsScalar(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type == typeof(string) || type == typeof(bool) || type == typeof(DateTime) || type.IsEnum || IsNumeric(type);
    }

    private static bool IsNumeric(Type type) =>
        type == typeof(decimal) || type == typeof(int) || type == typeof(long) || type == typeof(double)
        || type == typeof(float) || type == typeof(short);

    private static WorkflowConditionFieldType TypeOf(Type type)
    {
        if (type.IsEnum) return WorkflowConditionFieldType.Option;
        if (type == typeof(bool)) return WorkflowConditionFieldType.Boolean;
        if (type == typeof(DateTime)) return WorkflowConditionFieldType.Date;
        return IsNumeric(type) ? WorkflowConditionFieldType.Number : WorkflowConditionFieldType.Text;
    }

    private static bool CompareValue(object actual, string op, string target)
    {
        switch (actual)
        {
            case null:
                return CompareStrings(string.Empty, op, target);
            case bool flag:
                return bool.TryParse(target, out var wanted) || TryYesNo(target, out wanted)
                    ? Ordered(flag.CompareTo(wanted), op)
                    : true;
            case DateTime date:
                return DateTime.TryParse(target, CultureInfo.InvariantCulture, DateTimeStyles.None, out var when)
                    ? Ordered(date.Date.CompareTo(when.Date), op)
                    : true;
            case Enum value:
                if (int.TryParse(target, out var number))
                {
                    return Ordered(Convert.ToInt32(value).CompareTo(number), op);
                }
                return CompareStrings(value.ToString(), op, target);
            case string text:
                return CompareStrings(text, op, target);
        }

        if (IsNumeric(actual.GetType()))
        {
            return decimal.TryParse(target, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount)
                ? Ordered(Convert.ToDecimal(actual, CultureInfo.InvariantCulture).CompareTo(amount), op)
                : true;
        }

        return CompareStrings(actual.ToString(), op, target);
    }

    private static bool TryYesNo(string text, out bool value)
    {
        value = text.Equals("yes", StringComparison.OrdinalIgnoreCase);
        return value || text.Equals("no", StringComparison.OrdinalIgnoreCase);
    }

    private static bool Ordered(int comparison, string op) => op switch
    {
        ">=" => comparison >= 0,
        "<=" => comparison <= 0,
        ">" => comparison > 0,
        "<" => comparison < 0,
        "=" => comparison == 0,
        "!=" or "<>" => comparison != 0,
        _ => true,
    };

    private static bool CompareStrings(string actual, string op, string target)
    {
        actual ??= string.Empty;
        return op switch
        {
            "contains" => actual.Contains(target, StringComparison.OrdinalIgnoreCase),
            "=" => string.Equals(actual, target, StringComparison.OrdinalIgnoreCase),
            "!=" or "<>" => !string.Equals(actual, target, StringComparison.OrdinalIgnoreCase),
            _ => Ordered(string.Compare(actual, target, StringComparison.OrdinalIgnoreCase), op),
        };
    }

    private static string Normalize(string name) =>
        new string(name.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    /// <summary>"BuyFromVendorNo" → "Buy From Vendor No."</summary>
    private static string Caption(string name)
    {
        var words = Regex.Replace(name, "(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ");
        return words.EndsWith(" No", StringComparison.Ordinal) || words == "No" ? words + "." : words;
    }
}
