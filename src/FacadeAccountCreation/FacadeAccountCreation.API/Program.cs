using EPR.Common.Logging.Extensions;
using FacadeAccountCreation.API.Extensions;
using FacadeAccountCreation.API.HealthChecks;
using FacadeAccountCreation.API.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFeatureManagement();

builder.Services
    .AddApplicationInsightsTelemetry()
    .AddHealthChecks();

builder.Services.RegisterComponents(builder.Configuration);
builder.Services.ConfigureLogging();

// Services & HttpClients
builder.Services.AddServicesAndHttpClients();

// Logging
builder.Services.AddLogging();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
    }, options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
    });

// Authorization
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("FallbackPolicy", policy => policy.RequireAuthenticatedUser());

// General Config
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    options.OperationFilter<AddAuthHeaderOperationFilter>();
});

// App
var app = builder.Build();

app.UseExceptionHandler(app.Environment.IsDevelopment() ? "/error-development" : "/error");

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks(
    builder.Configuration.GetValue<string>("HealthCheckPath"),
    HealthCheckOptionBuilder.Build()).AllowAnonymous();

app.Run();
