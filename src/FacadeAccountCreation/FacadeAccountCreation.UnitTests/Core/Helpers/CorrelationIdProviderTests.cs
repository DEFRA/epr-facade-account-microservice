using FacadeAccountCreation.Core.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Moq;

namespace FacadeAccountCreation.UnitTests.Core.Helpers;

[TestClass]
public class CorrelationIdProviderTests
{
    [TestMethod]
    public void WhenHttpContextRequestHasNoCorrelationId_ThenReturnNewGuid()
    {
        Mock<IHttpContextAccessor> httpContextAccessor = new();

        var provider = new CorrelationIdProvider(NullLogger<CorrelationIdProvider>.Instance, httpContextAccessor.Object);

        provider.GetHttpRequestCorrelationIdOrNew().Should().NotBe(Guid.Empty);
    }
    
    [TestMethod]
    public void WhenHttpContextRequestHasCorrelationId_ThenReturnIt()
    {
        Mock<IHttpContextAccessor> httpContextAccessor = new();

        var correlationId = Guid.NewGuid();
        
        httpContextAccessor
            .Setup(accessor => accessor.HttpContext.Request.Headers)
            .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "X-EPR-Correlation",  correlationId.ToString() }
            }));

        var provider = new CorrelationIdProvider(NullLogger<CorrelationIdProvider>.Instance, httpContextAccessor.Object);

        provider.GetHttpRequestCorrelationIdOrNew().Should().Be(correlationId);
    }
}