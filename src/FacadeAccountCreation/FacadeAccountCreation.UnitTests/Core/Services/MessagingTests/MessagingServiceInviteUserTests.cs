using FacadeAccountCreation.Core.Models.Messaging;
using FluentAssertions;
using Moq;
using Notify.Exceptions;

namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests;

[TestClass]
public class MessagingServiceInviteUserTests : BaseMessagingTest
{
    [TestMethod]
    [DynamicData(nameof(ValidInviteUserEmailInput))]
    public void SendInviteUserEmail_WhenValidParameters_ShouldReturnCorrectId(InviteUserEmailInput inviteUserEmailInput)
    {
        // Arrange
        var emailNotificationId = "C123456";
        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<Dictionary<string, dynamic>>(), 
            null, 
            null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() { id = emailNotificationId });

        _sut = GetServiceUnderTest();
        
        // Act
        var result = _sut.SendInviteToUser(inviteUserEmailInput);
        
        // Assert
        result.Should().Be(emailNotificationId);
    }
    
    [TestMethod]
    [DynamicData(nameof(InvalidInviteUserEmailInputs))]
    [ExpectedException(typeof(ArgumentException))]
    public void SendInviteUserEmail_WhenInvalidParameters_ItShouldThrowArgumentException(InviteUserEmailInput inviteUserEmailInput)
    {
        // Arrange
        _sut = GetServiceUnderTest();

        // Act
        _ = _sut.SendInviteToUser(new InviteUserEmailInput()
        {
            UserId = inviteUserEmailInput.UserId,
            FirstName = inviteUserEmailInput.FirstName,
            LastName = inviteUserEmailInput.LastName,
            Recipient = inviteUserEmailInput.Recipient,
            OrganisationId = inviteUserEmailInput.OrganisationId,
            OrganisationName = inviteUserEmailInput.OrganisationName,
            JoinTheTeamLink = inviteUserEmailInput.JoinTheTeamLink
        });
    }
    
    [TestMethod]
    [DynamicData(nameof(ValidInviteUserEmailInput))]
    public void SendAccountCreationConfirmation_WhenNotifyFails_ItShouldReturnNull(InviteUserEmailInput inviteUserEmailInput)
    {
        // Arrange
        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null)).Throws(new NotifyClientException());

        _sut = GetServiceUnderTest();

        // Act
        var result = _sut.SendInviteToUser(inviteUserEmailInput);

        // Assert
        result.Should().Be(null);
    }
    
    private static IEnumerable<InviteUserEmailInput[]> ValidInviteUserEmailInput
    {
        get
        {
            yield return new InviteUserEmailInput[]
            {
                new InviteUserEmailInput()
                {
                    UserId = Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"),
                    FirstName = "Johnny",
                    LastName = "Ramone",
                    Recipient = "johnny@romone.com",
                    OrganisationId = Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"),
                    OrganisationName = "Some Organisation",
                    JoinTheTeamLink = "/some/url/x",
                    TemplateId = Guid.NewGuid().ToString()
                }
            };
        }
    }

    private static IEnumerable<object[]> InvalidInviteUserEmailParameters
    {
        get
        {
            yield return new object[] { Guid.Empty, "Johnny", "Ramone", "johnny@romone.com", Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"), "Some Organisation", "/some/url/x", "4c642e3e-f3cf-41a0-b9eb-4d6980999f5b"};
            yield return new object[] { Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"), null, "Ramone", "johnny@romone.com", Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"), "Some Organisation", "/some/url/x", "4c642e3e-f3cf-41a0-b9eb-4d6980999f5b"};
            yield return new object[] { Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"), string.Empty, "Ramone", "johnny@romone.com", Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"), "Some Organisation", "/some/url/x", "4c642e3e-f3cf-41a0-b9eb-4d6980999f5b"};
            yield return new object[] { Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"), "Johnny", null, "johnny@romone.com", Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"), "Some Organisation", "/some/url/x", "4c642e3e-f3cf-41a0-b9eb-4d6980999f5b"};
            yield return new object[] { Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"), "Johnny", string.Empty, "johnny@romone.com", Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"), "Some Organisation", "/some/url/x", "4c642e3e-f3cf-41a0-b9eb-4d6980999f5b"};
            yield return new object[] { Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"), "Johnny", "Ramone", null, Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"), "Some Organisation", "/some/url/x", "4c642e3e-f3cf-41a0-b9eb-4d6980999f5b"};
            yield return new object[] { Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"), "Johnny", "Ramone", string.Empty, Guid.Empty, "Some Organisation", "/some/url/x", "4c642e3e-f3cf-41a0-b9eb-4d6980999f5b"};
            yield return new object[] { Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"), "Johnny", "Ramone", "johnny@romone.com", Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"), null, "/some/url/x", "4c642e3e-f3cf-41a0-b9eb-4d6980999f5b"};
            yield return new object[] { Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"), "Johnny", "Ramone", "johnny@romone.com", Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"), string.Empty, "/some/url/x", "4c642e3e-f3cf-41a0-b9eb-4d6980999f5b"};
            yield return new object[] { Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"), "Johnny", "Ramone", "johnny@romone.com", Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"), "Some Organisation", null, "4c642e3e-f3cf-41a0-b9eb-4d6980999f5b"};
            yield return new object[] { Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"), "Johnny", "Ramone", "johnny@romone.com", Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"), "Some Organisation", string.Empty, "4c642e3e-f3cf-41a0-b9eb-4d6980999f5b"};
            yield return new object[] { Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"), "Johnny", "Ramone", "johnny@romone.com", Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"), "Some Organisation", string.Empty, null};
            yield return new object[] { Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"), "Johnny", "Ramone", "johnny@romone.com", Guid.Parse("2b2706e2-fa13-4810-9b1c-fc3e4a07048e"), "Some Organisation", string.Empty, string.Empty};
        }
    }
    
    private static IEnumerable<InviteUserEmailInput[]> InvalidInviteUserEmailInputs
    {
        get
        {
            var invalidInviteUserEmailInputs = new List<InviteUserEmailInput>();
            foreach (var x in InvalidInviteUserEmailParameters)
            {
                invalidInviteUserEmailInputs.Add(new InviteUserEmailInput()
                {
                    UserId = (Guid) x[0],
                    FirstName = (string) x[1],
                    LastName = (string) x[2],
                    Recipient = (string) x[3],
                    OrganisationId = (Guid) x[4],
                    OrganisationName = (string) x[5],
                    JoinTheTeamLink = (string) x[6]
                });
            }
            return invalidInviteUserEmailInputs.Select(x => new[] {x});
        }
    }
}
