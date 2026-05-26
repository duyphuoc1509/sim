namespace Sim.Api.Tests;

public sealed class ArchitectureTests
{
    [Fact]
    public void Solution_contains_clean_architecture_projects()
    {
        var root = FindRepositoryRoot();

        Assert.True(File.Exists(Path.Combine(root, "src", "Sim.Domain", "Sim.Domain.csproj")));
        Assert.True(File.Exists(Path.Combine(root, "src", "Sim.Application", "Sim.Application.csproj")));
        Assert.True(File.Exists(Path.Combine(root, "src", "Sim.Infrastructure", "Sim.Infrastructure.csproj")));
    }

    [Fact]
    public void Api_program_is_not_declaring_domain_entities_or_db_context()
    {
        var root = FindRepositoryRoot();
        var program = File.ReadAllText(Path.Combine(root, "src", "Sim.Api", "Program.cs"));

        Assert.DoesNotContain("public sealed class SimCard", program);
        Assert.DoesNotContain("public sealed class SimDbContext", program);
        Assert.DoesNotContain("public sealed record SimRequest", program);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Sim.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
