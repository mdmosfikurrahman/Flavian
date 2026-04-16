namespace Flavian.Shared.Dto;

public static class CommonStrings
{
    public const string EntityDemo = "Demo";
    public const string EntityDemoAudit = "Demo Audit";

    public const string NotFoundByIdTemplate = "The requested {0}";
    public const string DeleteMessage = "This entity has been deleted.";

    public const string AuditCreateForbiddenMessage = "Manual creation of audit records is not allowed.";
    public const string AuditUpdateForbiddenMessage = "Updating audit records is not allowed.";
    public const string AuditDeleteForbiddenMessage = "Deleting audit records is not allowed.";
}
