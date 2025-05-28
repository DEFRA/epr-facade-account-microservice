using Notify.Models;

namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests.ReExEmails;

[TestClass]
public class MessagingserviceReExRejectionEmailFromAPTests : BaseMessagingTest
{
    [TestMethod]
    public void SendRejectionEmailFromInvitedAP_Email_Sent_Successfully_Returns_ResponseID()
    {
        // Arrange
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
        var result = _sut.SendRejectionEmailFromInvitedAP("123", "John Smith", "john.smith@test.com", "85654", "Test Ltd", "John Doe");

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
    [DataRow(null, "John Smith", "john.smith@test.com", "85654", "Test Ltd", "John Doe")]
    [DataRow(" ", "John Smith", "john.smith@test.com", "85654", "Test Ltd", "John Doe")]
    [DataRow("123", null, "john.smith@test.com", "85654", "Test Ltd", "John Doe")]
    [DataRow("123", " ", "john.smith@test.com", "85654", "Test Ltd", "John Doe")]
    [DataRow("123", "John Smith", null, "85654", "Test Ltd", "John Doe")]
    [DataRow("123", "John Smith", "", "85654", "Test Ltd", "John Doe")]
    [DataRow("123", "John Smith", "john.smith@test.com", null, "Test Ltd", "John Doe")]
    [DataRow("123", "John Smith", "john.smith@test.com", "  ", "Test Ltd", "John Doe")]
    [DataRow("123", "John Smith", "john.smith@test.com", "85654", null, "John Doe")]
    [DataRow("123", "John Smith", "john.smith@test.com", "85654", "  ", "John Doe")]
    [DataRow("123", "John Smith", "john.smith@test.com", "85654", "Test Ltd", null)]
    [DataRow("123", "John Smith", "john.smith@test.com", "85654", "Test Ltd", "")]
    public void SendRejectionEmailFromInvitedAP_Throws_ArgumentException_As(string userId, string userFullName, string userEmail, string organisationId, string organisationName, string rejectedByName)
    {
        // Arrange
        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendRejectionEmailFromInvitedAP(userId, userFullName, userEmail, organisationId, organisationName, rejectedByName);
    }
}
