namespace FacadeAccountCreation.Core.Models.CreateAccount;

public class ReprocessorExporterUserModel
{
    public Guid UserId { get; set; }

    /// <summary>
    /// See https://developer-specs.company-information.service.gov.uk/companies-house-public-data-api/resources/officersummary?v=latest
    /// </summary>
    public string? JobTitle { get; set; }
   
    public bool IsApprovedUser { get; set; }
}
