// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using Duende.Bff.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Duende.Bff.Configuration;

/// <summary>
/// Options for BFF
/// </summary>
public sealed class BffOptions
{
    /// <summary>
    /// Base path for management endpoints. Defaults to "/bff".
    /// </summary>
    public PathString ManagementBasePath { get; set; } = "/bff";

    /// <summary>
    /// Flag that specifies if the *sid* claim needs to be present in the logout request as query string parameter. 
    /// Used to prevent cross site request forgery.
    /// Defaults to true.
    /// </summary>
    public bool RequireLogoutSessionId { get; set; } = true;

    /// <summary>
    /// Specifies if the user's refresh token is automatically revoked at logout time.
    /// Defaults to true.
    /// </summary>
    public bool RevokeRefreshTokenOnLogout { get; set; } = true;

    /// <summary>
    /// Specifies if during backchannel logout all matching user sessions are logged out.
    /// If true, all sessions for the subject will be revoked. If false, just the specific 
    /// session will be revoked.
    /// Defaults to false.
    /// </summary>
    public bool BackchannelLogoutAllUserSessions { get; set; }

    /// <summary>
    /// Specifies the name of the header used for anti-forgery header protection.
    /// Defaults to X-CSRF.
    /// </summary>
    public string AntiForgeryHeaderName { get; set; } = "X-CSRF";

    /// <summary>
    /// Specifies the expected value of the anti-forgery header.
    /// Defaults to 1.
    /// </summary>
    public string AntiForgeryHeaderValue { get; set; } = "1";

    /// <summary>
    /// Specifies if the management endpoints check that the BFF middleware is added to the pipeline.
    /// </summary>
    public bool EnforceBffMiddleware { get; set; } = true;

    /// <summary>
    /// License key
    /// </summary>
    public string? LicenseKey { get; set; }

    /// <summary>
    /// Login endpoint
    /// </summary>
    public PathString LoginPath => ManagementBasePath.Add(Constants.ManagementEndpoints.Login);

    /// <summary>
    /// Silent login endpoint
    /// </summary>
    [Obsolete("The silent login endpoint will be removed in a future version. Silent login is now handled by passing the prompt=none parameter to the login endpoint.")]
    public PathString SilentLoginPath => ManagementBasePath.Add(Constants.ManagementEndpoints.SilentLogin);

    /// <summary>
    /// Silent login callback endpoint
    /// </summary>
    public PathString SilentLoginCallbackPath => ManagementBasePath.Add(Constants.ManagementEndpoints.SilentLoginCallback);

    /// <summary>
    /// Logout endpoint
    /// </summary>
    public PathString LogoutPath => ManagementBasePath.Add(Constants.ManagementEndpoints.Logout);

    /// <summary>
    /// User endpoint
    /// </summary>
    public PathString UserPath => ManagementBasePath.Add(Constants.ManagementEndpoints.User);

    /// <summary>
    /// Back channel logout endpoint
    /// </summary>
    public PathString BackChannelLogoutPath => ManagementBasePath.Add(Constants.ManagementEndpoints.BackChannelLogout);

    /// <summary>
    /// Diagnostics endpoint
    /// </summary>
    public PathString DiagnosticsPath => ManagementBasePath.Add(Constants.ManagementEndpoints.Diagnostics);

    /// <summary>
    /// Interval at which expired sessions are cleaned up.
    /// Defaults to 10 minutes.
    /// </summary>
    public TimeSpan SessionCleanupInterval { get; set; } = TimeSpan.FromMinutes(10);

    ///// <summary>
    ///// Batch size expired sessions are deleted.
    ///// Defaults to 100.
    ///// </summary>
    //public int SessionCleanupBatchSize { get; set; } = 100;

    /// <summary>
    /// Controls the response behavior from the ~/bff/user endpoint when the user is anonymous.
    /// Defaults to Response401.
    /// </summary>
    public AnonymousSessionResponse AnonymousSessionResponse { get; set; } = AnonymousSessionResponse.Response401;

    /// <summary>
    /// The ASP.NET environment names that enable the diagnostics endpoint.
    /// Defaults to "Development".
    /// </summary>
    public ICollection<string> DiagnosticsEnvironments { get; } = new HashSet<string>()
    {
        Environments.Development
    };

    /// <summary>
    /// The Json Web Key to use when creating DPoP proof tokens. Defaults to
    /// null, which is appropriate when not using DPoP.
    /// </summary>
    public DPoPProofKey? DPoPJsonWebKey { get; set; }

    /// <summary>
    /// Flag that specifies if a user session should be removed after an attempt to use a Refresh Token to acquire
    /// a new Access Token fails. This behavior is only triggered when proxying requests to remote
    /// APIs with TokenType.User or TokenType.UserOrClient. Defaults to True. 
    /// </summary>
    public bool RemoveSessionAfterRefreshTokenExpiration { get; set; } = true;

    /// <summary>
    /// A delegate that determines if the anti-forgery check should be disabled for a given request.
    /// The default is not to disable anti-forgery checks.
    /// </summary>
    public DisableAntiForgeryCheck DisableAntiForgeryCheck { get; set; } = (c) => false;

    internal Action<OpenIdConnectOptions>? ConfigureOpenIdConnectDefaults { get; set; }
    internal Action<CookieAuthenticationOptions>? ConfigureCookieDefaults { get; set; }

    public HttpMessageHandler? BackchannelHttpHandler { get; set; }

    public string? IndexHtmlClientName { get; set; }

    /// <summary>
    /// Indicates if we should automatically register the BFF middleware in the pipeline.
    /// </summary>
    public bool AutomaticallyRegisterBffMiddleware { get; set; } = true;

    internal List<LoadPluginMiddlewares> MiddlewareLoaders { get; set; } = [];

    /// <summary>
    /// When using silent login in a split host scenario (where the api and bff are hosted on different domains),
    /// then the silent login script needs to be able to know which window to post the message to.
    /// In this case, we use the referer header, but this is a potential security risk. So, we check against a list of allowed referers.
    ///
    /// Note, referer is actually an incorrect spelling of the word referrer, but it is the spelling used in the HTTP specification.
    /// https://en.wikipedia.org/wiki/HTTP_referer
    /// </summary>
    public Collection<string> AllowedSilentLoginReferers { get; } = new();
}
