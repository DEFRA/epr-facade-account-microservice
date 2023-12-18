using AutoFixture;
using AutoFixture.AutoMoq;
using FacadeAccountCreation.API.Controllers;
using FacadeAccountCreation.Core.Models.AddressLookup;
using FacadeAccountCreation.Core.Services.AddressLookup;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class AddressLookupControllerTests
{
    private const string ValidPostcode = "BT30 9EG";
    
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<IAddressLookupService> _addressLookupServiceMock = new();
    private AddressLookupController _sut = null!;
    
    
    [TestInitialize]
    public void Setup()
    {
        _sut = 
            new AddressLookupController(_addressLookupServiceMock.Object);
    }

    [TestMethod]
    public async Task Should_return_address_lookup_response_when_postcode_is_valid()
    {
        // Arrange
        var handlerResponse = _fixture.Create<AddressLookupResponseDto>();
        
        _addressLookupServiceMock
            .Setup(x => x.GetAddressLookupResponseAsync(It.Is<string>(pc => pc == ValidPostcode)))
            .ReturnsAsync(handlerResponse);
        
        // Act
        var result = await _sut.Get("BT30 9EG");
        
        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var obj = result.Result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse);
    }
    
    [TestMethod]
    public async Task Should_return_address_lookup_response_when_postcode_is_valid_lower_case()
    {
        // Arrange
        var handlerResponse = _fixture.Create<AddressLookupResponseDto>();
        
        _addressLookupServiceMock
            .Setup(x => x.GetAddressLookupResponseAsync(It.Is<string>(pc => pc == ValidPostcode.ToLower())))
            .ReturnsAsync(handlerResponse);
        
        // Act
        var result = await _sut.Get(ValidPostcode.ToLower());
        
        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var obj = result.Result as OkObjectResult;
        obj?.Value.Should().BeEquivalentTo(handlerResponse);
    }
    
    [TestMethod]
    public async Task Should_return_NoContent_when_no_address()
    {
        // Arrange
        var handlerResponse = _fixture.Create<AddressLookupResponseDto>();

        _addressLookupServiceMock
            .Setup(x => x.GetAddressLookupResponseAsync(It.Is<string>(pc => pc == ValidPostcode)));
           
        
        // Act
        var result = await _sut.Get("BT30 9EG");
        
        // Assert
        result.Result.Should().BeOfType<NoContentResult>();
    }
}
