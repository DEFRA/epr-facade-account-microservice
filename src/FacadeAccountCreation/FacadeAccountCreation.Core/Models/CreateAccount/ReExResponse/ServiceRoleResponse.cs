namespace FacadeAccountCreation.Core.Models.CreateAccount.ReExResponse;

[ExcludeFromCodeCoverage]
public record ServiceRoleResponse
{
    public string? Key { get; set; }

    /// <summary>
    /// Role name can be as 'Approved Person', 'Admin user' etc.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Descriotion related to role name
    /// </summary>
    public string? Description { get; set; }
}
