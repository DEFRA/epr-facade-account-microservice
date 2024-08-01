using FacadeAccountCreation.Core.Models.ComplianceScheme;
using FacadeAccountCreation.Core.Models.Messaging;
using FluentAssertions;
using Moq;

namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests;

[TestClass]
public class MessagingRemovalNotficationToProducerTests : BaseMessagingTest
{
    [TestMethod]
    [DynamicData(nameof(DynamicnotifyComplianceSchemeProducerEmailInput))]
    public void SendMemberDissociationProducersNotification_WhenValidParameters_ItShouldSendEmail(NotifyComplianceSchemeProducerEmailInput notifyComplianceSchemeProducerEmailInput)
    {
        var emailNotificationId = "N123456";
        var recipient = "dummyEmail";
        var templateId = "dummytemplate";
        _ = _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() { id = emailNotificationId });

        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendMemberDissociationProducersNotification(notifyComplianceSchemeProducerEmailInput);

        // Assert
        _notificationClientMock.Verify(n => n.SendEmail(recipient, templateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null,
            null), Times.Once);
        result.Count.Should().Be(1);
    }

    private static IEnumerable<NotifyComplianceSchemeProducerEmailInput[]> DynamicnotifyComplianceSchemeProducerEmailInput
    {
        get
        {
            yield return new NotifyComplianceSchemeProducerEmailInput[]
            {
                new NotifyComplianceSchemeProducerEmailInput()
                {
                    UserId = Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7762"),
                    Recipients = new List<EmailRecipient>
                    {
                    new()
                        {
                            Email = "dummyEmail",
                            FirstName = "Test",
                            LastName = "last",

                        }
                    },
                    OrganisationId = "1",
                    OrganisationName = "Organisation",
                    ComplianceScheme = "Compliance Scheme Name"
                }
            };
        }
    }
}
