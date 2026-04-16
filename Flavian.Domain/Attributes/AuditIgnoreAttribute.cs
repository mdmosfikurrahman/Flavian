namespace Flavian.Domain.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class AuditIgnoreAttribute : Attribute;