using FacadeAccountCreation.Core.Models.Connections;
using FacadeAccountCreation.Core.Models.Messaging;
using FluentAssertions;
using Moq;
using Notify.Exceptions;

namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests;

[TestClass]
public class MessagingServiceSendApprovedUserAccountCreationTests : BaseMessagingTest
{
    [TestMethod]
    [DataRow("TestOrganisation", "Ramone", "johnny", "111222", "asfgldhjf@hbjhdsb.com")]
    [DataRow("TestOrganisation2", "Ramone2", "johnny2", "111222", "asfgldhjf2@hbjhdsb.com")]
    public void SendApprovedUserAccountCreationConfirmation_WhenValidParameters_ItShouldReturnCorrectId(string companyName,
        string firstName, string lastName, string organisationNumber, string recipient)
    {
        // Arrange
        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() {id = "C123456"});

        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendApprovedUserAccountCreationConfirmation(companyName, firstName, lastName,organisationNumber,recipient);

        // Assert
        result.Should().Be("C123456");
    }

    [TestMethod]
    [DataRow("", "", "", "","")]
    [DataRow("", "", "", "","")]
    [DataRow("", "", "", "","")]
    [ExpectedException(typeof(ArgumentException))]
    public void SendApprovedUserAccountCreationConfirmation_WhenInvalidParameters_ItShouldThrowArgumentException(string companyName,
        string firstName, string lastName, string organisationNumber, string recipient)
    {
        // Arrange
        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendApprovedUserAccountCreationConfirmation(companyName, firstName, lastName,organisationNumber,recipient);
    }

    [TestMethod]
    [DataRow("TestOrganisation", "Ramone", "johnny", "111222", "asfgldhjf@hbjhdsb.com")]
    [DataRow("TestOrganisation2", "Ramone2", "johnny2", "111222", "asfgldhjf2@hbjhdsb.com")]
    public void SendApprovedUserAccountCreationConfirmation_WhenNotifyFails_ItShouldReturnNull(string companyName,
        string firstName, string lastName, string organisationNumber, string recipient)
    {
        // Arrange
        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null)).Throws(new NotifyClientException());

        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendApprovedUserAccountCreationConfirmation(companyName, firstName, lastName,organisationNumber,recipient);

        // Assert
        result.Should().Be(null);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInValidCompanyName_ItShouldThrowException()
    {
        _sut = GetServiceUnderTest();

        _ = _sut.SendApprovedUserAccountCreationConfirmation("",
            "Ramone", "johnny@ramones.com", "Johnny",  "The Ramones");

    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInValidFirstName_ItShouldThrowException()
    {
        _sut = GetServiceUnderTest();

        _ = _sut.SendApprovedUserAccountCreationConfirmation("Ramone",
            "", "johnny@ramones.com", "Johnny",  "The Ramones");

    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInValidLastName_ItShouldThrowException()
    {
        _sut = GetServiceUnderTest();

        _ = _sut.SendApprovedUserAccountCreationConfirmation("Ramone",
            "johnny@ramones.com", "", "Johnny",  "The Ramones");

    }
}