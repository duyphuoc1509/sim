using Microsoft.EntityFrameworkCore;
using Sim.Domain.Entities;

namespace Sim.Infrastructure.Persistence;

public sealed class SimDbContext(DbContextOptions<SimDbContext> options) : DbContext(options)
{
    public DbSet<SimCard> Sims => Set<SimCard>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Collaborator> Collaborators => Set<Collaborator>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SimCard>(entity =>
        {
            entity.ToTable("sims");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.PhoneNumber).HasColumnName("phone_number");
            entity.Property(x => x.Carrier).HasColumnName("carrier");
            entity.Property(x => x.Status).HasColumnName("status");
            entity.Property(x => x.ActivationDate).HasColumnName("activation_date");
            entity.Property(x => x.ExpiryDate).HasColumnName("expiry_date");
        });
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.FullName).HasColumnName("full_name");
            entity.Property(x => x.IdentityNumber).HasColumnName("identity_number");
            entity.Property(x => x.PhoneNumbers).HasColumnName("phone_numbers").HasColumnType("text[]");
        });
        modelBuilder.Entity<Collaborator>(entity =>
        {
            entity.ToTable("collaborators");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.FullName).HasColumnName("full_name");
            entity.Property(x => x.PhoneNumber).HasColumnName("phone_number");
            entity.Property(x => x.Email).HasColumnName("email");
        });
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.CustomerId).HasColumnName("customer_id");
            entity.Property(x => x.SimId).HasColumnName("sim_id");
            entity.Property(x => x.CollaboratorId).HasColumnName("collaborator_id");
            entity.Property(x => x.Status).HasColumnName("status");
            entity.Property(x => x.Revenue).HasColumnName("revenue").HasPrecision(18, 2);
            entity.Property(x => x.OrderedAt).HasColumnName("ordered_at");
        });
    }
}
