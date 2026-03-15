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

        var loginRepository = context.HttpContext.RequestServices.GetRequiredService<ILoginRepository>();
        var login = await loginRepository.GetByTokenAsync(token);

        if (login == null)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid token." });
            return;
        }

        var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
        var user = await userRepository.GetByUserIdAsync(login.UserId);

        context.HttpContext.Items["Login"] = login;
        context.HttpContext.Items["User"] = user;
    }
}
