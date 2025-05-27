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
        var result = _sut.SendRejectionConfirmationToApprovedPerson("123", "Test Ltd", "John Doe", "john.doe@test.com");

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
    [DataRow(null, "Test Ltd", "John Doe", "john.doe@test.com")]
    [DataRow(" ", "Test Ltd", "John Doe", "john.doe@test.com")]
    [DataRow("123", null, "John Doe", "john.doe@test.com")]
    [DataRow("123", "  ", "John Doe", "john.doe@test.com")]
    [DataRow("123", "Test Ltd", null, "john.doe@test.com")]
    [DataRow("123", "Test Ltd", "    ", "john.doe@test.com")]
    [DataRow("123", "Test Ltd", "John Doe", null)]
    [DataRow("123", "Test Ltd", "John Doe", "   ")]
    public void SendRejectionConfirmationToApprovedPerson_Throws_Exception_As(string organisationId, string organisationName, string rejectedByName, string rejectedAPEmail)
    {
        _sut = GetServiceUnderTest();
        _ = _sut.SendRejectionConfirmationToApprovedPerson(organisationId, organisationName, rejectedByName, rejectedAPEmail);
    }
}
