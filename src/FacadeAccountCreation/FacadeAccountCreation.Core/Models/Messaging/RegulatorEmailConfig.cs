namespace FacadeAccountCreation.Core.Models.Messaging;

[ExcludeFromCodeCoverage]
public class RegulatorEmailConfig
{
    public const string SectionName = "RegulatorEmailConfig";
    public string England { get; set; } = string.Empty;
    public string Wales { get; set; } = string.Empty;
    public string Scotland { get; set; } = string.Empty;
    public string NorthernIreland { get; set; } = string.Empty;
    
}