// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff.Tests.TestFramework;
using Duende.Bff.Tests.TestInfra;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.HttpOverrides;
using Xunit.Abstractions;

namespace Duende.Bff.Tests.Headers;

public class ApiUseForwardedHeaders : BffTestBase
{
    public ApiUseForwardedHeaders(ITestOutputHelper output) : base(output)
    {
        Bff.OnConfigureBff += bff => bff.AddRemoteApis();

        Api.OnConfigureApp += app =>
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                   ForwardedHeaders.XForwardedProto |
                                   ForwardedHeaders.XForwardedHost
            });
        };

        Bff.OnConfigureApp += app =>
        {
            app.MapRemoteBffApiEndpoint(The.Path, Api.Url());
        };
    }

    [Fact]
    public async Task bff_host_name_should_propagate_to_api()
    {
        await InitializeAsync();

        ApiCallDetails apiResult = await Bff.BrowserClient.CallBffHostApi(The.PathAndSubPath);

        var host = apiResult.RequestHeaders["Host"].Single();
        host.ShouldBe(Bff.Url().Host);
    }

    [Fact]
    public async Task forwarded_host_name_should_not_propagate_to_api()
    {
        await InitializeAsync();

        ApiCallDetails apiResult = await Bff.BrowserClient.CallBffHostApi(The.PathAndSubPath,
            headers: new()
            {
                ["x-csrf"] = "1",
                ["X-Forwarded-Host"] = "external"
            });

        var host = apiResult.RequestHeaders["Host"].Single();
        host.ShouldBe(Bff.Url().Host);
    }
}
