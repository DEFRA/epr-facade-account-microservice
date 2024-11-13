using Notify.Interfaces;

namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests;

public class BaseMessagingTest
{
    protected Mock<INotificationClient> _notificationClientMock;
    protected IOptions<MessagingConfig> _messagingConfigOptions;
    protected IOptions<RegulatorEmailConfig> _regulatorEmailConfig;
    protected IOptions<EprPackagingRegulatorEmailConfig> _eprPackagingRegulatorEmailConfig;
    protected readonly NullLogger<MessagingService> _nullLogger = new();
    protected MessagingService _sut = default!;
    
    protected BaseMessagingTest()
    {
        _notificationClientMock = new Mock<INotificationClient>();
        _messagingConfigOptions = Options.Create(new MessagingConfig
        {
            ApiKey = "dummykey",
            ComplianceSchemeAccountConfirmationTemplateId = "dummytemplate",
            ProducerAccountConfirmationTemplateId = "dummytemplate",
            MemberDissociationRegulatorsTemplateId = "dummytemplate",
            MemberDissociationProducersTemplateId = "dummytemplate",
            ComplianceSchemeResubmissionTemplateId = "ComplianceSchemeResubmissionTemplateId",
            ProducerResubmissionTemplateId = "ProducerResubmissionTemplateId",
            AccountLoginUrl="dummyUrl",
            AccountCreationUrl= "dummyCreateUrl"
        });
        _regulatorEmailConfig = Options.Create(new RegulatorEmailConfig
        {
            England = "dummyEmailEngland",
            Scotland = "dummyEmailScotland",
            Wales = "dummyEmailWales",
            NorthernIreland = "dummyEmailNI"
        });
        _eprPackagingRegulatorEmailConfig = Options.Create(new EprPackagingRegulatorEmailConfig
        {
            England = "dummyEmailEngland",
            Scotland = "dummyEmailScotland",
            Wales = "dummyEmailWales",
            NorthernIreland = "dummyEmailNI"
        });
    }
    
    protected MessagingService GetServiceUnderTest()
    {
        return new MessagingService(_notificationClientMock.Object, _messagingConfigOptions, _regulatorEmailConfig, _eprPackagingRegulatorEmailConfig, _nullLogger);
    }
}