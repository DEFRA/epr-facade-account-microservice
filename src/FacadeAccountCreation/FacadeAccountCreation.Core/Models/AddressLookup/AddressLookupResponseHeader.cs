using System.Text.Json.Serialization;

namespace FacadeAccountCreation.Core.Models.AddressLookup;

public class AddressLookupResponseHeader
{
    public string? Query { get; set; }
    public string? Offset { get; set; }
    public string? TotalResults { get; set; }
    public string? Format { get; set; }
    public string? Dataset { get; set; }
    public string? Lr { get; set; }
    public string? MaxResults { get; set; }
    [JsonPropertyName("matching_totalresults")]
    public string? MatchingTotalResults { get; set; }
}
