using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Flavian.Domain.Attributes;
using Flavian.Shared.Dto;

namespace Flavian.Shared.Utils.Audit;

public static class AuditDetailsBuilder
{
    private static readonly string[] SensitiveFieldKeywords =
    [
        "Password",
        "PasswordHash",
        "AuxData"
    ];

    public static string BuildCreateActionDetails<TEntity>(TEntity entity)
    {
        if (entity is null) return string.Empty;

        var resultBuilder = new StringBuilder();
        foreach (var propertyInfo in GetAuditEligibleProperties<TEntity>())
        {
            var value = propertyInfo.GetValue(entity);
            var formattedValue = FormatPropertyValueForAudit(propertyInfo, value);

            if (resultBuilder.Length > 0) resultBuilder.Append(", ");
            resultBuilder.Append(propertyInfo.Name).Append(" => ").Append(formattedValue);
        }

        return resultBuilder.ToString();
    }

    public static string BuildUpdateActionDetails<TEntity>(TEntity oldEntity, TEntity newEntity)
    {
        if (oldEntity is null || newEntity is null) return string.Empty;

        var resultBuilder = new StringBuilder();
        foreach (var propertyInfo in GetAuditEligibleProperties<TEntity>())
        {
            var oldValue = propertyInfo.GetValue(oldEntity);
            var newValue = propertyInfo.GetValue(newEntity);

            var oldText = FormatPropertyValueForAudit(propertyInfo, oldValue);
            var newText = FormatPropertyValueForAudit(propertyInfo, newValue);

            if (string.Equals(oldText, newText, StringComparison.Ordinal)) continue;

            if (resultBuilder.Length > 0) resultBuilder.Append(", ");
            resultBuilder.Append(propertyInfo.Name)
                .Append(" : ")
                .Append(oldText)
                .Append(" => ")
                .Append(newText);
        }

        return resultBuilder.ToString();
    }

    public static string BuildDeleteActionDetails()
    {
        return CommonStrings.DeleteMessage;
    }

    private static IEnumerable<PropertyInfo> GetAuditEligibleProperties<T>()
        => GetReadableInstanceProperties<T>()
            .Where(propertyInfo => !ShouldExcludePropertyFromAudit(propertyInfo));

    private static List<PropertyInfo> GetReadableInstanceProperties<T>()
        => typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(propertyInfo => propertyInfo.CanRead && propertyInfo.GetIndexParameters().Length == 0)
            .ToList();

    private static bool ShouldExcludePropertyFromAudit(PropertyInfo propertyInfo)
    {
        if (SensitiveFieldKeywords.Any(keyword =>
                propertyInfo.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            return true;

        if (Attribute.IsDefined(propertyInfo, typeof(AuditIgnoreAttribute), inherit: true) ||
            propertyInfo.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
            propertyInfo.Name.EndsWith("Id", StringComparison.Ordinal) ||
            Attribute.IsDefined(propertyInfo, typeof(KeyAttribute), inherit: true))
            return true;

        var propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
        var isSimple = propertyType.IsValueType || propertyType == typeof(string);
        return !isSimple;
    }

    private static string FormatPropertyValueForAudit(PropertyInfo propertyInfo, object? value)
    {
        if (propertyInfo.Name.Equals(FieldNames.IsActive, StringComparison.OrdinalIgnoreCase) &&
            value is bool boolValue)
            return boolValue ? FieldNames.AuditActive : FieldNames.AuditInactive;

        return value?.ToString() ?? "<null>";
    }
}
