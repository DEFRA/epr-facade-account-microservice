using FacadeAccountCreation.Core.Models.Enrolments;

namespace FacadeAccountCreation.Core.Services.Enrolments;

public interface IEnrolmentService
{
    Task<HttpResponseMessage?> DeleteUser(DeleteUserModel model);
}
