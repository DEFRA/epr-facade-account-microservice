namespace FacadeAccountCreation.Core.Models.AddressLookup;

public class AddressLookupResponseAddress
{
    public string? AddressLine { get; set; }
    public string? BuildingNumber { get; set; }
    public string? Street { get; set; }
    public string? Locality { get; set; }
    public string? Town { get; set; }
    public string? County { get; set; }
    public string? Postcode { get; set; }
    public string? Country { get; set; }
    public int? XCoordinate { get; set; }
    public int? YCoordinate { get; set; }
    public string? Uprn { get; set; }
    public string? Match { get; set; }
    public string? MatchDescription { get; set; }
    public string? Language { get; set; }
    public string? SubBuildingName { get; set; }
}
