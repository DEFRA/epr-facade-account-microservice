using FacadeAccountCreation.Core.Models.Connections;
using FacadeAccountCreation.Core.Models.Messaging;
using FluentAssertions;
using Moq;
using Notify.Exceptions;

namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests;

[TestClass]
public class MessagingServiceSendAccountCreationTests : BaseMessagingTest
{
    [TestMethod]
    [DataRow("Johnny", "Ramone", "johnny@ramone.com", "111222", false, "C123456")]
    [DataRow("Joey", "Ramone", "joey@ramone.com", "333444", true, "P123456")]
    public void SendAccountCreationConfirmation_WhenValidParameters_ItShouldReturnCorrectId(string firstName,
        string lastName, string recipient, string organisationId, bool isComplianceScheme, string expected)
    {
        // Arrange
        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() {id = expected});

        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendAccountCreationConfirmation(Guid.NewGuid(), firstName, lastName, recipient, organisationId,
            Guid.NewGuid(), isComplianceScheme);

        // Assert
        result.Should().Be(expected);
    }

    [TestMethod]
    [DataRow("", "Ramone", "johnny@ramone.com", "111222")]
    [DataRow("Joey", "", "joey@ramone.com", "333444")]
    [DataRow("Tommy", "Ramone", "tommy@ramone.com", "")]
    [ExpectedException(typeof(ArgumentException))]
    public void SendAccountCreationConfirmation_WhenInvalidParameters_ItShouldThrowArgumentException(string firstName,
        string lastName, string recipient, string organisationId)
    {
        // Arrange
        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendAccountCreationConfirmation(Guid.NewGuid(), firstName, lastName, recipient, organisationId,
            new Guid());
    }

    [TestMethod]
    [DataRow("Johnny", "Ramone", "johnny@ramone.com", "111222", false)]
    [DataRow("Joey", "Ramone", "joey@ramone.com", "333444", true)]
    public void SendAccountCreationConfirmation_WhenNotifyFails_ItShouldReturnNull(string firstName, string lastName,
        string recipient, string organisationId, bool isComplianceScheme)
    {
        // Arrange
        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, 
            null)).Throws(new NotifyClientException());

        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendAccountCreationConfirmation(Guid.NewGuid(), firstName, lastName, recipient, organisationId,
            Guid.NewGuid(), isComplianceScheme);

        // Assert
        result.Should().Be(null);
    }

    [TestMethod]
    public void SendDelegatedUserNomination_WhenValidParameters_ItShouldReturnCorrectId()
    {
        string expectedId = "P123456";
        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() { id = expectedId });

        _sut = GetServiceUnderTest();

        var emailInput = new NominateUserEmailInput
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            Recipient = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid(),
            OrganisationNumber = "123456",
            NominatorFirstName = "Joey",
            NominatorLastName = "Ramone",
            OrganisationName = "The Ramones",
            UserId = Guid.NewGuid()
        };
        var result = _sut.SendDelegatedUserNomination(emailInput);

        result.Should().Be(expectedId);
    }

    [TestMethod]

    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInvalidParameters_ItShouldThrowArgumentException()
    {
        _sut = GetServiceUnderTest();

        var emailInput = new NominateUserEmailInput
        {
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid(),
            OrganisationNumber = "123456",
            NominatorFirstName = "Joey",
            NominatorLastName = "Ramone",
            OrganisationName = "The Ramones",
            UserId = Guid.NewGuid()
        };
        _ = _sut.SendDelegatedUserNomination(emailInput);
    }


    [TestMethod]
    public void SendDelegatedUserNomination_WhenNotifyFails_ItShouldReturnNull()
    {
        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Throws(new NotifyClientException());

        _sut = GetServiceUnderTest();

        var emailInput = new NominateUserEmailInput
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            Recipient = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid(),
            OrganisationNumber = "123456",
            NominatorFirstName = "Joey",
            NominatorLastName = "Ramone",
            OrganisationName = "The Ramones",
            UserId = Guid.NewGuid()
        };

        var result = _sut.SendDelegatedUserNomination(emailInput);

        result.Should().Be(null);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInValidOrganisationId_ItShouldThrowException()
    {
        string expectedId = "P123456";
        _sut = GetServiceUnderTest();

        var emailInput = new NominateUserEmailInput
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            Recipient = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.Empty,
            OrganisationNumber = "123456",
            NominatorFirstName = "Joey",
            NominatorLastName = "Ramone",
            OrganisationName = "The Ramones",
            UserId = Guid.NewGuid()
        };
        _ = _sut.SendDelegatedUserNomination(emailInput);

    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInValidUserId_ItShouldThrowException()
    {
        string expectedId = "P123456";
        _sut = GetServiceUnderTest();

        var emailInput = new NominateUserEmailInput
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            Recipient = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid(),
            OrganisationNumber = "123456",
            NominatorFirstName = "Joey",
            NominatorLastName = "Ramone",
            OrganisationName = "The Ramones",
            UserId = Guid.Empty
        };
        _ = _sut.SendDelegatedUserNomination(emailInput);

    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInValidTemplateId_ItShouldThrowException()
    {
        string expectedId = "P123456";
        _sut = GetServiceUnderTest();

        var emailInput = new NominateUserEmailInput
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            Recipient = "johnny@ramones.com",
            TemplateId = "",
            OrganisationId = Guid.NewGuid(),
            OrganisationNumber = "123456",
            NominatorFirstName = "Joey",
            NominatorLastName = "Ramone",
            OrganisationName = "The Ramones",
            UserId = Guid.NewGuid()
        };
        _ = _sut.SendDelegatedUserNomination(emailInput);

    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInValidNominatorFirstName_ItShouldThrowException()
    {
        string expectedId = "P123456";
        _sut = GetServiceUnderTest();

        var emailInput = new NominateUserEmailInput
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            Recipient = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid(),
            OrganisationNumber = "123456",
            NominatorFirstName = "",
            NominatorLastName = "Ramone",
            OrganisationName = "The Ramones",
            UserId = Guid.NewGuid()
        };
        _ = _sut.SendDelegatedUserNomination(emailInput);

    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInValidOrganisationName_ItShouldThrowException()
    {
        string expectedId = "P123456";
        _sut = GetServiceUnderTest();

        var emailInput = new NominateUserEmailInput
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            Recipient = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid(),
            OrganisationNumber = "123456",
            NominatorFirstName = "Joey",
            NominatorLastName = "Ramone",
            OrganisationName = "",
            UserId = Guid.NewGuid()
        };
        _ = _sut.SendDelegatedUserNomination(emailInput);

    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInValidOrganisationNumber_ItShouldThrowException()
    {
        _sut = GetServiceUnderTest();

        var emailInput = new NominateUserEmailInput
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            Recipient = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid(),
            OrganisationNumber = "",
            NominatorFirstName = "Joey",
            NominatorLastName = "Ramone",
            OrganisationName = "The Ramones",
            UserId = Guid.NewGuid()
        };
        _ = _sut.SendDelegatedUserNomination(emailInput);

    }

    [TestMethod]
    public void SendDelegatedRoleNotification_SendsWithCorrectInput()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() {id = "p12356"});
        _sut = GetServiceUnderTest();
        
        //Act
        var result = _sut.SendDelegatedRoleNotification(delagateRoleEmailInput);
        
        //Assert
        result.Should().NotBeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Once);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException),
        "FirstName cannot be empty string.")]
    public void SendDelegatedRoleNotification_ThrowsError_WhenInvalidFirstName()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        delagateRoleEmailInput.FirstName = "";
        _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() {id = "p12356"});
        _sut = GetServiceUnderTest();
        
        //Act
        var result = _sut.SendDelegatedRoleNotification(delagateRoleEmailInput);
        
        //Assert
        result.Should().BeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Never);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException),
        "LastName cannot be empty string.")]
    public void SendDelegatedRoleNotification_ThrowsError_WhenInvalidLastName()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        delagateRoleEmailInput.LastName = "";
        _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() {id = "p12356"});
        _sut = GetServiceUnderTest();
        
        //Act
        var result = _sut.SendDelegatedRoleNotification(delagateRoleEmailInput);
        
        //Assert
        result.Should().BeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Never);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException),
        "Recipient cannot be empty string.")]
    public void SendDelegatedRoleNotification_ThrowsError_WhenInvalidRecipient()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        delagateRoleEmailInput.Recipient = "";
        _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() {id = "p12356"});
        _sut = GetServiceUnderTest();
        
        //Act
        var result = _sut.SendDelegatedRoleNotification(delagateRoleEmailInput);
        
        //Assert
        result.Should().BeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Never);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException),
        "OrganisationNumber cannot be empty string.")]
    public void SendDelegatedRoleNotification_ThrowsError_WhenInvalidOrganisationNumber()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        delagateRoleEmailInput.OrganisationNumber = "";
        _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() {id = "p12356"});
        _sut = GetServiceUnderTest();
        
        //Act
        var result = _sut.SendDelegatedRoleNotification(delagateRoleEmailInput);
        
        //Assert
        result.Should().BeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Never);
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException),
        "OrganisationName cannot be empty string.")]
    public void SendDelegatedRoleNotification_ThrowsError_WhenInvalidOrganisationName()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        delagateRoleEmailInput.OrganisationName = "";
        _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() {id = "p12356"});
        _sut = GetServiceUnderTest();
        
        //Act
        var result = _sut.SendDelegatedRoleNotification(delagateRoleEmailInput);
        
        //Assert
        result.Should().BeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Never);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException),
        "NominatorFirstName cannot be empty string.")]
    public void SendDelegatedRoleNotification_ThrowsError_WhenInvalidNominatorFirstName()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        delagateRoleEmailInput.NominatorFirstName = "";
        _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() {id = "p12356"});
        _sut = GetServiceUnderTest();
        
        //Act
        var result = _sut.SendDelegatedRoleNotification(delagateRoleEmailInput);
        
        //Assert
        result.Should().BeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Never);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException),
        "TemplateId cannot be empty string.")]
    public void SendDelegatedRoleNotification_ThrowsError_WhenInvalidTemplateId()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        delagateRoleEmailInput.TemplateId = "";
        _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() {id = "p12356"});
        _sut = GetServiceUnderTest();
        
        //Act
        var result = _sut.SendDelegatedRoleNotification(delagateRoleEmailInput);
        
        //Assert
        result.Should().BeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Never);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException),
        "UserId is required.")]
    public void SendDelegatedRoleNotification_ThrowsError_WhenInvalidUserId()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        delagateRoleEmailInput.UserId = Guid.Empty;
        _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() {id = "p12356"});
        _sut = GetServiceUnderTest();
        
        //Act
        var result = _sut.SendDelegatedRoleNotification(delagateRoleEmailInput);
        
        //Assert
        result.Should().BeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Never);
    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException),
        "OrganisationId is required.")]
    public void SendDelegatedRoleNotification_ThrowsError_WhenInvalidOrganisationId()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        delagateRoleEmailInput.OrganisationId = Guid.Empty;
        _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() {id = "p12356"});
        _sut = GetServiceUnderTest();
        
        //Act
        var result = _sut.SendDelegatedRoleNotification(delagateRoleEmailInput);
        
        //Assert
        result.Should().BeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Never);
    }
    
    [TestMethod]
    public void SendDelegatedRoleNotification_ReturnsNull_WhenSendEmailFails()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Throws(new Exception("Test exception"));
        _sut = GetServiceUnderTest();
        
        //Act
        var result = _sut.SendDelegatedRoleNotification(delagateRoleEmailInput);
        
        //Assert
        result.Should().BeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Once);
    }

    [TestMethod]
    public void SendDelegatedRoleRemove_Notification_SendsWithCorrectInput()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        _ = _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
           null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() { id = "p12356" });
        _sut = GetServiceUnderTest();

        //Act
        var result = _sut.SendDelegatedRoleRemovedNotification(delagateRoleEmailInput);

        //Assert
        result.Should().NotBeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Once);
    }



    [TestMethod]
    public void SendNominationCancelledNotification_SendsWithCorrectInput()
    {
        //Arrange
        var delagateRoleEmailInput = EmailInput();
        _notificationClientMock.Setup(n => n.SendEmail(delagateRoleEmailInput.Recipient,
            delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
           null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() { id = "p78655" });
        _sut = GetServiceUnderTest();

        //Act
        var result = _sut.SendNominationCancelledNotification(delagateRoleEmailInput);

        //Assert
        result.Should().NotBeNull();
        _notificationClientMock.Verify(n => n.SendEmail(delagateRoleEmailInput.Recipient, delagateRoleEmailInput.TemplateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Once);
    }


    private DelegatedRoleEmailInput EmailInput()
    {
        return new DelegatedRoleEmailInput
        {
            Recipient = "recipient",
            FirstName = "name",
            LastName = "lname",
            NominatorFirstName = "nname",
            NominatorLastName = "nlname",
            OrganisationName = "orgName",
            OrganisationId = Guid.NewGuid(),
            OrganisationNumber=  "122348" ,
            PersonRole = PersonRole.Admin,
            UserId = Guid.NewGuid(),
            TemplateId = Guid.NewGuid().ToString()
        };
    }
        
}