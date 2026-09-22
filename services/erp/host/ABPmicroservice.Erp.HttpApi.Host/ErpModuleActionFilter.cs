using System.Threading.Tasks;
using ABPmicroservice.Erp.Modules;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;

namespace ABPmicroservice.Erp;

/// <summary>
/// Closes the endpoints of a module this company has switched off.
/// <para>
/// Hiding a module's menu entries would only be a presentation change: anyone holding the URL, or
/// any integration holding a token, would still reach it. Switching a module off has to mean the
/// same thing to the API as it does to the screen, so every request is checked here.
/// </para>
/// <para>
/// The module is found from the controller's namespace, so a new application service is covered
/// the moment it is added, with no list to keep in step.
/// </para>
/// </summary>
public class ErpModuleActionFilter : IAsyncActionFilter, ITransientDependency
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            var module = ErpModuleRegistry.ForType(descriptor.ControllerTypeInfo.AsType());

            // Core modules are always on, and anything outside the ERP service is not ours to gate.
            if (module is { IsCore: false })
            {
                await context
                    .HttpContext.RequestServices.GetRequiredService<ErpModuleManager>()
                    .EnsureEnabledAsync(module.Code);
            }
        }

        await next();
    }
}
