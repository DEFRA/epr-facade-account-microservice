namespace FacadeAccountCreation.UnitTests.TestHelpers;

public static class ControllerContextHelpers
{
    public static void AddDefaultContextWithOid(this ControllerBase controller, Guid oid, string authType)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        
        var claims = new List<Claim> { new(ClaimConstants.ObjectId, oid.ToString()) };
        var identity = new ClaimsIdentity(claims, authType);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        controller.ControllerContext.HttpContext.User = claimsPrincipal;
    }
}
