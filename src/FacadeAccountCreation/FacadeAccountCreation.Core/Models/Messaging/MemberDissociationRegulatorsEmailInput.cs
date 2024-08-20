
using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.Messaging
{
    [ExcludeFromCodeCoverage]
    public class MemberDissociationRegulatorsEmailInput
    {
        public string ComplianceSchemeName { get; set; }
        
        public string ComplianceSchemeNation { get; set; }
        
        public string OrganisationName { get; set; }
        
        public string OrganisationNation { get; set; }

        public string OrganisationNumber { get; set; }
        
        public Guid? UserId { get; set; }
        
        public void EnsureInitialised()
        {
            var errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(ComplianceSchemeName))
            {
                errors.Add("ComplianceScheme cannot be empty string. ");
            }
            
            if (string.IsNullOrWhiteSpace(ComplianceSchemeNation))
            {
                errors.Add("ComplianceSchemeNation cannot be empty string. ");
            }

            if (string.IsNullOrWhiteSpace(OrganisationName))
            {
                errors.Add("OrganisationName cannot be empty string. ");
            }
            
            if (string.IsNullOrWhiteSpace(OrganisationNation))
            {
                errors.Add("OrganisationNation cannot be empty string. ");
            }
            
            if (string.IsNullOrWhiteSpace(OrganisationNumber))
            {
                errors.Add("OrganisationNumber cannot be empty string. ");
            }

            if (errors.Count != 0)
            {
                throw new ArgumentException(string.Join(' ', errors));
            }
        }
    }
}