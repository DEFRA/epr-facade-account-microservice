﻿using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.User;
[ExcludeFromCodeCoverage]
public class UpdateUserDetailsResponse
{
    public bool HasTelephoneOnlyUpdated { get; set; } = false;

    public bool HasBasicUserDetailsUpdated { get; set; } = false;

    public bool HasApprovedOrDelegatedUserDetailsSentForApproval { get; set; } = false;

    public ChangeHistoryModel? ChangeHistory { get; set; }
}