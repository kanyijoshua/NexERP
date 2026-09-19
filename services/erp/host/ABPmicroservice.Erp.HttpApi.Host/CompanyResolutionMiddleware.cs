using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Microsoft.AspNetCore.Http;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace ABPmicroservice.Erp;

/// <summary>
/// Sets the ambient company for the request from the X-Company-Id header.
/// Must run after UseMultiTenancy (companies are looked up inside the tenant)
/// and after UseAuthentication.
/// </summary>
public class CompanyResolutionMiddleware : IMiddleware, ITransientDependency
{
    public const string HeaderName = "X-Company-Id";

    private readonly CompanyResolver _companyResolver;
    private readonly ICurrentCompany _currentCompany;

    public CompanyResolutionMiddleware(CompanyResolver companyResolver, ICurrentCompany currentCompany)
    {
        _companyResolver = companyResolver;
        _currentCompany = currentCompany;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Only the ERP API is company-scoped; swagger, health checks and ABP's own endpoints are not.
        if (!context.Request.Path.StartsWithSegments("/api/erp"))
        {
            await next(context);
            return;
        }

        Guid? requestedCompanyId = null;
        if (context.Request.Headers.TryGetValue(HeaderName, out var header) && !string.IsNullOrWhiteSpace(header))
        {
            if (!Guid.TryParse(header, out var parsed))
            {
                await WriteErrorAsync(
                    context,
                    StatusCodes.Status400BadRequest,
                    ErpErrorCodes.Companies.CompanyNotFound,
                    $"{HeaderName} must be a GUID."
                );
                return;
            }

            requestedCompanyId = parsed;
        }

        BasicCompanyInfo company;
        try
        {
            company = await _companyResolver.ResolveAsync(requestedCompanyId);
        }
        catch (BusinessException ex) when (ex.Code == ErpErrorCodes.Companies.CompanyNotFound)
        {
            // Thrown before MVC runs, so ABP's exception filter would not shape it.
            await WriteErrorAsync(
                context,
                StatusCodes.Status404NotFound,
                ex.Code,
                "The selected company does not exist or is not available to you."
            );
            return;
        }

        using (_currentCompany.Change(company?.Id, company?.Name))
        {
            await next(context);
        }
    }

    // Same envelope ABP uses for remote service errors, so clients handle it uniformly.
    private static Task WriteErrorAsync(HttpContext context, int statusCode, string code, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.Headers["_AbpErrorFormat"] = "true";
        return context.Response.WriteAsJsonAsync(new { error = new { code, message } });
    }
}
