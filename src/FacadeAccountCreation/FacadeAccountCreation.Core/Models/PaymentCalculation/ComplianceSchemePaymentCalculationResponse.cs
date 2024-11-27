﻿namespace FacadeAccountCreation.Core.Models.PaymentCalculation;

[ExcludeFromCodeCoverage]
public class ComplianceSchemePaymentCalculationResponse
{
    public int TotalFee { get; set; }
    public int ComplianceSchemeRegistrationFee { get; set; }
    public int PreviousPayment { get; set; }
    public int OutstandingPayment { get; set; }
    public List<ComplianceSchemePaymentCalculationResponseMember> ComplianceSchemeMembersWithFees { get; set; }
}