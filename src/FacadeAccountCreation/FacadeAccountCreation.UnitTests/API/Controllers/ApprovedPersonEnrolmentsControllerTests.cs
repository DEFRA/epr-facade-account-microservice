using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.API.Controllers;
using FacadeAccountCreation.Core.Models.Connections;
using FacadeAccountCreation.Core.Services.Connection;
using FacadeAccountCreation.Core.Services.Messaging;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Web;
using Moq;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FacadeAccountCreation.UnitTests.API.Controllers
{
    [TestClass]
    public class ApprovedPersonEnrolmentsControllerTests
    {
        private ApprovedPersonEnrolmentsController _sut = null!;
        private readonly Mock<IRoleManagementService> _roleManagementService = new();
        private readonly NullLogger<ApprovedPersonEnrolmentsController> _logger = new();
        private readonly Mock<HttpContext> _httpContextMock = new();
        private readonly Mock<IMessagingService> _mockedMessagingService = new Mock<IMessagingService>();

        private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

        private readonly Guid _enrolmentId = Guid.NewGuid();
        private readonly Guid _organisationId = Guid.NewGuid();
        private readonly string _serviceKey = "Packaging";

        [TestInitialize]
        public void Setup()
        {       
            _sut = new ApprovedPersonEnrolmentsController(_roleManagementService.Object, _mockedMessagingService.Object, _logger)
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
        public async Task AcceptNominationForApprovedPerson_WhenRequestRespondsOk_ThenReturnOkResult()
        {
            _roleManagementService
                .Setup(x => x.AcceptNominationForApprovedPerson(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                    It.IsAny<AcceptNominationApprovedPersonRequest>()))
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK
                    });

            var result =
                await _sut.AcceptNominationForApprovedPerson(_enrolmentId, GetAAPRequest(), _serviceKey,
                    _organisationId) as OkResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [TestMethod]
        public async Task AcceptNominationForApprovedPerson_WhenRequestRespondsWithNotAnOk_ThenReturnStatusCode()
        {
            _roleManagementService
                .Setup(x => x.AcceptNominationForApprovedPerson(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                    It.IsAny<AcceptNominationApprovedPersonRequest>()))
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest
                    });

            var result =
                await _sut.AcceptNominationForApprovedPerson(_enrolmentId, GetAAPRequest(), _serviceKey,
                    _organisationId) as BadRequestResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [TestMethod]
        public async Task AcceptNominationForApprovedPerson_WhenThrowsError_ThenReturnsInternalError()
        {
            _roleManagementService
                .Setup(x => x.AcceptNominationForApprovedPerson(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(),
                    It.IsAny<AcceptNominationApprovedPersonRequest>()))
                    .ThrowsAsync(new HttpRequestException("fake exception", null, HttpStatusCode.InternalServerError));

            var result =
                await _sut.AcceptNominationForApprovedPerson(_enrolmentId, GetAAPRequest(), _serviceKey,
                    _organisationId) as StatusCodeResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        private static AcceptNominationApprovedPersonRequest GetAAPRequest()
        {
            return new AcceptNominationApprovedPersonRequest
            {
                ContactEmail = "test@test.com",
                DeclarationFullName = "First Last",
                DeclarationTimeStamp = DateTime.Now,
                JobTitle = "Worker",
                OrganisationName = "Org 1",
                OrganisationNumber = "1",
                PersonFirstName = "first",
                PersonLastName = "last",
                Telephone = "01274558822"
            };
        }
    }
}