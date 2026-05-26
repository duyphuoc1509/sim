using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Sim.Api.Tests;

public sealed class ApiAcceptanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiAcceptanceTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder => builder.UseSetting("environment", "Testing")).CreateClient();
    }

    [Fact]
    public async Task Sim_customer_collaborator_and_order_crud_support_core_management_flow()
    {
        var sim = await Post<SimDto>("/api/v1/sims", new { phoneNumber = "0901000001", carrier = "Viettel", status = "Available", activationDate = "2026-05-20", expiryDate = "2026-06-20" });
        var customer = await Post<CustomerDto>("/api/v1/customers", new { fullName = "Nguyen Van A", identityNumber = "012345678901", phoneNumbers = new[] { "0911000001", "0911000002" } });
        var collaborator = await Post<CollaboratorDto>("/api/v1/collaborators", new { fullName = "Tran CTV", phoneNumber = "0922000001", email = "ctv@example.com" });
        var order = await Post<OrderDto>("/api/v1/orders", new { customerId = customer.Id, simId = sim.Id, collaboratorId = collaborator.Id, status = "Completed", revenue = 1200000m, orderedAt = "2026-05-26" });

        Assert.NotEqual(Guid.Empty, sim.Id);
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/v1/sims/{sim.Id}")).StatusCode);
        Assert.Contains("0911000002", (await _client.GetFromJsonAsync<CustomerDto>($"/api/v1/customers/{customer.Id}"))!.PhoneNumbers);
        Assert.Equal(1200000m, order.Revenue);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/v1/orders/{order.Id}")).StatusCode);
    }

    [Fact]
    public async Task Alerts_dashboard_reports_return_expiring_sims_and_business_metrics()
    {
        var sim = await Post<SimDto>("/api/v1/sims", new { phoneNumber = "0901000002", carrier = "Mobifone", status = "Available", activationDate = "2026-05-01", expiryDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(6).ToString("yyyy-MM-dd") });
        var customer = await Post<CustomerDto>("/api/v1/customers", new { fullName = "Le Thi B", identityNumber = "987654321012", phoneNumbers = new[] { "0933000001" } });
        var collaborator = await Post<CollaboratorDto>("/api/v1/collaborators", new { fullName = "CTV B", phoneNumber = "0944000001" });
        await Post<OrderDto>("/api/v1/orders", new { customerId = customer.Id, simId = sim.Id, collaboratorId = collaborator.Id, status = "Completed", revenue = 500000m, orderedAt = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd") });

        var alerts = await _client.GetFromJsonAsync<List<ExpiringSimDto>>("/api/v1/alerts/expiring-sims?days=7");
        var dashboard = await _client.GetFromJsonAsync<DashboardDto>("/api/v1/dashboard?period=day");
        var revenue = await _client.GetFromJsonAsync<RevenueReportDto>("/api/v1/reports/revenue");
        var customers = await _client.GetFromJsonAsync<List<CustomerDto>>("/api/v1/reports/customers");

        Assert.Contains(alerts!, x => x.PhoneNumber == "0901000002" && x.Carrier == "Mobifone");
        Assert.True(dashboard!.SimCount >= 1);
        Assert.True(dashboard.OrderCount >= 1);
        Assert.True(dashboard.Revenue >= 500000m);
        Assert.True(dashboard.AlertCount >= 1);
        Assert.True(revenue!.TotalRevenue >= 500000m);
        Assert.Contains(customers!, x => x.IdentityNumber == "987654321012");
    }

    private async Task<TResponse> Post<TResponse>(string path, object body)
    {
        var response = await _client.PostAsJsonAsync(path, body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TResponse>())!;
    }

    private sealed record SimDto(Guid Id, string PhoneNumber, string Carrier, string Status, DateOnly ActivationDate, DateOnly ExpiryDate);
    private sealed record CustomerDto(Guid Id, string FullName, string IdentityNumber, List<string> PhoneNumbers);
    private sealed record CollaboratorDto(Guid Id, string FullName, string PhoneNumber, string? Email);
    private sealed record OrderDto(Guid Id, Guid CustomerId, Guid SimId, Guid? CollaboratorId, string Status, decimal Revenue, DateOnly OrderedAt);
    private sealed record ExpiringSimDto(Guid Id, string PhoneNumber, string Carrier, DateOnly ExpiryDate, int DaysUntilExpiry);
    private sealed record DashboardDto(int SimCount, int OrderCount, int CustomerCount, decimal Revenue, int AlertCount);
    private sealed record RevenueReportDto(decimal TotalRevenue, int OrderCount);
}
