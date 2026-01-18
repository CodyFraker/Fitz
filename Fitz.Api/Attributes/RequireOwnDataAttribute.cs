using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Fitz.Api.Extensions;

namespace Fitz.Api.Attributes
{
    public class RequireOwnDataAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var authenticatedUserId = context.HttpContext.User.GetDiscordUserId();
            if (authenticatedUserId == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var routeData = context.RouteData;

            if (routeData.Values.TryGetValue("userId", out var routeUserId))
            {
                if (routeUserId != null && ulong.TryParse(routeUserId.ToString(), out var userId))
                {
                    if (userId != authenticatedUserId.Value)
                    {
                        context.Result = new ForbidResult();
                        return;
                    }
                }
            }
        }
    }
}
