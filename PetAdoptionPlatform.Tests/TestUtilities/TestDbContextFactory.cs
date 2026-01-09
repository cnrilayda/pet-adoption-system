using System;
using Microsoft.EntityFrameworkCore;
using PetAdoptionPlatform.Infrastructure.Data;

namespace PetAdoptionPlatform.Tests.TestUtilities;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString("N");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }
}
