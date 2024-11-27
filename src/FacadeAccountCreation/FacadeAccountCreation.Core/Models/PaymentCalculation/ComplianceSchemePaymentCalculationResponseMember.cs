﻿namespace FacadeAccountCreation.Core.Models.PaymentCalculation;
public class ComplianceSchemePaymentCalculationResponseMember
{
    public string MemberId { get; set; }
    public int MemberRegistrationFee { get; set; }
    public int MemberOnlineMarketPlaceFee { get; set;}
    public int MemberLateRegistrationFee { get; set; }
    public int SubsidiariesFee { get; set; }
    public int TotalMemberFee { get; set; }

    public SubsidiariesFeeBreakdown SubsidiariesFeeBreakdown { get; set; }
}