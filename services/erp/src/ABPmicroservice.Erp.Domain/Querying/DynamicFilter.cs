using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Volo.Abp;

namespace ABPmicroservice.Erp.Querying;

/// <summary>One line of a list's filter pane: a field, an operator and the value(s) it compares with.</summary>
public class DynamicFilterCondition
{
    public string Field { get; set; }

    /// <summary>
    /// contains, notContains, equals, notEquals, startsWith, endsWith, isEmpty, isNotEmpty, gt, gte,
    /// lt, lte, between, before, after, today, thisWeek, thisMonth, thisYear, isTrue, isFalse, or
    /// expression (a Business Central filter such as <c>1000..2000|3000</c>).
    /// </summary>
    public string Operator { get; set; }

    public string Value { get; set; }

    /// <summary>The upper bound of <c>between</c>.</summary>
    public string ValueTo { get; set; }
}

/// <summary>
/// The filter a list page sends with its query. Mirrors Business Central's filter pane and
/// Odoo's custom filters: any number of conditions on the list's fields, all or any of which must
/// hold. It travels as JSON in the list input's <c>DynamicFilter</c>.
/// </summary>
public class DynamicFilter
{
    public const int MaxConditions = 50;

    /// <summary>"and" (every condition) or "or" (any condition).</summary>
    public string Logic { get; set; } = "and";

    public List<DynamicFilterCondition> Conditions { get; set; } = new();

    public bool MatchAny => string.Equals(Logic, "or", StringComparison.OrdinalIgnoreCase);

    /// <summary>Reads the JSON a list page sends; blank means no filter.</summary>
    public static DynamicFilter Parse(string json)
    {
        if (json.IsNullOrWhiteSpace())
        {
            return new DynamicFilter();
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var filter = new DynamicFilter
            {
                Logic = Property(root, "logic")?.GetString() ?? "and",
            };

            if (Property(root, "conditions") is { ValueKind: JsonValueKind.Array } conditions)
            {
                foreach (var item in conditions.EnumerateArray())
                {
                    filter.Conditions.Add(new DynamicFilterCondition
                    {
                        Field = Text(Property(item, "field")),
                        Operator = Text(Property(item, "operator")),
                        Value = Text(Property(item, "value")),
                        ValueTo = Text(Property(item, "valueTo")),
                    });
                }
            }

            if (filter.Conditions.Count > MaxConditions)
            {
                throw new BusinessException(ErpErrorCodes.Querying.InvalidFilter);
            }

            return filter;
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            throw new BusinessException(ErpErrorCodes.Querying.InvalidFilter);
        }
    }

    private static JsonElement? Property(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        return null;
    }

    // Values may arrive as JSON strings, numbers or booleans; they are all compared as typed below.
    private static string Text(JsonElement? element)
    {
        return element?.ValueKind switch
        {
            JsonValueKind.String => element.Value.GetString(),
            JsonValueKind.Number => element.Value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null,
        };
    }
}

/// <summary>
/// Turns a <see cref="DynamicFilter"/> into a predicate over an entity. Fields are the entity's
/// public properties matched by name without regard to case (so a DTO's camelCase field names
/// work), or an alias a service declares. Every value is parsed into the property's own type
/// first, so nothing the caller sends reaches the database as text to be interpreted.
/// </summary>
public static class DynamicFilterBuilder
{
    private static readonly MethodInfo ToLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
    private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
    private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;
    private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!;
    private static readonly MethodInfo CompareMethod = typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string)])!;

    /// <summary>Filters <paramref name="query"/> by the JSON filter a list page sent; blank changes nothing.</summary>
    public static IQueryable<T> ApplyDynamicFilter<T>(
        this IQueryable<T> query,
        string json,
        DateTime today,
        IReadOnlyDictionary<string, string> aliases = null
    )
    {
        var filter = DynamicFilter.Parse(json);
        return filter.Conditions.Count == 0 ? query : query.Where(Build<T>(filter, today, aliases));
    }

    public static Expression<Func<T, bool>> Build<T>(
        DynamicFilter filter,
        DateTime today,
        IReadOnlyDictionary<string, string> aliases = null
    )
    {
        var parameter = Expression.Parameter(typeof(T), "e");
        Expression body = null;

        foreach (var condition in filter.Conditions)
        {
            var property = ResolveProperty(typeof(T), condition.Field, aliases);
            var member = Expression.Property(parameter, property);
            var next = new ConditionBuilder(member, condition.Field, today.Date).Build(condition.Operator, condition.Value, condition.ValueTo);

            body = body == null ? next : filter.MatchAny ? Expression.OrElse(body, next) : Expression.AndAlso(body, next);
        }

        return Expression.Lambda<Func<T, bool>>(body ?? Expression.Constant(true), parameter);
    }

    private static PropertyInfo ResolveProperty(Type type, string field, IReadOnlyDictionary<string, string> aliases)
    {
        if (field.IsNullOrWhiteSpace())
        {
            throw NotFilterable(field);
        }

        var name = field.Trim();
        if (aliases != null)
        {
            var alias = aliases.FirstOrDefault(a => string.Equals(a.Key, name, StringComparison.OrdinalIgnoreCase));
            if (alias.Value != null)
            {
                name = alias.Value;
            }
        }

        var property = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            // A property without a setter is computed in code (e.g. a full name), not a column the
            // database could filter on.
            .FirstOrDefault(p =>
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)
                && p.CanRead
                && HasSetter(p)
                && p.GetIndexParameters().Length == 0
            );

        if (property == null || !IsFilterableType(property.PropertyType))
        {
            throw NotFilterable(field);
        }

        return property;
    }

    // A base class's private setter is invisible through the derived type, so ask the declaring type.
    private static bool HasSetter(PropertyInfo property) =>
        property.SetMethod != null
        || property.DeclaringType?.GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.SetMethod != null;

    private static bool IsFilterableType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying == typeof(string)
            || underlying == typeof(bool)
            || underlying == typeof(DateTime)
            || underlying == typeof(Guid)
            || underlying.IsEnum
            || IsNumber(underlying);
    }

    private static bool IsNumber(Type type) =>
        type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte)
        || type == typeof(decimal) || type == typeof(double) || type == typeof(float);

    private static BusinessException NotFilterable(string field) =>
        new BusinessException(ErpErrorCodes.Querying.FieldNotFilterable).WithData("field", field ?? string.Empty);

    /// <summary>Builds the condition of one operator on one member, whatever its type.</summary>
    private sealed class ConditionBuilder
    {
        private readonly MemberExpression _member;
        private readonly string _field;
        private readonly DateTime _today;
        private readonly Type _type;
        private readonly Type _underlying;
        private readonly bool _nullable;

        public ConditionBuilder(MemberExpression member, string field, DateTime today)
        {
            _member = member;
            _field = field;
            _today = today;
            _type = member.Type;
            _underlying = Nullable.GetUnderlyingType(_type) ?? _type;
            _nullable = !_type.IsValueType || Nullable.GetUnderlyingType(_type) != null;
        }

        public Expression Build(string op, string value, string valueTo)
        {
            switch ((op ?? "contains").Trim())
            {
                case "isEmpty":
                    return IsEmpty();
                case "isNotEmpty":
                    return Expression.Not(IsEmpty());
                case "expression":
                    return Expression_(value);
            }

            if (_underlying == typeof(string))
            {
                return Text(op, value, valueTo);
            }

            if (_underlying == typeof(bool))
            {
                return Bool(op, value);
            }

            if (_underlying == typeof(DateTime))
            {
                return Date(op, value, valueTo);
            }

            return Ordered(op, value, valueTo);
        }

        private Expression IsEmpty()
        {
            if (_underlying == typeof(string))
            {
                return Expression.OrElse(
                    Expression.Equal(_member, Expression.Constant(null, typeof(string))),
                    Expression.Equal(_member, Expression.Constant(string.Empty))
                );
            }

            return _nullable ? Expression.Equal(_member, Expression.Constant(null, _type)) : Expression.Constant(false);
        }

        private Expression Text(string op, string value, string valueTo)
        {
            var lower = Expression.Call(Expression.Coalesce(_member, Expression.Constant(string.Empty)), ToLowerMethod);
            Expression Const(string v) => Expression.Constant((v ?? string.Empty).ToLowerInvariant());
            Expression Compare(string v) => Expression.Call(CompareMethod, lower, Const(v));

            return op switch
            {
                "contains" => Expression.Call(lower, ContainsMethod, Const(value)),
                "notContains" => Expression.Not(Expression.Call(lower, ContainsMethod, Const(value))),
                "startsWith" => Expression.Call(lower, StartsWithMethod, Const(value)),
                "endsWith" => Expression.Call(lower, EndsWithMethod, Const(value)),
                "equals" => Expression.Equal(lower, Const(value)),
                "notEquals" => Expression.NotEqual(lower, Const(value)),
                // Codes and numbers are compared as text, as BC does for a range like 1000..2000.
                "gt" => Expression.GreaterThan(Compare(value), Expression.Constant(0)),
                "gte" => Expression.GreaterThanOrEqual(Compare(value), Expression.Constant(0)),
                "lt" => Expression.LessThan(Compare(value), Expression.Constant(0)),
                "lte" => Expression.LessThanOrEqual(Compare(value), Expression.Constant(0)),
                "between" => Range(
                    value.IsNullOrEmpty() ? null : Expression.GreaterThanOrEqual(Compare(value), Expression.Constant(0)),
                    valueTo.IsNullOrEmpty() ? null : Expression.LessThanOrEqual(Compare(valueTo), Expression.Constant(0))
                ),
                _ => throw NotSupported(op),
            };
        }

        private Expression Bool(string op, string value)
        {
            var truth = op switch
            {
                "isTrue" => true,
                "isFalse" => false,
                "equals" => ParseBool(value),
                "notEquals" => !ParseBool(value),
                _ => throw NotSupported(op),
            };

            var yes = Expression.Equal(_member, Expression.Constant(true, _type));
            return truth ? yes : Expression.Not(yes);
        }

        private bool ParseBool(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "true" or "yes" or "1" => true,
                "false" or "no" or "0" => false,
                _ => throw Invalid(value),
            };
        }

        /// <summary>Numbers, enums and GUIDs: compared by value.</summary>
        private Expression Ordered(string op, string value, string valueTo)
        {
            if (_underlying == typeof(Guid) && op is not ("equals" or "notEquals"))
            {
                throw NotSupported(op);
            }

            // Enums have no ordering operators in expression trees; compare their numbers instead.
            var member = _underlying.IsEnum
                ? Expression.Convert(_member, _nullable ? typeof(long?) : typeof(long))
                : (Expression)_member;
            Expression Const(string v)
            {
                var parsed = Parse(v);
                return _underlying.IsEnum
                    ? Expression.Constant(parsed == null ? null : (object)Convert.ToInt64(parsed), member.Type)
                    : Expression.Constant(parsed, _type);
            }

            return op switch
            {
                "equals" => Expression.Equal(member, Const(value)),
                "notEquals" => Expression.NotEqual(member, Const(value)),
                "gt" => Expression.GreaterThan(member, Const(value)),
                "gte" => Expression.GreaterThanOrEqual(member, Const(value)),
                "lt" => Expression.LessThan(member, Const(value)),
                "lte" => Expression.LessThanOrEqual(member, Const(value)),
                "between" => Range(
                    value.IsNullOrEmpty() ? null : Expression.GreaterThanOrEqual(member, Const(value)),
                    valueTo.IsNullOrEmpty() ? null : Expression.LessThanOrEqual(member, Const(valueTo))
                ),
                _ => throw NotSupported(op),
            };
        }

        /// <summary>Dates compare by day: "on" a date is the whole of that day.</summary>
        private Expression Date(string op, string value, string valueTo)
        {
            Expression From(DateTime day) => Expression.GreaterThanOrEqual(_member, Expression.Constant(day, _type));
            Expression Before(DateTime day) => Expression.LessThan(_member, Expression.Constant(day, _type));
            Expression Day(DateTime day) => Expression.AndAlso(From(day), Before(day.AddDays(1)));

            // Weeks start on Monday, as in Business Central.
            var weekStart = _today.AddDays(-(((int)_today.DayOfWeek + 6) % 7));
            var monthStart = new DateTime(_today.Year, _today.Month, 1);
            var yearStart = new DateTime(_today.Year, 1, 1);

            return op switch
            {
                "equals" => Day(ParseDate(value)),
                "notEquals" => Expression.Not(Day(ParseDate(value))),
                "before" or "lt" => Before(ParseDate(value)),
                "lte" => Before(ParseDate(value).AddDays(1)),
                "after" or "gt" => From(ParseDate(value).AddDays(1)),
                "gte" => From(ParseDate(value)),
                "between" => Range(
                    value.IsNullOrEmpty() ? null : From(ParseDate(value)),
                    valueTo.IsNullOrEmpty() ? null : Before(ParseDate(valueTo).AddDays(1))
                ),
                "today" => Day(_today),
                "thisWeek" => Expression.AndAlso(From(weekStart), Before(weekStart.AddDays(7))),
                "thisMonth" => Expression.AndAlso(From(monthStart), Before(monthStart.AddMonths(1))),
                "thisYear" => Expression.AndAlso(From(yearStart), Before(yearStart.AddYears(1))),
                _ => throw NotSupported(op),
            };
        }

        /// <summary>
        /// A Business Central filter expression: terms separated by <c>|</c> (or) and <c>&amp;</c>
        /// (and); each term a value, a range <c>a..b</c>, a comparison (<c>&lt;&gt;</c>, <c>&gt;=</c>,
        /// <c>&lt;=</c>, <c>&gt;</c>, <c>&lt;</c>, <c>=</c>), a wildcard <c>*</c> on text, or
        /// <c>''</c> for blank. <c>@</c> (ignore case) is accepted; text is compared ignoring case anyway.
        /// </summary>
        private Expression Expression_(string expression)
        {
            if (expression.IsNullOrWhiteSpace())
            {
                return Expression.Constant(true);
            }

            Expression any = null;
            foreach (var alternative in expression.Split('|'))
            {
                Expression all = null;
                foreach (var part in alternative.Split('&'))
                {
                    var term = Term(part.Trim());
                    all = all == null ? term : Expression.AndAlso(all, term);
                }

                any = any == null ? all : Expression.OrElse(any, all);
            }

            return any;
        }

        private Expression Term(string term)
        {
            term = term.TrimStart('@');

            if (term is "''" or "\"\"")
            {
                return IsEmpty();
            }

            if (term.StartsWith("<>"))
            {
                var rest = term[2..];
                return rest is "''" or "" ? Expression.Not(IsEmpty()) : Expression.Not(Term(rest));
            }

            foreach (var (prefix, op) in new[] { (">=", "gte"), ("<=", "lte"), (">", "gt"), ("<", "lt"), ("=", "equals") })
            {
                if (term.StartsWith(prefix))
                {
                    return Build(op, Keyword(term[prefix.Length..].Trim()), null);
                }
            }

            var range = term.IndexOf("..", StringComparison.Ordinal);
            if (range >= 0)
            {
                return Build("between", Keyword(term[..range].Trim()), Keyword(term[(range + 2)..].Trim()));
            }

            if (_underlying == typeof(string) && term.Contains('*'))
            {
                return Wildcard(term);
            }

            return Build("equals", Keyword(term), null);
        }

        private Expression Wildcard(string pattern)
        {
            var pieces = pattern.Split('*');
            var first = pieces[0];
            var last = pieces[^1];
            var inner = pieces.Skip(1).Take(pieces.Length - 2).Where(p => p.Length > 0).ToList();

            Expression condition = null;
            void And(Expression next) => condition = condition == null ? next : Expression.AndAlso(condition, next);

            if (first.Length > 0)
            {
                And(Text("startsWith", first, null));
            }

            if (last.Length > 0)
            {
                And(Text("endsWith", last, null));
            }

            foreach (var piece in inner)
            {
                And(Text("contains", piece, null));
            }

            return condition ?? Expression.Constant(true);
        }

        // BC's date shortcut: t is today (work date).
        private string Keyword(string value) =>
            _underlying == typeof(DateTime) && value.ToLowerInvariant() is "t" or "today"
                ? _today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : value;

        private static Expression Range(Expression lower, Expression upper)
        {
            return lower == null ? upper ?? Expression.Constant(true)
                : upper == null ? lower
                : Expression.AndAlso(lower, upper);
        }

        private object Parse(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                if (_nullable)
                {
                    return null;
                }

                throw Invalid(value);
            }

            var text = value.Trim();
            try
            {
                if (_underlying.IsEnum)
                {
                    return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
                        ? Enum.ToObject(_underlying, number)
                        : Enum.Parse(_underlying, text, ignoreCase: true);
                }

                if (_underlying == typeof(Guid))
                {
                    return Guid.Parse(text);
                }

                return Convert.ChangeType(text, _underlying, CultureInfo.InvariantCulture);
            }
            catch (Exception exception) when (exception is FormatException or OverflowException or ArgumentException or InvalidCastException)
            {
                throw Invalid(value);
            }
        }

        private DateTime ParseDate(string value)
        {
            if (!value.IsNullOrWhiteSpace()
                && DateTime.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var date))
            {
                return date.Date;
            }

            throw Invalid(value);
        }

        private BusinessException Invalid(string value) =>
            new BusinessException(ErpErrorCodes.Querying.InvalidValue)
                .WithData("field", _field ?? string.Empty)
                .WithData("value", value ?? string.Empty);

        private BusinessException NotSupported(string op) =>
            new BusinessException(ErpErrorCodes.Querying.OperatorNotSupported)
                .WithData("field", _field ?? string.Empty)
                .WithData("operator", op ?? string.Empty);
    }
}
