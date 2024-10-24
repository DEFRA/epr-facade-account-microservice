using FacadeAccountCreation.Core.Models.PaymentCalculation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FacadeAccountCreation.Core.Services.PaymentCalculation
{
    public class PaymentCalculationService : IPaymentCalculationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentCalculationService> _logger;
        private readonly IConfiguration _config;

        public PaymentCalculationService(
            HttpClient httpClient,
            ILogger<PaymentCalculationService> logger,
            IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
        }

        public async Task<PaymentCalculationResponse> ProducerRegistrationFees(PaymentCalculationRequest paymentCalculationRequest)
        {
            try
            {
                var url = $"{_config.GetSection("PaymentCalculationEndpoints").GetSection("PayCalEndpointsConfig").Value}";

                _logger.LogInformation(message: "Attempting to calculate producer registration fee for {ApplicationReferenceNumber}", paymentCalculationRequest.ApplicationReferenceNumber);

                var response = await _httpClient.PostAsJsonAsync(url, paymentCalculationRequest);

                if (response.StatusCode == HttpStatusCode.NotFound) return null;

                response.EnsureSuccessStatusCode();

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var jsonContent = RemoveDecimalValues(await response.Content.ReadAsStringAsync());

                return JsonSerializer.Deserialize<PaymentCalculationResponse>(jsonContent, options);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to calculate producer registration fee for {ApplicationReferenceNumber}", paymentCalculationRequest.ApplicationReferenceNumber);
                throw;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
            }

            return null;
        }

        private string RemoveDecimalValues(string jsonString)
        {
            return Regex.Replace(jsonString, @"(\d+)\.0+", "$1");
        }

    }
}
