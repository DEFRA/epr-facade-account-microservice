using System.Web;
using FacadeAccountCreation.Core.Models.CreateAccount.ReExResponse;

namespace FacadeAccountCreation.Core.Models.Organisations.Mappers;

public static class ReExNotificationMapper
{
    public static ReExNotificationModel MapOrganisationModelToReExNotificationModel(ReExOrganisationModel organisationModel, ReExAddOrganisationResponse response, string accountCreationUrl)
    {
        return new ReExNotificationModel
        { 
            UserId = organisationModel.ReExUser.UserId.Value.ToString(),
            UserFirstName = response.UserFirstName,
            UserLastName = response.UserLastName,
            UserEmail = organisationModel.ReExUser.UserEmail,
            OrganisationId = response.OrganisationId.ToString(),
            OrganisationExternalId = string.Empty,
            CompanyName = GetName(organisationModel),
            CompanyHouseNumber = GetNumber(organisationModel, response.ReferenceNumber),
            ReExInvitedApprovedPersons = GetApprvedPersonList(organisationModel.InvitedApprovedPersons, response.InvitedApprovedUsers, accountCreationUrl)            
        };
    }

    private static string GetName(ReExOrganisationModel organisationModel)
    {
        var name = string.Empty;
        if (organisationModel?.Company is not null)
        {
            name = organisationModel.Company.CompanyName;
        }
        else if (organisationModel?.ManualInput is not null)
        {
            name = organisationModel.ManualInput.OrganisationName;
        }
        return name;
    }

    private static string GetNumber(ReExOrganisationModel organisationModel, string referenceNumber)
    {
        var referenceNo = string.Empty;
        if (organisationModel?.Company is not null)
        {
            referenceNo = organisationModel.Company.CompaniesHouseNumber;
        }
        else if (organisationModel?.ManualInput is not null)
        {
            referenceNo = referenceNumber;
        }
        return referenceNo;
    }

    private static List<ReExInvitedApprovedPerson> GetApprvedPersonList(IEnumerable<ReExInvitedApprovedPerson> reExInvitedPeople,  IEnumerable<InvitedApprovedUserResponse> approvedListResponse, string accountCreationUrl)
    {
        List<ReExInvitedApprovedPerson> approvedList = [];

        foreach (var approvedUser in reExInvitedPeople)
        {
            var invitedApprovedUser = approvedListResponse.FirstOrDefault(x => x.Email == approvedUser.Email);

            if (invitedApprovedUser != null)
            {
                if (string.IsNullOrWhiteSpace(invitedApprovedUser.InviteToken))
                {
                    continue;
                }

                var inviteLink = $"{accountCreationUrl}{HttpUtility.UrlEncode(invitedApprovedUser.InviteToken)}";
                approvedUser.InviteToken = inviteLink;
                approvedUser.ServiceRole = invitedApprovedUser.ServiceRole;
                approvedList.Add(approvedUser);
            }
        }
        return approvedList;
    }
}

