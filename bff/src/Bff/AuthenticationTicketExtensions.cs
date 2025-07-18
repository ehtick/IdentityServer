// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Duende.Bff.Internal;
using Duende.Bff.Otel;
using Duende.Bff.SessionManagement.SessionStore;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;


namespace Duende.Bff;

/// <summary>
///  Extension methods for AuthenticationTicket
/// </summary>
public static class AuthenticationTicketExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Extracts a subject identifier
    /// </summary>
    public static string GetSubjectId(this AuthenticationTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        var subjectId = ticket.Principal.FindFirst(JwtClaimTypes.Subject)?.Value ??
                        ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        // for the mfa remember me cookie, ASP.NET Identity uses the 'name' claim for the subject id (for some reason)
                        ticket.Principal.FindFirst(ClaimTypes.Name)?.Value ??
                        throw new InvalidOperationException("Missing 'sub' claim in AuthenticationTicket");

        return subjectId;
    }

    /// <summary>
    /// Extracts the session ID
    /// </summary>
    public static string? GetSessionId(this AuthenticationTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        return ticket.Principal.FindFirst(JwtClaimTypes.SessionId)?.Value;
    }

    /// <summary>
    /// Extracts the issuance time
    /// </summary>
    public static DateTime GetIssued(this AuthenticationTicket ticket, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        return ticket.Properties.IssuedUtc?.UtcDateTime ?? utcNow.UtcDateTime;
    }

    /// <summary>
    /// Extracts the expiration time
    /// </summary>
    public static DateTime? GetExpiration(this AuthenticationTicket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        return ticket.Properties.ExpiresUtc?.UtcDateTime;
    }

    /// <summary>
    /// Serializes and AuthenticationTicket to a string
    /// </summary>
    public static string Serialize(this AuthenticationTicket ticket, IDataProtector protector)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        ArgumentNullException.ThrowIfNull(protector);
        var data = new AuthenticationTicketLite
        {
            Scheme = ticket.AuthenticationScheme,
            User = ticket.Principal.ToClaimsPrincipalLite(),
            Items = ticket.Properties.Items
        };

        var payload = JsonSerializer.Serialize(data, JsonOptions);
        payload = protector.Protect(payload);

        var envelope = new Envelope { Version = 1, Payload = payload };
        var value = JsonSerializer.Serialize(envelope, JsonOptions);

        return value;
    }

    /// <summary>
    /// Deserializes a UserSession's Ticket to an AuthenticationTicket
    /// </summary>
    public static AuthenticationTicket? Deserialize(this UserSession session, IDataProtector protector, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(protector);
        try
        {
            var envelope = JsonSerializer.Deserialize<Envelope>(session.Ticket, JsonOptions);
            if (envelope == null || envelope.Version != 1)
            {
                logger.AuthenticationTicketEnvelopeVersionInvalid(
                    LogLevel.Debug,
                    session.GetUserSessionKey());
                return null;
            }

            string payload;
            try
            {
                payload = protector.Unprotect(envelope.Payload);
            }
            catch (CryptographicException ex)
            {
                logger.AuthenticationTicketPayloadInvalid(ex, LogLevel.Warning, session.GetUserSessionKey());
                return null;
            }

            var ticket = JsonSerializer.Deserialize<AuthenticationTicketLite>(payload, JsonOptions);
            if (ticket == null)
            {
                logger.AuthenticationTicketPayloadInvalid(ex: null, LogLevel.Warning, session.GetUserSessionKey());
                return null;
            }

            var user = ticket.User.ToClaimsPrincipal();
            var properties = new AuthenticationProperties(ticket.Items);

            // this allows us to extend the session from the DB column rather than from the payload
            if (session.Expires.HasValue)
            {
                properties.ExpiresUtc = new DateTimeOffset(session.Expires.Value, TimeSpan.Zero);
            }
            else
            {
                properties.ExpiresUtc = null;
            }

            return new AuthenticationTicket(user, properties, ticket.Scheme);
        }
        catch (JsonException ex)
        {
            // failed deserialize
            logger.AuthenticationTicketFailedToDeserialize(ex, LogLevel.Warning, session.GetUserSessionKey());
        }

        return null;
    }

    /// <summary>
    /// Serialization friendly AuthenticationTicket
    /// </summary>
    internal class AuthenticationTicketLite
    {
        /// <summary>
        /// The scheme
        /// </summary>
        public string Scheme { get; set; } = default!;

        /// <summary>
        /// The user
        /// </summary>
        public ClaimsPrincipalRecord User { get; set; } = default!;

        /// <summary>
        /// The items
        /// </summary>
        public IDictionary<string, string?> Items { get; set; } = default!;
    }

    /// <summary>
    /// Envelope for serialized data
    /// </summary>
    public class Envelope
    {
        /// <summary>
        /// Version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// Payload
        /// </summary>
        public string Payload { get; set; } = default!;
    }
}
