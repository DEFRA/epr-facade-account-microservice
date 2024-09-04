using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.Messaging;

[ExcludeFromCodeCoverage]
public class EprPackagingRegulatorEmailConfig
{
    public const string SectionName = "EprPackagingRegulatorEmailConfig";
    public string England { get; set; } = string.Empty;
    public string Wales { get; set; } = string.Empty;
    public string Scotland { get; set; } = string.Empty;
    public string NorthernIreland { get; set; } = string.Empty;
    
}