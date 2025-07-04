using FacadeAccountCreation.Core.Models.ReprocessorExporter;
using FacadeAccountCreation.Core.Services.ReprocessorExporter;

namespace FacadeAccountCreation.UnitTests.API.Controllers;

[TestClass]
public class ReprocessorExporterControllerTests
{
	private Mock<IReprocessorExporterService> _reprocessorExporterServiceMock;
	private ReprocessorExporterController _controller;

	[TestInitialize]
	public void SetUp()
	{
		_reprocessorExporterServiceMock = new Mock<IReprocessorExporterService>();

		_controller = new ReprocessorExporterController(_reprocessorExporterServiceMock.Object);
	}

	[TestMethod]
	public async Task GetOrganisationDetailsByOrgId_ReturnsOk_WithExpectedResult()
	{
		// Arrange
		var organisationId = Guid.NewGuid();
		var expectedOrganisationDetails = new OrganisationDetailsResponseDto();

		_reprocessorExporterServiceMock
			.Setup(s => s.GetOrganisationDetailsByOrgId(organisationId))
			.ReturnsAsync(expectedOrganisationDetails);

		// Act
		var result = await _controller.GetOrganisationDetailsByOrgId(organisationId);

		// Assert
		var okResult = result as OkObjectResult;
		okResult.Should().NotBeNull();
		okResult!.Value.Should().BeEquivalentTo(expectedOrganisationDetails);
	}
}
