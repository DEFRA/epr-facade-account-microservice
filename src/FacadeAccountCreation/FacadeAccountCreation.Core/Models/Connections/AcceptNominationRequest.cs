using System.ComponentModel.DataAnnotations;

namespace FacadeAccountCreation.Core.Models.Connections
{
    public class AcceptNominationRequest
    {
        [Required]
        [MaxLength(50)]
        public string? Telephone { get; set; }

        [Required]
        [MaxLength(450)]
        public string? NomineeDeclaration { get; set; }
    }
}