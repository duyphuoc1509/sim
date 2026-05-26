namespace Sim.Application.Contracts;

public sealed record ExpiringSimDto(Guid Id, string PhoneNumber, string Carrier, DateOnly ExpiryDate, int DaysUntilExpiry);
public sealed record DashboardDto(int SimCount, int OrderCount, int CustomerCount, decimal Revenue, int AlertCount);
public sealed record RevenueReportDto(decimal TotalRevenue, int OrderCount);
public sealed record ApiError(string Code, string Message);
