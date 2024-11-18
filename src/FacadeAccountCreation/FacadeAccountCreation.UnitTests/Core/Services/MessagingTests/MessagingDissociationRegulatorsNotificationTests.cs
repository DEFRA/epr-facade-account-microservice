namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests;

[TestClass]
public class MessagingServiceDissociationRegulatorsNotificationTests : BaseMessagingTest
{
    [TestMethod]
    [DynamicData(nameof(ValidSameNationMemberDissociationRegulatorsEmailInput))]
    public void MemberDissociationRegulatorsEmail_WhenSameNation_ShouldSendOneEmail(MemberDissociationRegulatorsEmailInput memberDissociationRegulatorsEmailInput)
    {
        // Arrange
        var emailNotificationId = "C123456";
        var recipient = "dummyemailengland";
        var templateId = "dummytemplate";
        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(), 
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(), 
            null, 
            null, null)).Returns(new EmailNotificationResponse { id = emailNotificationId });

        _sut = GetServiceUnderTest();
        
        // Act
        var result = _sut.SendMemberDissociationRegulatorsNotification(memberDissociationRegulatorsEmailInput);
        
        // Assert
        _notificationClientMock.Verify(n => n.SendEmail(recipient, templateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Once);
        result.Count.Should().Be(1);
    }
    
    [TestMethod]
    [DynamicData(nameof(ValidDifferentNationMemberDissociationRegulatorsEmailInput))]
    public void MemberDissociationRegulatorsEmail_WhenDifferentNation_ShouldSendOneEmail(MemberDissociationRegulatorsEmailInput memberDissociationRegulatorsEmailInput)
    {
        // Arrange
        var emailNotificationId = "C123456";
        var recipientOne = "dummyemailwales";
        var recipientTwo = "dummyemailscotland";
        var templateId = "dummytemplate";
        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<Dictionary<string, dynamic>>(), 
            null, 
            null, null)).Returns(new EmailNotificationResponse { id = emailNotificationId });

        _sut = GetServiceUnderTest();
        
        // Act
        var result = _sut.SendMemberDissociationRegulatorsNotification(memberDissociationRegulatorsEmailInput);
        
        // Assert
        _notificationClientMock.Verify(n => n.SendEmail(recipientOne, templateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Once);
        _notificationClientMock.Verify(n => n.SendEmail(recipientTwo, templateId, It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null), Times.Once);
        result.Count.Should().Be(2);
    }
    
    private static IEnumerable<MemberDissociationRegulatorsEmailInput[]> ValidSameNationMemberDissociationRegulatorsEmailInput
    {
        get
        {
            yield return
            [
                new MemberDissociationRegulatorsEmailInput
                {
                    UserId = Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"),
                    ComplianceSchemeName = "Compliance Scheme Name",
                    ComplianceSchemeNation = "England",
                    OrganisationName = "Organisation",
                    OrganisationNation = "England",
                    OrganisationNumber = "1"
                }
            ];
        }
    }
    
    private static IEnumerable<MemberDissociationRegulatorsEmailInput[]> ValidDifferentNationMemberDissociationRegulatorsEmailInput
    {
        get
        {
            yield return
            [
                new MemberDissociationRegulatorsEmailInput
                {
                    UserId = Guid.Parse("fd4c6137-dd77-4684-a7cd-c9ae7f4c7763"),
                    ComplianceSchemeName = "Compliance Scheme Name",
                    ComplianceSchemeNation = "Wales",
                    OrganisationName = "Organisation",
                    OrganisationNation = "Scotland",
                    OrganisationNumber = "1"
                }
            ];
        }
    }
}
