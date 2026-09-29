using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ABPmicroservice.Gateway;

public static class OpenApiOptionsExtensions
{
    public static OpenApiOptions UseJwtBearerAuthentication(this OpenApiOptions options)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Name = JwtBearerDefaults.AuthenticationScheme,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
        };

        options.AddDocumentTransformer(
            (document, context, cancellationToken) =>
            {
                document.Components ??= new();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes.Add(
                    JwtBearerDefaults.AuthenticationScheme,
                    scheme
                );

                return Task.CompletedTask;
            }
        );

        options.AddOperationTransformer(
            (operation, context, cancellationToken) =>
            {
                if (
                    context
                        .Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>()
                        .Any()
                )
                {
                    var reference = new OpenApiSecuritySchemeReference(
                        JwtBearerDefaults.AuthenticationScheme,
                        context.Document
                    );
                    operation.Security = [new() { [reference] = [] }];
                }

                return Task.CompletedTask;
            }
        );

        return options;
    }
}
