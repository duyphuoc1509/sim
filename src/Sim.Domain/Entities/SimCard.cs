namespace Sim.Domain.Entities;

public sealed class SimCard : Entity
{
    public string PhoneNumber { get; set; } = "";
    public string Carrier { get; set; } = "";
    public string Status { get; set; } = "Available";
    public DateOnly ActivationDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
}
