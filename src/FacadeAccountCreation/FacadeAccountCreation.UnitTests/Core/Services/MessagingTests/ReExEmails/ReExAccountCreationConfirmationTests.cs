namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests.ReExEmails;

[TestClass]
public class ReExAccountCreationConfirmationTests : BaseMessagingTest
{
    [TestMethod]
    public void SendReExAccountCreationConfirmation_Email_Sent_Successfully_Returns_ResponseID()
    {
        // Arrange
        _ = _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null))
            .Returns(new EmailNotificationResponse { id = "236656" });

        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendReExAccountCreationConfirmation("456-787-9050", "John", "Doe", "john.doe@test.com");

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be("236656");

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
    [DataRow(null, "John", "Doe", "john.doe@test.com")]
    [DataRow("  ", "John", "Doe", "john.doe@test.com")]
    [DataRow("96325899", null, "Doe", "john.doe@test.com")]
    [DataRow("96325899", "  ", "Doe", "john.doe@test.com")]
    [DataRow("96325899", "John", null, "john.doe@test.com")]
    [DataRow("96325899", "John", "    ", "john.doe@test.com")]
    [DataRow("96325899", "John", "Doe", null)]
    [DataRow("96325899", "John", "Doe", "   ")]
    public void SendReExAccountCreationConfirmation_Throws_Exception_As(string userId, string firstName, string lastName, string contactEmail)
    {
        _sut = GetServiceUnderTest();
        _ = _sut.SendReExAccountCreationConfirmation(userId, firstName, lastName, contactEmail);
    }

    [TestMethod]
    public void SendReExAccountCreationConfirmation_WhenNotificationClientThrowsException_ReturnsNull_And_LogsError()
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
        var result = _sut.SendReExAccountCreationConfirmation("456-787-9050", "John", "Doe", "john.doe@test.com");

        // Assert
        result.Should().BeNull();
    }
}
