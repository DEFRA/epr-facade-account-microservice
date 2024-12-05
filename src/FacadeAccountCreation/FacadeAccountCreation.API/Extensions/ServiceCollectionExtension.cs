using FacadeAccountCreation.Core.Configs;
using FacadeAccountCreation.Core.Helpers;
using FacadeAccountCreation.Core.Models.ServiceRolesLookup;
using Notify.Client;
using Notify.Interfaces;

namespace FacadeAccountCreation.API.Extensions;

public static class ServiceCollectionExtension
{
    public static void RegisterComponents(this IServiceCollection services, IConfiguration configuration)
    {
        RegisterConfigs(services, configuration);
        RegisterServices(services, configuration);
    }

    private static void RegisterConfigs(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ApiConfig>(configuration.GetSection(ApiConfig.SectionName));
        services.Configure<AccountsEndpointsConfig>(configuration.GetSection(AccountsEndpointsConfig.SectionName));
        services.Configure<MessagingConfig>(configuration.GetSection(MessagingConfig.SectionName));
        services.Configure<ServiceRolesConfig>(configuration.GetSection(ServiceRolesConfig.SectionName));
        services.Configure<ConnectionsEndpointsConfig>(configuration.GetSection(ConnectionsEndpointsConfig.SectionName));
        services.Configure<RegulatorEmailConfig>(configuration.GetSection(RegulatorEmailConfig.SectionName));
        services.Configure<EprPackagingRegulatorEmailConfig>(configuration.GetSection(EprPackagingRegulatorEmailConfig.SectionName));
        services.Configure<EnrolmentApplicationEndpointsConfig>(configuration.GetSection(EnrolmentApplicationEndpointsConfig.SectionName));
    }

    private static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<INotificationClient>(_ => new NotificationClient(configuration.GetValue<string>("MessagingConfig:ApiKey")));
        services.AddSingleton<IMessagingService, MessagingService>();
        services.AddSingleton<IServiceRolesLookupService, ServiceRolesLookupService>();
        services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();
    }
}
