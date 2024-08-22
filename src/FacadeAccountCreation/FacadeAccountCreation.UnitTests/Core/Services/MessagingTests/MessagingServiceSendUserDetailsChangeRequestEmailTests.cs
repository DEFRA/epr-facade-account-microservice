using FacadeAccountCreation.Core.Models.Messaging;
using FluentAssertions;
using Moq;
using Notify.Exceptions;

namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests;

[TestClass]
public class MessagingServiceSendUserDetailsChangeRequestEmailTests : BaseMessagingTest
{
    [TestMethod]
    public void SendUserDetailChangeRequestEmailToRegulator_WhenValidParameters_ShouldReturnCorrectNotificationId()
    {
        // Arrange
        var input = new UserDetailsChangeNotificationEmailInput
        {
            OldFirstName = "John",
            OldLastName = "Doe",
            OrganisationName = "TestOrg",
            OrganisationNumber = "12345",
            OldJobTitle = "Manager",
            NewFirstName = "Johnny",
            NewLastName = "Doe",
            NewJobTitle = "Senior Manager",
            ContactEmailAddress = "john.doe@example.com",
            ContactTelephone = "123456789",
            Nation = "England"
        };

        var emailNotificationId = "Notification123";
        var emailResponse = new Notify.Models.Responses.EmailNotificationResponse { id = emailNotificationId };

        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(), null, null))
            .Returns(emailResponse);

        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendUserDetailChangeRequestEmailToRegulator(input);

        // Assert
        result.Should().Be(emailNotificationId);
    }

    [TestMethod]
    public void SendUserDetailChangeRequestEmailToRegulator_WhenNotifyFails_ShouldReturnNull()
    {
        // Arrange
        var input = new UserDetailsChangeNotificationEmailInput
        {
            OldFirstName = "John",
            OldLastName = "Doe",
            OrganisationName = "TestOrg",
            OrganisationNumber = "12345",
            OldJobTitle = "Manager",
            NewFirstName = "Johnny",
            NewLastName = "Doe",
            NewJobTitle = "Senior Manager",
            ContactEmailAddress = "john.doe@example.com",
            ContactTelephone = "123456789",
            Nation = "England"
        };

        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(), null, null))
            .Throws(new NotifyClientException());

        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendUserDetailChangeRequestEmailToRegulator(input);

        // Assert
        result.Should().BeNull();
    }
}
