using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using Volo.Abp;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// Turns the filter lines a caller sends into a predicate over the entity.
/// <para>
/// Only the operators below are ever built, and every value is parsed into the field's own type
/// first, so nothing the caller sends reaches the database as text to be interpreted.
/// </para>
/// </summary>
public static class EntityFilterExpressionBuilder
{
    public static Expression<Func<TEntity, bool>> Build<TEntity>(
        ErpEntityDefinition definition,
        IReadOnlyList<EntityFilter> filters
    )
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        Expression body = null;

        foreach (var filter in filters ?? Array.Empty<EntityFilter>())
        {
            var field = definition.GetField(filter.Field);
            var condition = BuildCondition(parameter, field, filter);
            body = body == null ? condition : Expression.AndAlso(body, condition);
        }

        body ??= Expression.Constant(true);

        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }

    private static Expression BuildCondition(ParameterExpression parameter, ErpEntityField field, EntityFilter filter)
    {
        var member = Expression.Property(parameter, field.Property);

        if (filter.Operator is EntityFilterOperator.Contains or EntityFilterOperator.StartsWith)
        {
            if (field.ClrType != typeof(string))
            {
                throw NotSupported(field, filter.Operator);
            }

            return BuildTextCondition(member, filter);
        }

        var value = ConvertValue(field, filter.Value);
        var constant = Expression.Constant(value, field.Property.PropertyType);

        return filter.Operator switch
        {
            EntityFilterOperator.Equals => Expression.Equal(member, constant),
            EntityFilterOperator.NotEquals => Expression.NotEqual(member, constant),
            EntityFilterOperator.GreaterThan => Comparison(member, constant, field, filter, Expression.GreaterThan),
            EntityFilterOperator.GreaterOrEqual => Comparison(member, constant, field, filter, Expression.GreaterThanOrEqual),
            EntityFilterOperator.LessThan => Comparison(member, constant, field, filter, Expression.LessThan),
            EntityFilterOperator.LessOrEqual => Comparison(member, constant, field, filter, Expression.LessThanOrEqual),
            _ => throw NotSupported(field, filter.Operator),
        };
    }

    /// <summary>
    /// Both sides are lower-cased so that a search behaves the same whichever database is
    /// underneath; PostgreSQL compares text case-sensitively and SQLite does not.
    /// </summary>
    private static Expression BuildTextCondition(MemberExpression member, EntityFilter filter)
    {
        var toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
        var method =
            filter.Operator == EntityFilterOperator.Contains
                ? typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!
                : typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;

        var left = Expression.Call(
            Expression.Coalesce(member, Expression.Constant(string.Empty)),
            toLower
        );

        var right = Expression.Constant((filter.Value ?? string.Empty).ToLowerInvariant());

        return Expression.Call(left, method, right);
    }

    private static Expression Comparison(
        Expression member,
        Expression constant,
        ErpEntityField field,
        EntityFilter filter,
        Func<Expression, Expression, BinaryExpression> build
    )
    {
        if (field.ClrType == typeof(string) || field.ClrType == typeof(bool) || field.ClrType == typeof(Guid))
        {
            throw NotSupported(field, filter.Operator);
        }

        return build(member, constant);
    }

    private static object ConvertValue(ErpEntityField field, string value)
    {
        if (value.IsNullOrWhiteSpace())
        {
            if (!field.IsNullable)
            {
                throw Invalid(field, value);
            }

            return null;
        }

        var text = value.Trim();

        try
        {
            if (field.ClrType.IsEnum)
            {
                return Enum.Parse(field.ClrType, text, ignoreCase: true);
            }

            if (field.ClrType == typeof(Guid))
            {
                return Guid.Parse(text);
            }

            if (field.ClrType == typeof(DateTime))
            {
                return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }

            if (field.ClrType == typeof(DateTimeOffset))
            {
                return DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            }

            if (field.ClrType == typeof(TimeSpan))
            {
                return TimeSpan.Parse(text, CultureInfo.InvariantCulture);
            }

            return Convert.ChangeType(text, field.ClrType, CultureInfo.InvariantCulture);
        }
        catch (Exception exception) when (exception is FormatException or OverflowException or ArgumentException)
        {
            throw Invalid(field, value);
        }
    }

    private static BusinessException Invalid(ErpEntityField field, string value)
    {
        return new BusinessException(ErpErrorCodes.Exporting.FilterValueNotValid)
            .WithData("fieldName", field.Name)
            .WithData("value", value ?? string.Empty);
    }

    private static BusinessException NotSupported(ErpEntityField field, EntityFilterOperator op)
    {
        return new BusinessException(ErpErrorCodes.Exporting.OperatorNotSupportedForField)
            .WithData("fieldName", field.Name)
            .WithData("operator", op.ToString());
    }
}
