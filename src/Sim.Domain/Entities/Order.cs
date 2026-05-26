namespace Sim.Domain.Entities;

public sealed class Order : Entity
{
    public Guid CustomerId { get; set; }
    public Guid SimId { get; set; }
    public Guid? CollaboratorId { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal Revenue { get; set; }
    public DateOnly OrderedAt { get; set; }
}
