using Core.Domain;

namespace Core.Tests;

public class ArchitectureTests
{
    [Fact]
    public void CoreAssemblyMustNotReferenceEfOrOuterLayers()
    {
        var referencedAssemblies = typeof(Product).Assembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToList();

        Assert.DoesNotContain(referencedAssemblies, name =>
            name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("Microsoft.AspNetCore", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Infrastructure", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Api", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("Web", StringComparison.OrdinalIgnoreCase));
    }
}
