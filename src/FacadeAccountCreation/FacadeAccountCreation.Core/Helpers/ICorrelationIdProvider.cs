namespace FacadeAccountCreation.Core.Helpers;

public interface ICorrelationIdProvider
{
    public Guid GetHttpRequestCorrelationIdOrNew();
}