using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using FacadeAccountCreation.Core.Models.PaymentCalculation;
using Microsoft.Extensions.Logging;

namespace FacadeAccountCreation.Core.Services.PaymentCalculation;

public class PaymentCalculationService(
    HttpClient httpClient,
    ILogger<PaymentCalculationService> logger)
    : IPaymentCalculationService
{
    private const string ProducerRegistrationFeesUri = "api/producer/registration-fee";
    private const string PaymentInitiationUrl = "api/organisations/nation-code";
    private readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<PaymentCalculationResponse?> ProducerRegistrationFees(PaymentCalculationRequest paymentCalculationRequest)
    {
        try
        {
            logger.LogInformation(message: "Attempting to calculate producer registration fee for {Reference}", paymentCalculationRequest.ApplicationReferenceNumber);

            var response = await httpClient.PostAsJsonAsync(ProducerRegistrationFeesUri, paymentCalculationRequest);

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var jsonContent = RemoveDecimalValues(await response.Content.ReadAsStringAsync());

            return JsonSerializer.Deserialize<PaymentCalculationResponse>(jsonContent, _options);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to calculate producer registration fee for {Reference}", paymentCalculationRequest.ApplicationReferenceNumber);
            throw;
        }
        finally
        {
            httpClient.DefaultRequestHeaders.Clear();
        }
    }

    public async Task<string?> PaymentInitiation(PaymentInitiationRequest paymentInitiationRequest)
    {
        try
        {
            logger.LogInformation(message: "Attempting to initialise Payment request for {Reference}", paymentInitiationRequest.Reference);

            var response = await httpClient.PostAsJsonAsync(PaymentInitiationUrl, paymentInitiationRequest);
            
            response.EnsureSuccessStatusCode();

            var htmlContent = await response.Content.ReadAsStringAsync();

            const string pattern = @"window\.location\.href\s*=\s*'(?<url>.*?)';";
            
            var match = Regex.Match(htmlContent, pattern);

            // Extract the URL if the match is found
            if (match.Success)
            {
                return match.Groups["url"].Value;
            }
            else
            {
                logger.LogWarning("Redirect URL not found in the initialise Payment response.");
                return null;
            }
        }
        finally
        {
            httpClient.DefaultRequestHeaders.Clear();
        }
    }

    private static string RemoveDecimalValues(string jsonString)
    {
        return Regex.Replace(jsonString, @"(\d+)\.0+", "$1");
    }
}