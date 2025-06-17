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
        var result = _sut.SendReExInvitationConfirmationToInviter("10", "Adam", "Smith", "adam.smith@test.com", "Test Ltd", notificationList);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be("123456");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    [DataRow(null, "Adam", "Smith", "adam.smith@test.com", "Test Ltd")]
    [DataRow("", "Adam", "Smith", "adam.smith@test.com", "Test Ltd")]
    [DataRow("   ", "Adam", "Smith", "adam.smith@test.com", "Test Ltd")]
    [DataRow("123", null, "Smith", "adam.smith@test.com", "Test Ltd")]
    [DataRow("123", " ", "Smith ", "adam.smith@test.com", "Test Ltd")]
    [DataRow("123", "Adam", null, "adam.smith@test.com", "Test Ltd")]
    [DataRow("123", "Adam", "  ", "adam.smith@test.com", "Test Ltd")]
    [DataRow("123", "Adam", "Smith", null, "Test Ltd")]
    [DataRow("123", "Adam", "Smith", " ", "Test Ltd")]
    [DataRow("123", "Adam", "Smith", "adam.smith@test.com", null)]
    [DataRow("123", "Adam", "Smith", "adam.smith@test.com", "  ")]
    public void SendReExInvitationToBeApprovedPerson_Email_Throws_ArgumentException_As(string userId, string inviterFirstName, string inviterLastName, string userEmail, string companyName)
    {
        // Arrange
        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExInvitationConfirmationToInviter(userId, inviterFirstName, inviterLastName, userEmail, companyName, notificationList);
    }
}
