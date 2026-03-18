using DMD.PERSISTENCE.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DMD.API.Configurations.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ActionValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }

            if (context.HttpContext != null && context.HttpContext.User != null)
            {
                var identity = new ClaimsIdentity(context.HttpContext.User.Identity);
                var principal = new ClaimsPrincipal(identity);
                Thread.CurrentPrincipal = principal;

                var endpoint = context.HttpContext.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
                var allowLockedClinicAccess =
                    endpoint.Contains("/login")
                    || endpoint.Contains("/register")
                    || endpoint.Contains("/api/dmd/clinic/data-privacy-status");

                if (!allowLockedClinicAccess)
                {
                    var clinicIdValue = context.HttpContext.User.FindFirstValue("clinicId");
                    if (int.TryParse(clinicIdValue, out var clinicId))
                    {
                        var dbContext = context.HttpContext.RequestServices.GetService(typeof(DmdDbContext)) as DmdDbContext;

                        if (dbContext != null)
                        {
                            var isLocked = dbContext.ClinicProfiles
                                .IgnoreQueryFilters()
                                .AsNoTracking()
                                .Any(item => item.Id == clinicId && item.IsLocked);

                            if (isLocked)
                            {
                                context.Result = new ObjectResult("Clinic account is locked.")
                                {
                                    StatusCode = StatusCodes.Status423Locked
                                };
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
