// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using IdentityServer;
using IdentityServerHost;
using Serilog;

internal static class Extensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRazorPages();

        var isBuilder = builder.Services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.EmitStaticAudienceClaim = true;
            })
            .AddTestUsers(TestUsers.Users);

        // in-memory, code config
        isBuilder.AddInMemoryIdentityResources(Config.IdentityResources);
        isBuilder.AddInMemoryApiScopes(Config.ApiScopes);
        isBuilder.AddClientStore<ServiceDiscoveringClientStore>();
        isBuilder.AddInMemoryApiResources(Config.ApiResources);
        isBuilder.AddExtensionGrantValidator<TokenExchangeGrantValidator>();

        return builder.Build();
    }
    
    public static WebApplication ConfigurePipeline(this WebApplication app)
    { 
        app.UseSerilogRequestLogging();
        app.UseDeveloperExceptionPage();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();
        app.MapRazorPages()
            .RequireAuthorization();
        
        return app;
    }
}