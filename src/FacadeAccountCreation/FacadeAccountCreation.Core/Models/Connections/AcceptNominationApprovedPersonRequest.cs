using System.ComponentModel.DataAnnotations;

namespace FacadeAccountCreation.Core.Models.Connections
{
    public class AcceptNominationApprovedPersonRequest
    {
        [Required]
        [MaxLength(450)]
        public string? JobTitle { get; set; }   

        [Required]
        [MaxLength(50)]
        public string? Telephone { get; set; }

        [Required]
        [MaxLength(450)]
        public string? DeclarationFullName { get; set; }

        [Required]
        public DateTime? DeclarationTimeStamp { get; set; }
    }
}