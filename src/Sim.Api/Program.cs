using Microsoft.EntityFrameworkCore;
using Sim.Application.Contracts;
using Sim.Domain.Entities;
using Sim.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<SimDbContext>(options =>
{
    var postgresConnection = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(postgresConnection))
    {
        options.UseNpgsql(postgresConnection);
        return;
    }

    var sqliteConnection = builder.Environment.IsEnvironment("Testing")
        ? $"Data Source={Path.Combine(Path.GetTempPath(), builder.Configuration["Testing:DatabaseName"] ?? Guid.NewGuid().ToString("N"))}.db"
        : builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=sim.local.db";
    options.UseSqlite(sqliteConnection);
});

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SimDbContext>();
    if (db.Database.IsSqlite())
    {
        db.Database.OpenConnection();
        db.Database.EnsureCreated();
    }
}
app.UseSwagger();
app.UseSwaggerUI();

var api = app.MapGroup("/api/v1");
MapCrud<SimCard, SimRequest>(api, "sims", (request, entity) => { entity.PhoneNumber = request.PhoneNumber; entity.Carrier = request.Carrier; entity.Status = request.Status; entity.ActivationDate = request.ActivationDate; entity.ExpiryDate = request.ExpiryDate; });
MapCrud<Customer, CustomerRequest>(api, "customers", (request, entity) => { entity.FullName = request.FullName; entity.IdentityNumber = request.IdentityNumber; entity.PhoneNumbers = request.PhoneNumbers; });
MapCrud<Collaborator, CollaboratorRequest>(api, "collaborators", (request, entity) => { entity.FullName = request.FullName; entity.PhoneNumber = request.PhoneNumber; entity.Email = request.Email; });
MapCrud<Order, OrderRequest>(api, "orders", (request, entity) => { entity.CustomerId = request.CustomerId; entity.SimId = request.SimId; entity.CollaboratorId = request.CollaboratorId; entity.Status = request.Status; entity.Revenue = request.Revenue; entity.OrderedAt = request.OrderedAt; });

api.MapGet("/alerts/expiring-sims", async (SimDbContext db, int days = 7) =>
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var until = today.AddDays(days);
    return await db.Sims.Where(x => x.ExpiryDate >= today && x.ExpiryDate <= until)
        .Select(x => new ExpiringSimDto(x.Id, x.PhoneNumber, x.Carrier, x.ExpiryDate, x.ExpiryDate.DayNumber - today.DayNumber)).ToListAsync();
});

api.MapGet("/dashboard", async (SimDbContext db, string period = "day") =>
{
    var (from, to) = Period(period);
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var alertUntil = today.AddDays(7);
    var periodOrders = db.Orders.Where(x => x.OrderedAt >= from && x.OrderedAt <= to);
    return new DashboardDto(
        await db.Sims.CountAsync(),
        await periodOrders.CountAsync(),
        await db.Customers.CountAsync(),
        (await periodOrders.Select(x => x.Revenue).ToListAsync()).Sum(),
        await db.Sims.CountAsync(x => x.ExpiryDate >= today && x.ExpiryDate <= alertUntil));
});
api.MapGet("/reports/customers", async (SimDbContext db) => await db.Customers.ToListAsync());
api.MapGet("/reports/orders", async (SimDbContext db, DateOnly? from, DateOnly? to) => await FilterDates(db.Orders, from, to).ToListAsync());
api.MapGet("/reports/collaborators", async (SimDbContext db) => await db.Collaborators.ToListAsync());
api.MapGet("/reports/revenue", async (SimDbContext db, DateOnly? from, DateOnly? to) =>
{
    var orders = FilterDates(db.Orders, from, to);
    return new RevenueReportDto((await orders.Select(x => x.Revenue).ToListAsync()).Sum(), await orders.CountAsync());
});
api.MapGet("/reports/expiring-sims", async (SimDbContext db, int days = 7) =>
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    return await db.Sims.Where(x => x.ExpiryDate >= today && x.ExpiryDate <= today.AddDays(days)).ToListAsync();
});

app.Run();

static IQueryable<Order> FilterDates(IQueryable<Order> orders, DateOnly? from, DateOnly? to) => orders.Where(x => (!from.HasValue || x.OrderedAt >= from) && (!to.HasValue || x.OrderedAt <= to));
static (DateOnly From, DateOnly To) Period(string period)
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    return period.ToLowerInvariant() switch { "week" => (today.AddDays(-6), today), "month" => (today.AddDays(-29), today), _ => (today, today) };
}
static void MapCrud<TEntity, TRequest>(RouteGroupBuilder api, string route, Action<TRequest, TEntity> apply) where TEntity : Entity, new()
{
    var group = api.MapGroup("/" + route);
    group.MapGet("/", async (SimDbContext db) => await db.Set<TEntity>().ToListAsync());
    group.MapGet("/{id:guid}", async (SimDbContext db, Guid id) => await db.Set<TEntity>().FindAsync(id) is { } item ? Results.Ok(item) : Results.NotFound(new ApiError("not_found", $"{route} not found")));
    group.MapPost("/", async (SimDbContext db, TRequest request) => { var item = new TEntity { Id = Guid.NewGuid() }; apply(request, item); db.Add(item); await db.SaveChangesAsync(); return Results.Created($"/api/v1/{route}/{item.Id}", item); });
    group.MapPut("/{id:guid}", async (SimDbContext db, Guid id, TRequest request) => { var item = await db.Set<TEntity>().FindAsync(id); if (item is null) return Results.NotFound(new ApiError("not_found", $"{route} not found")); apply(request, item); await db.SaveChangesAsync(); return Results.Ok(item); });
    group.MapDelete("/{id:guid}", async (SimDbContext db, Guid id) => { var item = await db.Set<TEntity>().FindAsync(id); if (item is null) return Results.NotFound(new ApiError("not_found", $"{route} not found")); db.Remove(item); await db.SaveChangesAsync(); return Results.NoContent(); });
}

public partial class Program { }
