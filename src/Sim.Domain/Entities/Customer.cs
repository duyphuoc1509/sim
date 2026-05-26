namespace Sim.Domain.Entities;

public sealed class Customer : Entity
{
    public string FullName { get; set; } = "";
    public string IdentityNumber { get; set; } = "";
    public List<string> PhoneNumbers { get; set; } = [];
}
