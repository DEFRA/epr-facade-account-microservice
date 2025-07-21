namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests.ReExEmails;

[TestClass]
public class MessagingServiceReExInvitationEmailTests : BaseMessagingTest
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    [TestMethod]
    public void SendReExInvitationToBeApprovedPerson_Email_Sent_Successfully_Returns_ResponseID()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        
        _ = _notificationClientMock.SetupSequence(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null))
            .Returns(new EmailNotificationResponse { id = "1" })
            .Returns(new EmailNotificationResponse { id = "2" })
            .Returns(new EmailNotificationResponse { id = "3" });

        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendReExInvitationToBeApprovedPerson(notificationModel);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Count.Should().Be(3);
        result[0].email.Should().NotBeNullOrWhiteSpace();
        result[0].notificationResponseId.Should().BeEquivalentTo("1");
        result[1].notificationResponseId.Should().BeEquivalentTo("2");
        result[2].notificationResponseId.Should().BeEquivalentTo("3");

        _notificationClientMock.Verify(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null), Times.Exactly(3));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendReExInvitationToBeApprovedPerson_Email_Throws_ArgumentException_ForFirstName()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        notificationModel.ReExInvitedApprovedPersons[0].FirstName = string.Empty;

        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExInvitationToBeApprovedPerson(notificationModel);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendReExInvitationToBeApprovedPerson_Email_Throws_ArgumentException_ForLastName()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        notificationModel.ReExInvitedApprovedPersons[0].LastName = string.Empty;

        _ = _notificationClientMock.SetupSequence(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null))
            .Returns(new EmailNotificationResponse { id = "1" })
            .Returns(new EmailNotificationResponse { id = "2" })
            .Returns(new EmailNotificationResponse { id = "3" });

        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExInvitationToBeApprovedPerson(notificationModel);

        _notificationClientMock.Verify(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null), Times.Never());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendReExInvitationToBeApprovedPerson_Email_Throws_ArgumentException_ForMember_InviteToken()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        notificationModel.ReExInvitedApprovedPersons[0].InviteToken = string.Empty;

        _ = _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null))
            .Returns(new EmailNotificationResponse { id = "1" });

        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExInvitationToBeApprovedPerson(notificationModel);

        _notificationClientMock.Verify(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null), Times.Never());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendReExInvitationToBeApprovedPerson_Email_Throws_ArgumentException_For_ApprovedPersonEmail()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        notificationModel.ReExInvitedApprovedPersons[0].Email = string.Empty;

        _ = _notificationClientMock.SetupSequence(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null))
            .Returns(new EmailNotificationResponse { id = "1" })
            .Returns(new EmailNotificationResponse { id = "2" })
            .Returns(new EmailNotificationResponse { id = "3" });

        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExInvitationToBeApprovedPerson(notificationModel);

        _notificationClientMock.Verify(nc => nc.SendEmail(
           It.IsAny<string>(),
           It.IsAny<string>(),
           It.IsAny<Dictionary<string, dynamic>>(),
           null,
           null,
           null), Times.Never());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendReExInvitationToBeApprovedPerson_Email_Throws_ArgumentException_ForOrganisationName()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        notificationModel.CompanyName = string.Empty;

        _ = _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null))
            .Returns(new EmailNotificationResponse { id = "3" });

        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExInvitationToBeApprovedPerson(notificationModel);

        _notificationClientMock.Verify(nc => nc.SendEmail(
           It.IsAny<string>(),
           It.IsAny<string>(),
           It.IsAny<Dictionary<string, dynamic>>(),
           null,
           null,
           null), Times.Never());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendReExInvitationToBeApprovedPerson_Email_Throws_ArgumentException_For_InviteeFirstName()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        notificationModel.UserFirstName = string.Empty;

        _ = _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null))
            .Returns(new EmailNotificationResponse { id = "3" });

        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExInvitationToBeApprovedPerson(notificationModel);

        _notificationClientMock.Verify(nc => nc.SendEmail(
           It.IsAny<string>(),
           It.IsAny<string>(),
           It.IsAny<Dictionary<string, dynamic>>(),
           null,
           null,
           null), Times.Never());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendReExInvitationToBeApprovedPerson_Email_Throws_ArgumentException_For_InviteeLastName()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        notificationModel.UserLastName = string.Empty;

        _ = _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null))
            .Returns(new EmailNotificationResponse { id = "3" });

        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExInvitationToBeApprovedPerson(notificationModel);

        _notificationClientMock.Verify(nc => nc.SendEmail(
           It.IsAny<string>(),
           It.IsAny<string>(),
           It.IsAny<Dictionary<string, dynamic>>(),
           null,
           null,
           null), Times.Never());
    }

    [TestMethod]
    public void SendReExInvitationToBeApprovedPerson_WhenNotificationClientThrowsException_ReturnsNull_And_LogsError()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        var thrownException = new InvalidOperationException("some exception");

        _notificationClientMock
            .Setup(nc => nc.SendEmail(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                null,
                null,
                null))
            .Throws(thrownException);

        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendReExInvitationToBeApprovedPerson(notificationModel);

        // Assert
        result.Count.Should().Be(0);
    }
}
