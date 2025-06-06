namespace FacadeAccountCreation.Core.Models.CreateAccount;

public class ReExManualInputModel
{
    public string TradingName { get; set; }

    public ProducerType? ProducerType { get; set; }

    public AddressModel? BusinessAddress { get; set; }
}
