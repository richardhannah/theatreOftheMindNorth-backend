using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TheatreOfTheMind.Models;

namespace TheatreOfTheMind.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly UserRole _requiredRole;

    public RequireRoleAttribute(UserRole role)
    {
        _requiredRole = role;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.Items["User"] as User;

        if (user == null || user.Role != _requiredRole)
        {
            context.Result = new ObjectResult(new { error = "Forbidden." }) { StatusCode = 403 };
        }
    }
}
