using FacadeAccountCreation.Core.Enums;

namespace FacadeAccountCreation.Core.Models.Connections
{
    public class ConnectionWithEnrolmentsModel
    {
        public PersonRole PersonRole { get; set; }
        public Guid UserId { get; set; }
        public ICollection<EnrolmentsFromConnection> Enrolments { get; init; }
    }

    public class EnrolmentsFromConnection
    {
        public string ServiceRoleKey { get; set; }
        public EnrolmentStatus EnrolmentStatus { get; set; }
    }
}
