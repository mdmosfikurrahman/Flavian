using System.Security.Claims;
using Flavian.Application.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Http;

namespace Flavian.Application.Services.Implementations.Auth;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public string? AuthorizationHeader =>
        httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid PartnerId => GetClaimAsGuid("PartnerId");
    public Guid? BasePartnerId => GetOptionalClaimAsGuid("BasePartnerId");
    public string PartnerType => GetClaim("PartnerType");
    public string PartnerFullName => GetClaim("PartnerFullName");

    public string UserName => GetClaim(ClaimTypes.Name);
    public string Email => GetClaim(ClaimTypes.Email);

    public string RoleName => GetClaim(ClaimTypes.Role);
    public Guid RoleId => GetClaimAsGuid("RoleId");

    public Guid GroupId => GetClaimAsGuid("GroupId");
    public string GroupName => GetClaim("GroupName");

    public bool IsAdmin => RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    public bool IsSuperAdmin => RoleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);

    public bool IsInRole(string roleName) =>
        RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase);

    public bool IsInGroup(string groupName) =>
        GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase);

    private string GetClaim(string claimType) =>
        User?.FindFirst(claimType)?.Value ?? string.Empty;

    private Guid GetClaimAsGuid(string claimType) =>
        Guid.TryParse(GetClaim(claimType), out var value) ? value : Guid.Empty;

    private Guid? GetOptionalClaimAsGuid(string claimType)
    {
        var value = GetClaim(claimType);
        return Guid.TryParse(value, out var guid) ? guid : null;
    }
}
