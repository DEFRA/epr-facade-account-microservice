using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;

namespace FacadeAccountCreation.API.Handlers;

public class ClientSecretCredentialHandler : DelegatingHandler
{
    private readonly TokenRequestContext _tokenRequestContext;
    private readonly ClientSecretCredential _credentials;

    public ClientSecretCredentialHandler(IOptions<ApiConfig> options)
    {
        var clientApiOptions = options.Value;
        _tokenRequestContext = new TokenRequestContext(new[] { clientApiOptions.Scope });
        _credentials = new ClientSecretCredential(clientApiOptions.TenantId, clientApiOptions.ClientId, clientApiOptions.ClientSecret);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenResult = await _credentials.GetTokenAsync(_tokenRequestContext, cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue(Constants.Client.Bearer, tokenResult.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}
