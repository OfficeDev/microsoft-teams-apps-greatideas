// <copyright file="MustBeValidUserRequirement.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Authentication
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// This class is an authorization policy requirement.
    /// It specifies that an user is a member of a certain team.
    /// </summary>
    public class MustBeValidUserRequirement : IAuthorizationRequirement
    {
    }
}
