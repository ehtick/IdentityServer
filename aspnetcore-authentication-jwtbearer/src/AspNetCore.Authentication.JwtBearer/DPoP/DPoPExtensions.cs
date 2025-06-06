// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Text.Json;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Duende.AspNetCore.Authentication.JwtBearer.DPoP;

/// <summary>
/// Extensions methods for DPoP
/// </summary>
internal static class DPoPExtensions
{
    public static string? GetAuthorizationScheme(this HttpRequest request) => request.Headers.Authorization.FirstOrDefault()?.Split(' ', System.StringSplitOptions.RemoveEmptyEntries)[0];

    public static string? GetDPoPProofToken(this HttpRequest request) => request.Headers[OidcConstants.HttpHeaders.DPoP].FirstOrDefault();

    public static string? GetDPoPNonce(this AuthenticationProperties props)
    {
        if (props.Items.ContainsKey("DPoP-Nonce"))
        {
            return props.Items["DPoP-Nonce"];
        }
        return null;
    }
    public static void SetDPoPNonce(this AuthenticationProperties props, string nonce) => props.Items["DPoP-Nonce"] = nonce;

    /// <summary>
    /// Create the value of a thumbprint-based cnf claim
    /// </summary>
    public static string CreateThumbprintCnf(this JsonWebKey jwk)
    {
        var jkt = jwk.CreateThumbprint();
        var values = new Dictionary<string, string>
        {
            { JwtClaimTypes.ConfirmationMethods.JwkThumbprint, jkt }
        };
        return JsonSerializer.Serialize(values);
    }

    /// <summary>
    /// Create the value of a thumbprint
    /// </summary>
    public static string CreateThumbprint(this JsonWebKey jwk) => Base64Url.Encode(jwk.ComputeJwkThumbprint());
}
