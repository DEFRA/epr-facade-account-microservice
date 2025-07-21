namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests.ReExEmails;

[TestClass]
public class MessagingServiceConfirmationOfAPTests : BaseMessagingTest
{
    [TestMethod]
    public void SendReExConfirmationOfAnApprovedPerson_Email_Sent_Successfully_Returns_ResponseID()
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
        var result = _sut.SendReExConfirmationOfAnApprovedPerson("678", "john.smith@tester.com", "John", "Smith", "Test Ltd", "Peter", "Welsh");

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
    [DataRow(null, "john.smith@tester.com", "John", "Smith", "Test Ltd", "Peter", "Welsh")]
    [DataRow("  ", "john.smith@tester.com", "John", "Smith", "Test Ltd", "Peter", "Welsh")]
    [DataRow("678", null, "John", "Smith", "Test Ltd", "Peter", "Welsh")]
    [DataRow("678", " ", "John", "Smith", "Test Ltd", "Peter", "Welsh")]
    [DataRow("678", "john.smith@tester.com", null, "Smith", "Test Ltd", "Peter", "Welsh")]
    [DataRow("678", "john.smith@tester.com", "", "Smith", "Test Ltd", "Peter", "Welsh")]
    [DataRow("678", "john.smith@tester.com", "John", null, "Test Ltd", "Peter", "Welsh")]
    [DataRow("678", "john.smith@tester.com", "John", "  ", "Test Ltd", "Peter", "Welsh")]
    [DataRow("678", "john.smith@tester.com", "John", "Smith", null, "Peter", "Welsh")]
    [DataRow("678", "john.smith@tester.com", "John", "Smith", "    ", "Peter", "Welsh")]
    [DataRow("678", "john.smith@tester.com", "John", "Smith", "Test Ltd", null, "Welsh")]
    [DataRow("678", "john.smith@tester.com", "John", "Smith", "Test Ltd", " ", "Welsh")]
    [DataRow("678", "john.smith@tester.com", "John", "Smith", "Test Ltd", "Peter", null)]
    [DataRow("678", "john.smith@tester.com", "John", "Smith", "Test Ltd", "Peter", "")]
    public void SendReExConfirmationOfAnApprovedPerson_Throws_ArgumentException_As_InviterFirstNameIsEmpty(string userId, string inviterEmail, string inviteeFirstName, string inviteeLastName, string companyName, string inviterFirstName, string inviterLastName)
    {
        // Arrange
        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendReExConfirmationOfAnApprovedPerson(userId, inviterEmail, inviteeFirstName, inviteeLastName, companyName, inviterFirstName, inviterLastName);
    }

    [TestMethod]
    public void SendReExConfirmationOfAnApprovedPerson_WhenNotificationClientThrowsException_ReturnsNull_And_LogsError()
    {
        // Arrange
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
        var result = _sut.SendReExConfirmationOfAnApprovedPerson("678", "john.smith@tester.com", "John", "Smith", "Test Ltd", "Peter", "Welsh");

        // Assert
        result.Should().BeNull();
    }
}
