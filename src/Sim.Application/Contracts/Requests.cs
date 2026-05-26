namespace Sim.Application.Contracts;

public sealed record SimRequest(string PhoneNumber, string Carrier, string Status, DateOnly ActivationDate, DateOnly ExpiryDate);
public sealed record CustomerRequest(string FullName, string IdentityNumber, List<string> PhoneNumbers);
public sealed record CollaboratorRequest(string FullName, string PhoneNumber, string? Email);
public sealed record OrderRequest(Guid CustomerId, Guid SimId, Guid? CollaboratorId, string Status, decimal Revenue, DateOnly OrderedAt);
