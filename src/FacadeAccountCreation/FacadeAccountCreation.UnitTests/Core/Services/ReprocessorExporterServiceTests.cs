using FacadeAccountCreation.Core.Exceptions;
using FacadeAccountCreation.Core.Models.ReprocessorExporter;
using FacadeAccountCreation.Core.Services.ReprocessorExporter;

namespace FacadeAccountCreation.UnitTests.Core.Services;

[TestClass]
public class ReprocessorExporterServiceTests
{
	private const string baseAddress = "http://localhost";
	private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
	private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();

	[TestMethod]
	public async Task GetOrganisationDetailsByOrgId_WhenRecordFound_ShouldReturnPersonResponse()
	{
		var organsationId = Guid.NewGuid();

		var endpoint = $"api/organisations/organisation-with-persons/";
		var expectedUrl = $"{baseAddress}/{endpoint}{organsationId}";

		var apiResponse = _fixture.Create<OrganisationDetailsResponseDto>();

		_httpMessageHandlerMock.Protected()
			 .Setup<Task<HttpResponseMessage>>("SendAsync",
				 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
				 ItExpr.IsAny<CancellationToken>())
			 .ReturnsAsync(new HttpResponseMessage
			 {
				 StatusCode = HttpStatusCode.OK,
				 Content = new StringContent(JsonSerializer.Serialize(apiResponse))
			 }).Verifiable();

		var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
		{
			BaseAddress = new Uri(baseAddress)
		};

		var sut = new ReprocessorExporterService(httpClient);

		var result = await sut.GetOrganisationDetailsByOrgId(organsationId);

		_httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
			ItExpr.Is<HttpRequestMessage>(
				req => req.Method == HttpMethod.Get &&
					   req.RequestUri != null &&
					   req.RequestUri.ToString() == expectedUrl),
			ItExpr.IsAny<CancellationToken>());

		result.Should().BeOfType<OrganisationDetailsResponseDto>();
	}

	[TestMethod]
	[DataRow(HttpStatusCode.BadRequest)]
	[DataRow(HttpStatusCode.Unauthorized)]
	[ExpectedException(typeof(ProblemResponseException))]
	public async Task GetOrganisationDetailsByOrgId_FailedStatusCode_ThrowHttpRequestException(HttpStatusCode statusCode)
	{
		var organsationId = Guid.NewGuid();

		var endpoint = $"api/organisations/organisation-with-persons/";
		var expectedUrl = $"{baseAddress}/{endpoint}{organsationId}";
        var problemDetails = new ProblemDetails();

		_httpMessageHandlerMock.Protected()
			 .Setup<Task<HttpResponseMessage>>("SendAsync",
				 ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
				 ItExpr.IsAny<CancellationToken>())
			 .ReturnsAsync(new HttpResponseMessage
			 {
				 Content = new StringContent(JsonSerializer.Serialize(problemDetails)),
				 StatusCode = statusCode
			 }).Verifiable();

		var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
		{
			BaseAddress = new Uri(baseAddress)
		};

		var sut = new ReprocessorExporterService(httpClient);

		await sut.GetOrganisationDetailsByOrgId(organsationId);
	}

	[TestMethod]
	public async Task GetOrganisationDetailsByOrgId_WhenRecordNotFound_ShouldReturnNull()
	{
		var organsationId = Guid.NewGuid();

		var endpoint = $"api/organisations/organisation-with-persons/";
		var expectedUrl = $"{baseAddress}/{endpoint}{organsationId}";

		_httpMessageHandlerMock.Protected()
			.Setup<Task<HttpResponseMessage>>("SendAsync",
				ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.ToString() == expectedUrl),
				ItExpr.IsAny<CancellationToken>())
			.ReturnsAsync(new HttpResponseMessage
			{
				StatusCode = HttpStatusCode.NoContent,
			}).Verifiable();

		var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
		{
			BaseAddress = new Uri(baseAddress)
		};

		var sut = new ReprocessorExporterService(httpClient);

		var result = await sut.GetOrganisationDetailsByOrgId(organsationId);

		_httpMessageHandlerMock.Protected().Verify("SendAsync", Times.Once(),
			ItExpr.Is<HttpRequestMessage>(
				req => req.Method == HttpMethod.Get &&
					   req.RequestUri != null &&
					   req.RequestUri.ToString() == expectedUrl),
			ItExpr.IsAny<CancellationToken>());

		result.Should().BeNull();

	}
}
