using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.API.Controllers;
using FacadeAccountCreation.Core.Models.Messaging;
using FacadeAccountCreation.Core.Models.Organisations;
using FacadeAccountCreation.Core.Models.Regulators;
using FacadeAccountCreation.Core.Services.Messaging;
using FacadeAccountCreation.Core.Services.Organisation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Web;
using Moq;
using System.Security.Claims;

namespace FacadeAccountCreation.UnitTests.API.Controllers
{
    [TestClass]
    public class RegulatorControllerTests
    {
        private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
        private readonly NullLogger<RegulatorController> _nullLogger = new();
        private readonly Mock<IOrganisationService> _mockOrganisationService = new();
        private readonly Mock<IMessagingService> _mockMessagingService = new();
        private readonly Mock<HttpContext> _httpContextMock = new();
        private RegulatorController _sut = default!;

        [TestInitialize]
        public void Setup()
        {
            _sut = new RegulatorController(_nullLogger,_mockOrganisationService.Object, _mockMessagingService.Object)
            {
                ControllerContext =
                {
                    HttpContext = _httpContextMock.Object
                }
            };

            _httpContextMock.Setup(x => x.User.Claims).Returns(new List<Claim>
            {
                new ("emails", "abc@efg.com"),
                new (ClaimConstants.ObjectId, _fixture.Create<Guid>().ToString())
            }.AsEnumerable());
        }

        [TestMethod]
        public void SendNotificationOfResubmissionToUser_WhenProducer_PopulatesRequestSuccessfully()
        {
            var request = new ResubmissionNotificationEmailModel
            {
                OrganisationNumber = "123456",
                ProducerOrganisationName = "Organisation Name",
                SubmissionPeriod = "Jan to Jun 2023",
                NationId = 1,
                IsComplianceScheme = false
            };

            var regOrg = new CheckRegulatorOrganisationExistResponseModel
            {
                OrganisationName = "Organisation Name"
            };
            _mockOrganisationService.Setup(m => m.GetRegulatorOrganisationByNationId(1)).ReturnsAsync(regOrg);
            _mockMessagingService.Setup(m => m.SendPoMResubmissionEmailToRegulator(It.IsAny<ResubmissionNotificationEmailInput>()))
                .Returns("notification");

            var result = _sut.SendNotificationOfResubmissionToUser(request);
            Assert.IsNotNull(result);
            _mockMessagingService.Verify(n =>
            n.SendPoMResubmissionEmailToRegulator(It.Is<ResubmissionNotificationEmailInput>(x =>
            x.OrganisationNumber == request.OrganisationNumber
            && !x.IsComplianceScheme
            && x.ComplianceSchemeName == null)), Times.Once);

        }

        [TestMethod]
        public void SendNotificationOfResubmissionToUser_WhenComplianceScheme_PopulatesRequestSuccessfully()
        {
            var request = new ResubmissionNotificationEmailModel
            {
                OrganisationNumber = "123456",
                ProducerOrganisationName = "Organisation Name",
                SubmissionPeriod = "Jan to Jun 2023",
                NationId = 1,
                IsComplianceScheme = true,
                ComplianceSchemeName = "Compliance Scheme Name",
                ComplianceSchemePersonName = "First Last"
            };

            var regOrg = new CheckRegulatorOrganisationExistResponseModel
            {
                OrganisationName = "Organisation Name"
            };
            _mockOrganisationService.Setup(m => m.GetRegulatorOrganisationByNationId(1)).ReturnsAsync(regOrg);
            _mockMessagingService.Setup(m => m.SendPoMResubmissionEmailToRegulator(It.IsAny<ResubmissionNotificationEmailInput>()))
                .Returns("notification");

            var result = _sut.SendNotificationOfResubmissionToUser(request);
            Assert.IsNotNull(result);
            _mockMessagingService.Verify(n =>
            n.SendPoMResubmissionEmailToRegulator(It.Is<ResubmissionNotificationEmailInput>(x =>
            x.OrganisationNumber == request.OrganisationNumber
            && x.IsComplianceScheme
            && x.ComplianceSchemeName == "Compliance Scheme Name")), Times.Once);
        }
    }
}
