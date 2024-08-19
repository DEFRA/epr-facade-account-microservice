namespace FacadeAccountCreation.Core.Models.Organisations
{
    public class ExportOrganisationSubsidiariesResponseModel
    {
        public int OrganisationId { get; set; }

        public int? SubsidiaryId { get; set; }

        public string OrganisationName { get; set; }

        public string CompaniesHouseNumber { get; set; }
    }
}
