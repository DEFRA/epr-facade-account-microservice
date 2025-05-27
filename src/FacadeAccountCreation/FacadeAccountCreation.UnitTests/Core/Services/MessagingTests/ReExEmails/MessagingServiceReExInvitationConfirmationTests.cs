namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests.ReExEmails;

[TestClass]
public class MessagingServiceReExInvitationConfirmationTests : BaseMessagingTest
{
    private readonly List<(string email, string responseId)> notificationList =
        [
            ("test01@test.com", "123456"),
            ("test02@test.com", "98765")
        ];

    [TestMethod]
    public void SendReExInvitationToBeApprovedPerson_Email_Sent_Successfully_With_Response()
    {
        // Arrange
        var orgId = Guid.NewGuid().ToString();

        _ = _notificationClientMock.SetupSequence(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null))
            .Returns(new EmailNotificationResponse { id = "123456" });


        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendReExInvitationConfirmationToInviter("10", "Adam Smith", "adam.smith@test.com", orgId, "Test Ltd", notificationList);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be("123456");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [DataRow(null, "AdamSmith", "adam.smith@test.com", "7885", "Test Ltd")]
    [DataRow("", "AdamSmith", "adam.smith@test.com", "7885", "Test Ltd")]
    [DataRow("   ", "AdamSmith", "adam.smith@test.com", "7885", "Test Ltd")]
    [DataRow("123", null, "adam.smith@test.com", "7885", "Test Ltd")]
    [DataRow("123", " ", "adam.smith@test.com", "7885", "Test Ltd")]
    [DataRow("123", "AdamSmith", null, "7885", "Test Ltd")]
    [DataRow("123", "AdamSmith", " ", "7885", "Test Ltd")]
    [DataRow("123", "AdamSmith", "adam.smith@test.com", null, "Test Ltd")]
    [DataRow("123", "AdamSmith", "adam.smith@test.com", " ", "Test Ltd")]
    [DataRow("123", "AdamSmith", "adam.smith@test.com", "7885", null)]
    [DataRow("123", "AdamSmith", "adam.smith@test.com", "7885", " ")]
    public void SendReExInvitationToBeApprovedPerson_Email_Throws_ArgumentException_As(string userId, string fullName, string userEmail, string organisationId, string companyName)
    {
        // Arrange
        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExInvitationConfirmationToInviter(userId, fullName, userEmail, organisationId, companyName, notificationList);
    }
}
