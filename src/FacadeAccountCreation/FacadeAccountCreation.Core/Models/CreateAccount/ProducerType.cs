using System.Diagnostics.CodeAnalysis;

namespace FacadeAccountCreation.Core.Models.CreateAccount;

public enum ProducerType
{
    NotSet = 0,
    Partnership = 1,
    UnincorporatedBody = 2,
    NonUkOrganisation = 3,
    SoleTrader = 4,
    Other = 5
}
