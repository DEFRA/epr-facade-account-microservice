﻿using FacadeAccountCreation.Core.Models.CreateAccount.ReExResponse;

namespace FacadeAccountCreation.Core.Models.CreateAccount;

/// <summary>
/// Re-Ex invited approved person model
/// </summary>
public class ReExInvitedApprovedPerson
{
    /// <summary>
    ///Role/Job for approved person i.e. Director, CompanySecretary
    /// </summary>
    public string? Role { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string TelephoneNumber { get; set; }

    public string Email { get; set; }

    public string? InviteToken { get; set; }

    /// <summary>
    /// required when mapping the response from back-end for email-service
    /// </summary>
   public ServiceRoleResponse? ServiceRole { get; set; }
}
