﻿namespace FacadeAccountCreation.Core.Models.Enrolments;

public class DeleteUserModel
{
    public Guid PersonExternalIdToDelete { get; set; }
    public Guid LoggedInUserId { get; set; }
    public Guid OrganisationId { get; set; }
    public int ServiceRoleId { get; set; }
}
