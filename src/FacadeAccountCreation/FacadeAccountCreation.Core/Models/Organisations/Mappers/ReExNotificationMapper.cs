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
            CompanyName = organisationModel.Company.CompanyName,
            CompanyHouseNumber = organisationModel.Company.CompaniesHouseNumber,
            ReExInvitedApprovedPersons = GetApprvedPersonList(organisationModel.InvitedApprovedPersons, response.InvitedApprovedUsers, accountCreationUrl)            
        };
    }

    private static List<ReExInvitedApprovedPerson> GetApprvedPersonList(IEnumerable<ReExInvitedApprovedPerson> reExInvitedPeople,  IEnumerable<InvitedApprovedUserResponse> approvedListResponse, string accountCreationUrl)
    {
        List<ReExInvitedApprovedPerson> approvedList = [];

        foreach (var approvedUser in reExInvitedPeople)
        {
            string invitedToken = approvedListResponse.FirstOrDefault(x => x.Email == approvedUser.Email)?.InviteToken;
           
            if(string.IsNullOrWhiteSpace(invitedToken))
            {
                continue;
            }

            var inviteLink = $"{accountCreationUrl}{HttpUtility.UrlEncode(invitedToken)}";
            approvedUser.InviteToken = inviteLink;
            approvedList.Add(approvedUser);
        }
        return approvedList;
    }
}

