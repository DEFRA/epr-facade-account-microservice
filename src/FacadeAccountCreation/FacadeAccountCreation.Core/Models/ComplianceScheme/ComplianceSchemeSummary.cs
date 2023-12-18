﻿using System.Text.Json.Serialization;
using FacadeAccountCreation.Core.Models.CreateAccount;

namespace FacadeAccountCreation.Core.Models.ComplianceScheme;

public record ComplianceSchemeSummary
{
    public string Name { get; init; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Nation? Nation { get; init; }
    
    public DateTimeOffset CreatedOn { get; init; }
    
    public DateTimeOffset? MembersLastUpdatedOn { get; init; }
    
    public int MemberCount { get; init; }
}