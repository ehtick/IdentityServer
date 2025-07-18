// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff.SessionManagement.SessionStore;

namespace Duende.Bff.EntityFramework;

/// <summary>
/// Entity class for a user session
/// </summary>
public class UserSessionEntity : UserSession
{
    /// <summary>
    /// Id for record in database
    /// </summary>
    public long Id { get; set; }
}
