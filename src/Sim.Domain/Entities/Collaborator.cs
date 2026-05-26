namespace Sim.Domain.Entities;

public sealed class Collaborator : Entity
{
    public string FullName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string? Email { get; set; }
}
