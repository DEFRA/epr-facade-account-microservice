using System.Text.RegularExpressions;
using FacadeAccountCreation.Core.Models.PaymentCalculation;

namespace FacadeAccountCreation.Core.Services.PaymentCalculation;

public class PaymentCalculationService(
    HttpClient httpClient,
    ILogger<PaymentCalculationService> logger)
    : IPaymentCalculationService
{
    private const string ProducerRegistrationFeesUri = "/api/V1/producer/registration-fee";
    private const string ComplianceSchemeRegistrationFeesUri = "/api/V1/compliance-scheme/registration-fee";

    private const string PaymentInitiationUrl = "/api/V1/online-payments";
    private readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    // ReSharper disable once MemberCanBePrivate.Global
    public bool ReturnFakeData { get; set; } = true;

    public async Task<PaymentCalculationResponse?> ProducerRegistrationFees(PaymentCalculationRequest paymentCalculationRequest)
    {
        try
        {
            if (ReturnFakeData)
                return new PaymentCalculationResponse { OutstandingPayment = 10, PreviousPayment = 20, ProducerLateRegistrationFee = 30, ProducerOnlineMarketPlaceFee = 50, ProducerRegistrationFee = 100, SubsidiariesFee = 200, SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdown { CountOfOMPSubsidiaries = 5, FeeBreakdowns = [new FeeBreakdown { BandNumber = 1, TotalPrice = 50, UnitCount = 1, UnitPrice = 50 }], TotalSubsidiariesOMPFees = 500, UnitOMPFees = 5} };

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

    public async Task<ComplianceSchemePaymentCalculationResponse?> ComplianceSchemeRegistrationFees(ComplianceSchemePaymentCalculationRequest paymentCalculationRequest)
    {
        try
        {
            if (ReturnFakeData)
                #region Return Compliance Scheme Dummy Data
                return new ComplianceSchemePaymentCalculationResponse
                {
                    TotalFee = 20,
                    ComplianceSchemeRegistrationFee = 10,
                    ComplianceSchemeMembersWithFees = new List<ComplianceSchemePaymentCalculationResponseMember>
                    {
                        new ComplianceSchemePaymentCalculationResponseMember
                        {
                            MemberId = "1",
                            MemberLateRegistrationFee = 30,
                            MemberOnlineMarketPlaceFee = 15,
                            MemberRegistrationFee = 40,
                            SubsidiariesFee = 45,
                            TotalMemberFee = 100,
                            SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdown
                            {
                                CountOfOMPSubsidiaries = 4,
                                TotalSubsidiariesOMPFees = 5,
                                UnitOMPFees = 6,
                                FeeBreakdowns = new FeeBreakdown[2]
                                {
                                    new FeeBreakdown
                                    {
                                        BandNumber = 1,
                                        TotalPrice = 2,
                                        UnitCount = 3,
                                        UnitPrice = 4
                                    },
                                    new FeeBreakdown
                                    {
                                        BandNumber = 11,
                                        TotalPrice = 12,
                                        UnitCount = 13,
                                        UnitPrice = 14
                                    }
                                }

                            }
                        },
                        new ComplianceSchemePaymentCalculationResponseMember
                        {
                            MemberId = "2",
                            MemberLateRegistrationFee = 1000,
                            MemberOnlineMarketPlaceFee = 2000,
                            MemberRegistrationFee = 3000,
                            SubsidiariesFee = 4000,
                            TotalMemberFee = 5000,
                            SubsidiariesFeeBreakdown = new SubsidiariesFeeBreakdown
                            {
                                CountOfOMPSubsidiaries = 66,
                                TotalSubsidiariesOMPFees = 77,
                                UnitOMPFees = 88,
                                FeeBreakdowns =
                                [
                                    new FeeBreakdown
                                    {
                                        BandNumber = 456,
                                        TotalPrice = 345,
                                        UnitCount = 234,
                                        UnitPrice = 123
                                    },
                                    new FeeBreakdown
                                    {
                                        BandNumber = 27,
                                        TotalPrice = 29,
                                        UnitCount = 31,
                                        UnitPrice = 33
                                    }
                                ]

                            }
                        }
                    },
                    OutstandingPayment = 5,
                    PreviousPayment = 2
                };
            #endregion

            logger.LogInformation(message: "Attempting to calculate compliance scheme registration fee for {Reference}", paymentCalculationRequest.ApplicationReferenceNumber);

            var response = await httpClient.PostAsJsonAsync(ComplianceSchemeRegistrationFeesUri, paymentCalculationRequest);

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            var jsonContent = RemoveDecimalValues(await response.Content.ReadAsStringAsync());

            return JsonSerializer.Deserialize<ComplianceSchemePaymentCalculationResponse>(jsonContent, _options);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to calculate compliance scheme registration fee for {Reference}", paymentCalculationRequest.ApplicationReferenceNumber);
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
            if (ReturnFakeData)
                return "https://card.payments.service.gov.uk/secure/9defb517-66f8-45cd-8d9b-20e571b76fb5";

            logger.LogInformation(message: "Attempting to initialise Payment request for {Reference}", paymentInitiationRequest.Reference);

            var response = await httpClient.PostAsJsonAsync(PaymentInitiationUrl, paymentInitiationRequest);

            response.EnsureSuccessStatusCode();

            var htmlContent = await response.Content.ReadAsStringAsync();

            const string pattern = @"window\.location\.href\s*=\s*'(?<url>.*?)';";

            var match = Regex.Match(htmlContent, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(100));

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
        return Regex.Replace(jsonString, @"(\d+)\.0+", "$1", RegexOptions.None, TimeSpan.FromMilliseconds(100));
    }
}