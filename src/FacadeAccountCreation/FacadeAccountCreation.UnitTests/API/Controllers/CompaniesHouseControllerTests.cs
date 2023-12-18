using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.API.Controllers;
using FacadeAccountCreation.Core.Models.CompaniesHouse;
using FacadeAccountCreation.Core.Models.CreateAccount;
using FacadeAccountCreation.Core.Services.CompaniesHouse;
using FacadeAccountCreation.Core.Services.CreateAccount;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class CompaniesHouseControllerTests
{
    private const string ValidCompanyId = "realCompanyId";

    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<ICompaniesHouseLookupService> _mockCompaniesHouseLookupServiceMock = new();
    private readonly Mock<IAccountService> _mockAccountServiceMock = new();
    private CompaniesHouseController _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _sut = new CompaniesHouseController(_mockCompaniesHouseLookupServiceMock.Object, _mockAccountServiceMock.Object);
    }

    [TestMethod]
    public async Task Should_return_company_when_id_is_valid()
    {
        // Arrange
        var handlerResponse = _fixture.Create<CompaniesHouseResponse>();

        _mockCompaniesHouseLookupServiceMock
            .Setup(x => x.GetCompaniesHouseResponseAsync(It.Is<string>(pc => pc == ValidCompanyId)))
            .ReturnsAsync(handlerResponse);

        // Act
        var result = await _sut.Get(ValidCompanyId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var obj = result.Result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse);
    }

    [TestMethod]
    public async Task Should_return_company_when_id_is_valid_and_account_exists()
    {
        // Arrange
        var handlerResponse = _fixture.Create<CompaniesHouseResponse>();
        var handlerResponseOrg = _fixture.Create<List<OrganisationResponseModel>?>();

        _mockCompaniesHouseLookupServiceMock
            .Setup(x => x.GetCompaniesHouseResponseAsync(It.Is<string>(pc => pc == ValidCompanyId)))
            .ReturnsAsync(handlerResponse);

        _mockAccountServiceMock
            .Setup(x => x.GetOrganisationsByCompanyHouseNumberAsync(It.Is<string>(pc => pc == ValidCompanyId)))
            .ReturnsAsync(handlerResponseOrg);

        // Act
        var result = await _sut.Get(ValidCompanyId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var obj = result.Result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse);
        var companiesHouseResponse = obj?.Value as CompaniesHouseResponse;
        companiesHouseResponse?.AccountExists.Should().BeTrue();
    }

    [TestMethod]
    public async Task Should_return_nocontent_when_companyhouse()
    {
        // Arrange
        var handlerResponse = _fixture.Create<CompaniesHouseResponse>();

        _mockCompaniesHouseLookupServiceMock
            .Setup(x => x.GetCompaniesHouseResponseAsync(It.Is<string>(pc => pc == ValidCompanyId)));

        // Act
        var result = await _sut.Get(ValidCompanyId);

        // Assert
        result.Result.Should().BeOfType<NoContentResult>();
    }

}
