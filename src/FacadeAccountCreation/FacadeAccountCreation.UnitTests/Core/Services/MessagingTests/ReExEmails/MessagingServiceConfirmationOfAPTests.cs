namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests.ReExEmails;

[TestClass]
public class MessagingServiceConfirmationOfAPTests : BaseMessagingTest
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    [TestMethod]
    public void SendReExConfirmationOfAnApprovedPerson_Email_Sent_Successfully_Returns_ResponseID()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();

        _ = _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null))
            .Returns(new EmailNotificationResponse { id = "8171" });

        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendReExConfirmationOfAnApprovedPerson(notificationModel);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be("8171");

        _notificationClientMock.Verify(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendRejectionEmailFromInvitedAP_Throws_ArgumentException_AsFirstNameIsEmpty()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        notificationModel.UserFirstName = string.Empty;

        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExConfirmationOfAnApprovedPerson(notificationModel);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendRejectionEmailFromInvitedAP_Throws_ArgumentException_AsLastNameIsEmpty()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        notificationModel.UserLastName = string.Empty;

        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExConfirmationOfAnApprovedPerson(notificationModel);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendRejectionEmailFromInvitedAP_Throws_ArgumentException_AsOrganisationIdIsEmpty()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        notificationModel.OrganisationId = string.Empty;

        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExConfirmationOfAnApprovedPerson(notificationModel);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendRejectionEmailFromInvitedAP_Throws_ArgumentException_AsCompanyNameIsEmpty()
    {
        // Arrange
        var notificationModel = _fixture.Create<ReExNotificationModel>();
        notificationModel.CompanyName = string.Empty;

        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExConfirmationOfAnApprovedPerson(notificationModel);
    }
}
