using FacadeAccountCreation.Core.Models.Messaging;
using FluentAssertions;
using Moq;

namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests
{
    [TestClass]
    public class MessagingServiceResubmissionEmailTests : BaseMessagingTest
    {
        [TestMethod]
        public void SendPoMResubmissionConfirmationToRegulator_WhenProducer_UsesCorrectTemplateId()
        {
            var input = new ResubmissionNotificationEmailInput
            {
                OrganisationNumber = "123456",
                ProducerOrganisationName = "Compliance Scheme Name",
                SubmissionPeriod = "Jan to Jun 2023",
                NationId = 1,
                IsComplianceScheme = false
            };

            var email = "dummyemailengland";

            _notificationClientMock.Setup(n => n.SendEmail(email,
            "ProducerResubmissionTemplateId", It.IsAny<Dictionary<string, object>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() { id = "p12356" });

            _sut = GetServiceUnderTest();

            var result = _sut.SendPoMResubmissionEmailToRegulator(input);

            result.Should().NotBeNull();
            _notificationClientMock.Verify(n => n.SendEmail("dummyemailengland", "ProducerResubmissionTemplateId", It.IsAny<Dictionary<string, object>>(),
                null,
                null, null), Times.Once);
        }

        [TestMethod]
        public void SendPoMResubmissionConfirmationToRegulator_WhenComplianceScheme_UsesCorrectTemplateId()
        {
            var input = new ResubmissionNotificationEmailInput
            {
                OrganisationNumber = "123456",
                ProducerOrganisationName = "Compliance Scheme Name",
                SubmissionPeriod = "Jan to Jun 2023",
                NationId = 1,
                IsComplianceScheme = true,
                ComplianceSchemeName = "Organisation Name",
                ComplianceSchemePersonName = "First Last"
            };

            var email = "dummyemailengland";

            _notificationClientMock.Setup(n => n.SendEmail(email,
            "ComplianceSchemeResubmissionTemplateId", It.IsAny<Dictionary<string, object>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() { id = "p12356" });

            _sut = GetServiceUnderTest();

            var result = _sut.SendPoMResubmissionEmailToRegulator(input);

            result.Should().NotBeNull();
            _notificationClientMock.Verify(n => n.SendEmail("dummyemailengland", "ComplianceSchemeResubmissionTemplateId", It.IsAny<Dictionary<string, object>>(),
                null,
                null, null), Times.Once);
        }
    }
}
