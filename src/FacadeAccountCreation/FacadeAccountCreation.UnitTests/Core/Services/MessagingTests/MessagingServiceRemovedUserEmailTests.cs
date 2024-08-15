using FacadeAccountCreation.Core.Models.Enrolments;
using FluentAssertions;
using Moq;
using Notify.Exceptions;

namespace FacadeAccountCreation.UnitTests.Core.Services.MessagingTests;

[TestClass]
public class MessagingServiceRemovedUserEmailTests : BaseMessagingTest
{
    [TestMethod]
   public void SendRemovedUserNotification_WhenValidParameters_ItShouldReturnCorrectId()
    {
        string expectedId = "P123456";
        
        _notificationClientMock.Setup(nc => nc.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, dynamic>>(),
            null,
            null, null)).Returns(new Notify.Models.Responses.EmailNotificationResponse() { id = expectedId });

        _sut = GetServiceUnderTest();

        var emailInput = new RemovedUserNotificationEmailModel
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            RecipientEmail = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = "100 125",
            CompanyName = "The Ramones",
            UserId = Guid.NewGuid()
        };
        var result = _sut.SendRemovedUserNotification(emailInput);

        result.Should().Be(expectedId);
    }
   
   [TestMethod]
   [ExpectedException(typeof(ArgumentException))]
   public void SendRemovedUserNotification_WhenInvalidParameters_ItShouldThrowArgumentException()
   {
       _sut = GetServiceUnderTest();

       var emailInput = new RemovedUserNotificationEmailModel
       {
           FirstName = "Johnny",
           LastName = "Ramone",
           RecipientEmail = "johnny@ramones.com",
           TemplateId = Guid.NewGuid().ToString(),
           OrganisationId = "100 125",
            UserId = Guid.NewGuid()
       };
       _ = _sut.SendRemovedUserNotification(emailInput);
   }
   
   [TestMethod]
   public void SendRemovedUserNotification_WhenNotifyFails_ItShouldReturnNull()
   {
       _notificationClientMock.Setup(nc => nc.SendEmail(
           It.IsAny<string>(),
           It.IsAny<string>(),
           It.IsAny<Dictionary<string, dynamic>>(),
           null,
           null, null)).Throws(new NotifyClientException());

       _sut = GetServiceUnderTest();

       var emailInput = new RemovedUserNotificationEmailModel
       {
           FirstName = "Johnny",
           LastName = "Ramone",
           RecipientEmail = "johnny@ramones.com",
           TemplateId = Guid.NewGuid().ToString(),
           OrganisationId = "100 125",
           UserId = Guid.NewGuid(),
           CompanyName = "The Ramones",
       };
     
       var result = _sut.SendRemovedUserNotification(emailInput);

       result.Should().Be(null);
   }
   
   [TestMethod]
   [ExpectedException(typeof(ArgumentException))]
   public void SendRemovedUserNotification_WhenInvalidOrganisationId_ItShouldThrowException()
   {
       _sut = GetServiceUnderTest();

       var emailInput = new RemovedUserNotificationEmailModel
       {
           FirstName = "Johnny",
           LastName = "Ramone",
           RecipientEmail = "johnny@ramones.com",
           TemplateId = Guid.NewGuid().ToString(),
           OrganisationId = string.Empty,
           CompanyName = "The Company",
           UserId = Guid.NewGuid()
       };
       _ = _sut.SendRemovedUserNotification(emailInput);

   }
   
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInvalidUserId_ItShouldThrowException()
    {
        _sut = GetServiceUnderTest();

        var emailInput = new RemovedUserNotificationEmailModel
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            RecipientEmail = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = "100 125",
            CompanyName = "The Ramones",
            UserId = Guid.Empty
        };
        _ = _sut.SendRemovedUserNotification(emailInput);

    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInvalidUserEmail_ItShouldThrowException()
    {
        _sut = GetServiceUnderTest();

        var emailInput = new RemovedUserNotificationEmailModel
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            RecipientEmail = string.Empty,
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = "100 125",
            CompanyName = "The Ramones",
            UserId = Guid.NewGuid()
        };
        _ = _sut.SendRemovedUserNotification(emailInput);

    }


    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInvalidTemplateId_ItShouldThrowException()
    {
       _sut = GetServiceUnderTest();

       var emailInput = new RemovedUserNotificationEmailModel
       {
           FirstName = "Johnny",
           LastName = "Ramone",
           RecipientEmail = "johnny@ramones.com",
           TemplateId = string.Empty,
           OrganisationId = "100 125",
           CompanyName = "The Ramones",
           UserId = Guid.NewGuid()
       };
       _ = _sut.SendRemovedUserNotification(emailInput);

    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInvalidNominatorFirstName_ItShouldThrowException()
    {
        _sut = GetServiceUnderTest();

        var emailInput = new RemovedUserNotificationEmailModel
        {
            FirstName = string.Empty,
            LastName = "Ramone",
            RecipientEmail = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = "100 125",
            CompanyName = "The Ramones",
            UserId = Guid.NewGuid()
        };
        _ = _sut.SendRemovedUserNotification(emailInput);

    }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInvalidNominatorSurname_ItShouldThrowException()
    {
        _sut = GetServiceUnderTest();

        var emailInput = new RemovedUserNotificationEmailModel
        {
            FirstName = "Jonny",
            LastName = string.Empty,
            RecipientEmail = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = "100 125",
            CompanyName = "The Ramones",
            UserId = Guid.NewGuid()
        };
        _ = _sut.SendRemovedUserNotification(emailInput);

    }
   
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInvalidCompanyName_ItShouldThrowException()
    {
        _sut = GetServiceUnderTest();

        var emailInput = new RemovedUserNotificationEmailModel
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            RecipientEmail = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = "100 125",
            CompanyName = string.Empty,
            UserId = Guid.NewGuid()
        };
        _ = _sut.SendRemovedUserNotification(emailInput);

    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenInvalidOrganisationId_ItShouldThrowException()
    {
        _sut = GetServiceUnderTest();

        var emailInput = new RemovedUserNotificationEmailModel
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            RecipientEmail = "johnny@ramones.com",
            TemplateId = Guid.NewGuid().ToString(),
            OrganisationId = string.Empty,
            CompanyName = "The Ramones",
            UserId = Guid.NewGuid()
        };
        _ = _sut.SendRemovedUserNotification(emailInput);

    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void SendDelegatedUserNomination_WhenNullemplateId_ItShouldThrowException()
    {
        _sut = GetServiceUnderTest();

        var emailInput = new RemovedUserNotificationEmailModel
        {
            FirstName = "Johnny",
            LastName = "Ramone",
            RecipientEmail = "johnny@ramones.com",
            TemplateId = null,
            OrganisationId = "100 125",
            CompanyName = "The Ramones",
            UserId = Guid.NewGuid()
        };
        _ = _sut.SendRemovedUserNotification(emailInput);
    }
}