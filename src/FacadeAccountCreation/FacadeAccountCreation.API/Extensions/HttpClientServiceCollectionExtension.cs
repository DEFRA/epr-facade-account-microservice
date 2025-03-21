﻿using FacadeAccountCreation.API.Handlers;
using FacadeAccountCreation.Core.Constants;
using FacadeAccountCreation.Core.Services.AddressLookup;
using FacadeAccountCreation.Core.Services.CompaniesHouse;
using FacadeAccountCreation.Core.Services.ComplianceScheme;
using FacadeAccountCreation.Core.Services.Enrolments;
using FacadeAccountCreation.Core.Services.Notification;
using FacadeAccountCreation.Core.Services.RoleManagement;
using FacadeAccountCreation.Core.Services.User;
using Microsoft.FeatureManagement;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace FacadeAccountCreation.API.Extensions;

public static class HttpClientServiceCollectionExtension
{
    public static IServiceCollection AddServicesAndHttpClients(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();

        var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();
        var useBoomiOAuth = featureManager.IsEnabledAsync(FeatureFlags.UseBoomiOAuth).GetAwaiter().GetResult();

        services.AddTransient<AccountServiceAuthorisationHandler>();
        services.AddTransient<AddressLookupCredentialHandler>();
        services.AddTransient<CompaniesHouseCredentialHandler>();

        services.AddHttpClient<IAddressLookupService, AddressLookupService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

            client.BaseAddress = new Uri(config.AddressLookupBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);
        })
        .AddAddressLookupCredentialHandler(useBoomiOAuth);

        services.AddHttpClient<ICompaniesHouseLookupService, CompaniesHouseLookupService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<ApiConfig>>().Value;

            client.BaseAddress = new Uri(config.CompaniesHouseLookupBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);
        })
            .AddCompaniesHouseCredentialHandler(useBoomiOAuth);

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

        return services;
    }

    public static IHttpClientBuilder AddAddressLookupCredentialHandler(this IHttpClientBuilder builder, bool useBoomiOAuth)
    {
        if (useBoomiOAuth)
        {
            builder.AddHttpMessageHandler<AddressLookupCredentialHandler>();
        }
        else
        {
            builder.ConfigurePrimaryHttpMessageHandler(GetClientCertificateHandler);
        }

        return builder;
    }

    public static IHttpClientBuilder AddCompaniesHouseCredentialHandler(this IHttpClientBuilder builder, bool useBoomiOAuth)
    {
        if (useBoomiOAuth)
        {
            builder.AddHttpMessageHandler<CompaniesHouseCredentialHandler>();
        }
        else
        {
            builder.ConfigurePrimaryHttpMessageHandler(GetClientCertificateHandler);
        }

        return builder;
    }

    private static HttpMessageHandler GetClientCertificateHandler(IServiceProvider sp)
    {
        if (sp == null)
        {
            throw new ArgumentException("ServiceProvider must not be null");
        }

        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.SslProtocols = SslProtocols.Tls12;
        handler.ClientCertificates
            .Add(new X509Certificate2(
                Convert.FromBase64String(sp.GetRequiredService<IOptions<ApiConfig>>().Value.Certificate))
            );

        return handler;
    }
}
