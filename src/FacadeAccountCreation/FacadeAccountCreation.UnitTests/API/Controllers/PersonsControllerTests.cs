using FacadeAccountCreation.Core.Models.Organisations;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.API.Controllers;
using FacadeAccountCreation.Core.Models.Person;
using FacadeAccountCreation.Core.Models.User;
using FacadeAccountCreation.Core.Services.Person;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;
using System.Net;
using System.Security.Claims;

[TestClass]
public class PersonsControllerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<IPersonService> _mockPersonService = new();
    private readonly ILogger<PersonsController> _logger = null;
    private PersonsController _sut = null!;
    private Guid _userId;
    private Guid _externalId;

    [TestInitialize]
    public void Setup()
    {
        var httpContextMock = new Mock<HttpContext>();
        _sut = new PersonsController(_mockPersonService.Object, _logger);
        _sut.ControllerContext.HttpContext = httpContextMock.Object;
        _userId = _fixture.Create<Guid>();
        _externalId = _fixture.Create<Guid>();

        httpContextMock.Setup(x => x.User.Claims)
            .Returns(new List<Claim> {
                new ("emails", "joey@ramones.com"),
                new (ClaimConstants.ObjectId, _userId.ToString())
            }.AsEnumerable());
    }

    [TestMethod]
    public async Task GetCurrent_WhenExistingUser_ShouldReturnOK()
    {
        var handlerResponse = _fixture.Create<PersonResponseModel>();

        _mockPersonService
            .Setup(x => x.GetPersonByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        var result = await _sut.GetCurrent();

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult?.Value.Should().BeEquivalentTo(handlerResponse);
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task GetCurrent_WhenNotExistingUser_ShouldReturnNoContent()
    {
        var handlerResponse = (PersonResponseModel?)null;

        _mockPersonService
            .Setup(x => x.GetPersonByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        var result = await _sut.GetCurrent();

        result.Should().BeOfType<NoContentResult>();
        var notFoundResult = result as NoContentResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult?.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
    }


    [TestMethod]
    public async Task GetPerson_WhenExistingUser_ShouldReturnOK()
    {
        // arrange
        var handlerResponse = _fixture.Create<PersonResponseModel>();

        _mockPersonService
            .Setup(x => x.GetPersonByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // act
        var result = await _sut.GetPerson(_userId);

        // assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult?.Value.Should().BeEquivalentTo(handlerResponse);
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task GetPerson_WhenNotExistingUser_ShouldReturnNoContent()
    {
        // arrange
        var handlerResponse = (PersonResponseModel?)null;

        _mockPersonService
            .Setup(x => x.GetPersonByUserIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // act
        var result = await _sut.GetPerson(_userId);

        // assert
        result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = result as NotFoundResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetPersonFromExternalId_WhenExistingUser_ShouldReturnOK()
    {
        // arrange
        var handlerResponse = _fixture.Create<PersonResponseModel>();
        _mockPersonService
            .Setup(x => x.GetPersonByExternalIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // act
        var result = await _sut.GetPersonFromExternalId(_externalId);

        // assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult?.Value.Should().BeEquivalentTo(handlerResponse);
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task GetPersonFromExternalId_WhenNotExistingUser_ShouldReturnNoContent()
    {
        // arrange
        var handlerResponse = (PersonResponseModel?)null;

        _mockPersonService
            .Setup(x => x.GetPersonByExternalIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(handlerResponse);

        // act
        var result = await _sut.GetPersonFromExternalId(_externalId);

        // assert
        result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = result as NotFoundResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult?.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetPersonFromInviteToken_ShouldReturnOK()
    {
        // arrange
        var handlerResponse = _fixture.Create<InviteApprovedUserModel>();

        _mockPersonService
            .Setup(x => x.GetPersonByInviteToken(It.IsAny<string>()))
            .ReturnsAsync(handlerResponse);

        // act
        var result = await _sut.GetPersonFromInviteToken(It.IsAny<string>());

        // assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult?.Value.Should().BeEquivalentTo(handlerResponse);
        okResult?.StatusCode.Should().Be((int)HttpStatusCode.OK);
    }

    //[TestMethod]
    //public async Task PutUserDetailsByUserId_WhenPassedValidData_ShouldReturnOK()
    //{
    //    // arrange  
    //    var requestData = _fixture.Create<UserDetailsUpdateModel>();
    //    var userDetailsDto = new UserDetailsUpdateModel { FirstName = "First", LastName = "Last", JobTitle = "Director", Telephone = "079" };

    //    _mockPersonService
    //        .Setup(x => x.UpdateUserDetailsByUserId(It.IsAny<Guid>(), requestData));

    //    // act
    //    var result = await _sut.PutUserDetailsByUserId(_userId, userDetailsDto);

    //    // assert
    //    result.Should().NotBeNull();
    //    ((StatusCodeResult)result).StatusCode.Should().Be((int)HttpStatusCode.OK);
    //}
}
