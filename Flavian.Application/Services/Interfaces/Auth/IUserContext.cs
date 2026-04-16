namespace Flavian.Application.Services.Interfaces.Auth;

public interface IUserContext
{
    string? AuthorizationHeader { get; }
    bool IsAuthenticated { get; }

    Guid PartnerId { get; }
    Guid? BasePartnerId { get; }
    string PartnerType { get; }
    string PartnerFullName { get; }

    string UserName { get; }
    string Email { get; }

    string RoleName { get; }
    Guid RoleId { get; }

    Guid GroupId { get; }
    string GroupName { get; }

    bool IsAdmin { get; }
    bool IsSuperAdmin { get; }

    bool IsInRole(string roleName);
    bool IsInGroup(string groupName);
}
