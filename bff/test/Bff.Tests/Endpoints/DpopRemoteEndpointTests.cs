// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff.AccessTokenManagement;
using Duende.Bff.Tests.TestFramework;
using Duende.Bff.Tests.TestInfra;
using Duende.Bff.Yarp;
using Xunit.Abstractions;

namespace Duende.Bff.Tests.Endpoints;

public class DpopRemoteEndpointTests(ITestOutputHelper output) : BffTestBase(output), IAsyncLifetime
{
    public override async Task InitializeAsync()
    {
        var idSrvClient = IdentityServer.AddClient(The.ClientId, Bff.Url());

        idSrvClient.RequireDPoP = true;

        Bff.OnConfigureBff += bff => bff.AddRemoteApis();
        Bff.OnConfigureApp += app =>
        {
            app.MapRemoteBffApiEndpoint(The.Path, Api.Url())
                .WithAccessToken(RequiredTokenType.Client);
        };

        await base.InitializeAsync();
        Bff.BffOptions.DPoPJsonWebKey = The.DPoPJsonWebKey;
        Bff.BffOptions.ConfigureOpenIdConnectDefaults = opt =>
        {
            opt.BackchannelHttpHandler = Internet;
            The.DefaultOpenIdConnectConfiguration(opt);
        };
    }

    [Fact]
    public async Task Can_login_with_dpop_enabled() => await Bff.BrowserClient.Login()
        .CheckHttpStatusCode();

    [Fact]
    public async Task When_calling_api_endpoint_with_dpop_enabled_then_dpop_headers_are_sent()
    {
        ApiCallDetails callToApi = await Bff.BrowserClient.CallBffHostApi(
            url: Bff.Url(The.PathAndSubPath)
        );

        callToApi.RequestHeaders["DPoP"].First().ShouldNotBeNullOrEmpty();
        callToApi.RequestHeaders["Authorization"].First().StartsWith("DPoP ").ShouldBeTrue();
    }
}
