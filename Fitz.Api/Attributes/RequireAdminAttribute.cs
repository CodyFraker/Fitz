using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Fitz.Api.Extensions;

namespace Fitz.Api.Attributes
{
    public class RequireAdminAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!context.HttpContext.User.IsAdmin())
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
