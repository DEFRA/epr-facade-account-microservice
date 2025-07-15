namespace FacadeAccountCreation.Core.Services.Enrolments;

public interface IEnrolmentService
{
    Task<HttpResponseMessage?> DeleteUser(DeleteUserModel model);
    Task<HttpResponseMessage?> DeletePersonConnectionAndEnrolment(DeleteUserModel model);
}
