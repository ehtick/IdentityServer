// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using MvcCode;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Duende.IdentityModel", LogEventLevel.Debug)
    .MinimumLevel.Override("MvcCode", LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
    .CreateLogger();

try
{
    var builder = WebApplication
        .CreateBuilder(args);

    builder
        .AddServiceDefaults();

    builder
        .ConfigureServices()
        .ConfigurePipeline()
        .Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, messageTemplate: "Unhandled exception");
}
finally
{
    Log.Information(messageTemplate: "Shut down complete");
    Log.CloseAndFlush();
}
