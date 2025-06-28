namespace FacadeAccountCreation.Core.Models.User;

public class EnrolmentModel
{
    public int? EnrolmentId { get; set; }
    
    public string? EnrolmentStatus { get; set; }

    public string? ServiceRole { get; set; }

	public string? ServiceRoleKey { get; set; }

	public string? Service { get; set; }

    public int? ServiceRoleId { get; set; }
}