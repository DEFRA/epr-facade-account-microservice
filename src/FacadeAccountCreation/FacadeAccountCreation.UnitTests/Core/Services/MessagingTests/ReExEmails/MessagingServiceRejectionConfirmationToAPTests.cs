namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests.ReExEmails;

[TestClass]
public class MessagingServiceRejectionConfirmationToAPTests : BaseMessagingTest
{
    [TestMethod]
    public void SendRejectionConfirmationToApprovedPerson_Email_Sent_Successfully_Returns_ResponseID()
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
        var result = _sut.SendRejectionConfirmationToApprovedPerson("123", "76543211", "Test Ltd", "John Doe", "john.doe@test.com");

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
    [DataRow(null, "96325899", "Test Ltd", "John Doe", "john.doe@test.com")]
    [DataRow(" ", "96325899", "Test Ltd", "John Doe", "john.doe@test.com")]
    [DataRow("123", null, "Test Ltd", "John Doe", "john.doe@test.com")]
    [DataRow("123 ", "  ", "Test Ltd", "John Doe", "john.doe@test.com")]
    [DataRow("123", "96325899", null, "John Doe", "john.doe@test.com")]
    [DataRow("123", "96325899", "  ", "John Doe", "john.doe@test.com")]
    [DataRow("123", "96325899", "Test Ltd", null, "john.doe@test.com")]
    [DataRow("123", "96325899", "Test Ltd", "    ", "john.doe@test.com")]
    [DataRow("123", "96325899", "Test Ltd", "John Doe", null)]
    [DataRow("123", "96325899", "Test Ltd", "John Doe", "   ")]
    public void SendRejectionConfirmationToApprovedPerson_Throws_Exception_As(string userId, string organisationId, string organisationName, string rejectedByName, string rejectedAPEmail)
    {
        _sut = GetServiceUnderTest();
        _ = _sut.SendRejectionConfirmationToApprovedPerson(userId, organisationId, organisationName, rejectedByName, rejectedAPEmail);
    }

    [TestMethod]
    public void SendRejectionConfirmationToApprovedPerson_WhenNotificationClientThrowsException_ReturnsNull_And_LogsError()
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
        var result = _sut.SendRejectionConfirmationToApprovedPerson("123", "76543211", "Test Ltd", "John Doe", "john.doe@test.com");

        // Assert
        result.Should().BeNull();
    }
}
