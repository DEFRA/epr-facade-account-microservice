namespace FacadeAccountCreation.Core.Models.Organisations
{
    public class OrganisationSubsidiariesResponseModel
    {
        public int org_id { get; set; }

        public string org_name { get; set; }

        public string subsidiary_id { get; set; }

        public string subsidiary_name { get; set; }

        public string companies_house { get; set; }
    }
}
