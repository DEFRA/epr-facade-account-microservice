using FacadeAccountCreation.Core.Enums;
using FacadeAccountCreation.Core.Models.Organisations.OrganisationUsers;
using FacadeAccountCreation.Core.Models.ServiceRolesLookup;

namespace FacadeAccountCreation.UnitTests.Core.Models;

[TestClass]
public class OrganisationUsersMapperTests
{
    [TestMethod]
    public void ConvertToOrganisationUserModels_UpdatedWtihCorrectValue()
    {
        //Arrange
        var connectionId = Guid.NewGuid();
        var personId = Guid.NewGuid();
        var organiserUsers = new List<OrganisationUser>
        {
            new()
            {
                FirstName = "name",
                LastName = "lName",
                Email = "test@test.com",
                PersonId = personId,
                PersonRoleId = 2,
                ConnectionId = connectionId,
                Enrolments =
                [
                    new()
                    {
                        ServiceRoleId = 3,
                        EnrolmentStatus = EnrolmentStatus.Enrolled
                    }
                ]
            }
        };
        var rolesLookup = new List<ServiceRolesLookupModel>
        {
            new()
            {
                Key = "key",
                ServiceRoleId = 3,
                EnrolmentStatus = EnrolmentStatus.Enrolled,
                PersonRoleId = 2,
                InvitationTemplateId = Guid.NewGuid().ToString(),
                DescriptionKey = "description"
            }
        };

        var expectedOrganiseUserModel = new OrganisationUserModel
        {
            ConnectionId = connectionId,
            Email = "test@test.com",
            EnrolmentStatus = EnrolmentStatus.Enrolled,
            FirstName = "name",
            LastName = "lName",
            PersonRoleId = 2,
            ServiceRoleId = 3,
            ServiceRoleKey = "key",
            PersonId = personId
        };
        
        //Act
        var result = OrganisationUsersMapper.ConvertToOrganisationUserModels(organiserUsers, rolesLookup);
        
        //Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count >= 1);
        Assert.AreEqual(expectedOrganiseUserModel.ConnectionId, result[0].ConnectionId);
        Assert.AreEqual(expectedOrganiseUserModel.Email, result[0].Email);
        Assert.AreEqual(expectedOrganiseUserModel.EnrolmentStatus, result[0].EnrolmentStatus);
        Assert.AreEqual(expectedOrganiseUserModel.FirstName, result[0].FirstName);
        Assert.AreEqual(expectedOrganiseUserModel.LastName, result[0].LastName);
        Assert.AreEqual(expectedOrganiseUserModel.PersonId, result[0].PersonId);
        Assert.AreEqual(expectedOrganiseUserModel.PersonRoleId, result[0].PersonRoleId);
        Assert.AreEqual(expectedOrganiseUserModel.ServiceRoleId, result[0].ServiceRoleId);
        Assert.AreEqual(expectedOrganiseUserModel.ServiceRoleKey, result[0].ServiceRoleKey);
        Assert.AreEqual(expectedOrganiseUserModel.ServiceRoleKey, result[0].ServiceRoleKey);

    }
    
}