using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TheatreOfTheMind.Repositories;

namespace TheatreOfTheMind.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TokenAuthAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(authHeader)
            || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            || !Guid.TryParse(authHeader["Bearer ".Length..], out var token))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Missing or invalid token." });
            return;
        }

        var repository = context.HttpContext.RequestServices.GetRequiredService<ILoginRepository>();
        var login = await repository.GetByTokenAsync(token);

        if (login == null)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid token." });
            return;
        }

        context.HttpContext.Items["User"] = login;
    }
}
