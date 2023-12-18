using System.Net;
using FacadeAccountCreation.API.Controllers;
using FacadeAccountCreation.Core.Models.Enrolments;
using FacadeAccountCreation.Core.Models.Messaging;
using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Models.Person;
using FacadeAccountCreation.Core.Services.Enrolments;
using FacadeAccountCreation.Core.Services.Messaging;
using FacadeAccountCreation.Core.Services.Organisation;
using FacadeAccountCreation.Core.Services.Person;
using FacadeAccountCreation.Core.Services.ServiceRoleLookup;
using FacadeAccountCreation.UnitTests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class EnrolmentsControllerTests
{
    private EnrolmentsController _sut = null!;
    private readonly Mock<IEnrolmentService> _enrolmentService = new();
    private readonly Mock<IPersonService> _mockPersonService = new();
    private readonly Mock<IOrganisationService> _mockOrganisationService = new();
    private readonly Mock<IMessagingService> _mockMessagingService = new();
    private readonly Mock<IServiceRolesLookupService> _mockServiceRolesLookup = new();
    private readonly NullLogger<EnrolmentsController> _logger = new();
    private readonly DeleteUserModel _deleteUserModel = new();
    private readonly Guid _userId = Guid.NewGuid();
    private const int ProducerApprovedPerson = 1;
    private const int RegulatorAdmin = 4;
        
    [TestInitialize]
    public void Setup()
    {
        _deleteUserModel.PersonExternalIdToDelete = Guid.Parse("00000000-0000-0000-0000-000000000001");
        _deleteUserModel.ServiceRoleId = ProducerApprovedPerson;
        
        var messagingConfig = new MessagingConfig() { 
            ApiKey = "test", 
            RemovedUserNotificationTemplateId = Guid.NewGuid().ToString()
        }; 
        var mockMessagingConfig = new Mock<IOptions<MessagingConfig>>();
        mockMessagingConfig.Setup(ap => ap.Value).Returns(messagingConfig);
        
        _sut = new EnrolmentsController(_enrolmentService.Object,
                                        _logger, 
                                        _mockPersonService.Object, 
                                        _mockOrganisationService.Object, 
                                        _mockMessagingService.Object, 
                                        mockMessagingConfig.Object, 
                                        _mockServiceRolesLookup.Object);
        _sut.AddDefaultContextWithOid(_userId, "TestAuth");
    }

    [TestMethod]
    [DataRow("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000001", 1)]
    [DataRow("00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000000", 1)]
    [DataRow("00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000001", 0)]
    public async Task When_invalid_arguments_return_BadRequest(string personExternalIdString, string organisationIdString, int serviceId)
    {
        // Arrange
        var personExternalId = Guid.Parse(personExternalIdString);
        var organisationId = Guid.Parse(organisationIdString);

        // Act
        var response = await _sut.Delete(personExternalId, organisationId, serviceId);
        
        // Assert
        var objectResult = response as ObjectResult;
        objectResult.StatusCode.Should().Be(500);
        var problemDetails = objectResult.Value as ProblemDetails;
        problemDetails.Detail.Should().StartWith("Invalid request to delete user ");
    }

    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task When_service_successfully_deletes_user_then_return_NoContent(HttpStatusCode statusCode)
    {
        // Arrange
        _enrolmentService.Setup(x => x.DeleteUser(It.IsAny<DeleteUserModel>()))
            .ReturnsAsync(new HttpResponseMessage(statusCode));

        _mockPersonService.Setup(x => x.GetPersonByExternalIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new PersonResponseModel());
        _mockOrganisationService.Setup(x => x.GetOrganisationByExternalId(It.IsAny<Guid>()))
            .ReturnsAsync(new RemovedUserOrganisationModel());
        _mockServiceRolesLookup.Setup(x => x.IsProducer(1)).Returns(true);
       
        // Act
        var response = await _sut.Delete(Guid.NewGuid(), Guid.NewGuid(), _deleteUserModel.ServiceRoleId);
        
        // Assert
        response.Should().BeOfType<NoContentResult>();
        _mockMessagingService.Verify(x => x.SendRemovedUserNotification(It.IsAny<RemovedUserNotificationEmailModel>()), Times.Once);
    }
    
    [TestMethod]
    [DataRow(HttpStatusCode.OK)]
    [DataRow(HttpStatusCode.NoContent)]
    public async Task When_serviceRoleId_not_producer_successfully_deletes_user_but_doesnt_send_email(HttpStatusCode statusCode)
    {
        // Arrange
        _enrolmentService.Setup(x => x.DeleteUser(It.IsAny<DeleteUserModel>()))
            .ReturnsAsync(new HttpResponseMessage(statusCode));

        _mockPersonService.Setup(x => x.GetPersonByExternalIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new PersonResponseModel());
        _mockOrganisationService.Setup(x => x.GetOrganisationByExternalId(It.IsAny<Guid>()))
            .ReturnsAsync(new RemovedUserOrganisationModel());
        _mockServiceRolesLookup.Setup(x => x.IsProducer(4)).Returns(false);
       
        // Act
        var response = await _sut.Delete(Guid.NewGuid(), Guid.NewGuid(), RegulatorAdmin);
        
        // Assert
        response.Should().BeOfType<NoContentResult>();
        _mockMessagingService.Verify(x => x.SendRemovedUserNotification(It.IsAny<RemovedUserNotificationEmailModel>()), Times.Never);
    }
    
    [TestMethod]
    public async Task When_service_notsuccessfully_deletes_user_then_return_error()
    {
        // Arrange
        _enrolmentService.Setup(x => x.DeleteUser(It.IsAny<DeleteUserModel>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));
        _mockPersonService.Setup(x => x.GetPersonByExternalIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new PersonResponseModel());
        _mockOrganisationService.Setup((x => x.GetOrganisationByExternalId(It.IsAny<Guid>())))
            .ReturnsAsync(new RemovedUserOrganisationModel());
        
        // Act
        var response = await _sut.Delete(Guid.NewGuid(), Guid.NewGuid(), _deleteUserModel.ServiceRoleId);
        
        // Assert
        response.Should().BeOfType<ObjectResult>();
        var statusCodeResult = response as ObjectResult;
        statusCodeResult?.StatusCode.Should().Be(500);
        _mockMessagingService.Verify(x => x.SendRemovedUserNotification(It.IsAny<RemovedUserNotificationEmailModel>()), Times.Never);
    }
}