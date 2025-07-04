using FacadeAccountCreation.API.Handlers;
using FacadeAccountCreation.Core.Services.AddressLookup;
using FacadeAccountCreation.Core.Services.CompaniesHouse;
using FacadeAccountCreation.Core.Services.ComplianceScheme;
using FacadeAccountCreation.Core.Services.Enrolments;
using FacadeAccountCreation.Core.Services.Notification;
using FacadeAccountCreation.Core.Services.ReprocessorExporter;
using FacadeAccountCreation.Core.Services.RoleManagement;
using FacadeAccountCreation.Core.Services.User;

namespace FacadeAccountCreation.API.Extensions;

public static class HttpClientServiceCollectionExtension
{
    public static IServiceCollection AddServicesAndHttpClients(this IServiceCollection services)
    {
        services.AddTransient<AccountServiceAuthorisationHandler>();
        services.AddTransient<AddressLookupCredentialHandler>();
        services.AddTransient<CompaniesHouseCredentialHandler>();

        services.AddHttpClient<IAddressLookupService, AddressLookupService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

            client.BaseAddress = new Uri(config.AddressLookupBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);
        })
            .AddHttpMessageHandler<AddressLookupCredentialHandler>();

        services.AddHttpClient<ICompaniesHouseLookupService, CompaniesHouseLookupService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

            client.BaseAddress = new Uri(config.CompaniesHouseLookupBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);
        })
            .AddHttpMessageHandler<CompaniesHouseCredentialHandler>();

        services.AddHttpClient<IComplianceSchemeService, ComplianceSchemeService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

            client.BaseAddress = new Uri(config.AccountServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);
        })
        .AddHttpMessageHandler<AccountServiceAuthorisationHandler>();

        services.AddHttpClient<IUserService, UserService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

            client.BaseAddress = new Uri(config.AccountServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);
        })
        .AddHttpMessageHandler<AccountServiceAuthorisationHandler>();
        
        services.AddHttpClient<IEnrolmentService, EnrolmentService>((sp, client) =>
            {
                var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

                client.BaseAddress = new Uri(config.AccountServiceBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(config.Timeout);
            })
        .AddHttpMessageHandler<AccountServiceAuthorisationHandler>();

        services.AddHttpClient<IAccountService, AccountService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

            client.BaseAddress = new Uri(config.AccountServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);
        })
        .AddHttpMessageHandler<AccountServiceAuthorisationHandler>();

        services.AddHttpClient<IPersonService, PersonService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

            client.BaseAddress = new Uri(config.AccountServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);
        })
        .AddHttpMessageHandler<AccountServiceAuthorisationHandler>();

        services.AddHttpClient<IOrganisationService, OrganisationService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

            client.BaseAddress = new Uri(config.AccountServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);
        })
        .AddHttpMessageHandler<AccountServiceAuthorisationHandler>();

        services.AddHttpClient<IRoleManagementService, RoleManagementService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

            client.BaseAddress = new Uri(config.AccountServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);
        })
        .AddHttpMessageHandler<AccountServiceAuthorisationHandler>();

        services.AddHttpClient<INotificationsService, NotificationsService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

            client.BaseAddress = new Uri(config.AccountServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);
        })
        .AddHttpMessageHandler<AccountServiceAuthorisationHandler>();

		services.AddHttpClient<IReprocessorExporterService, ReprocessorExporterService>((sp, client) =>
		{
			var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

			client.BaseAddress = new Uri(config.AccountServiceBaseUrl);
			client.Timeout = TimeSpan.FromSeconds(config.Timeout);
		})
		.AddHttpMessageHandler<AccountServiceAuthorisationHandler>();

		return services;
    }
}
