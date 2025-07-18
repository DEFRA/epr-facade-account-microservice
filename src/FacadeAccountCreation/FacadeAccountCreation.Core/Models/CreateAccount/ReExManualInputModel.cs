﻿namespace FacadeAccountCreation.Core.Models.CreateAccount;

[ExcludeFromCodeCoverage]
public class ReExManualInputModel
{
    public string? OrganisationName { get; set; }

    public ProducerType? ProducerType { get; set; }

    public AddressModel? BusinessAddress { get; set; }

    public Nation? Nation { get; set; }

    public OrganisationType? OrganisationType { get; set; }
}
