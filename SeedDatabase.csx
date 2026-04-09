using Microsoft.EntityFrameworkCore;
using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;

var factory = new BankDbContextFactory();
using var context = factory.CreateDbContext(args);

// Check if banks exist
if (!context.Banks.Any())
{
    Console.WriteLine("Seeding banks and ratings...");

    // Add banks
    var banks = new[]
    {
        new Bank { BankCode = "bank-alpha", ViewCount = 150, CreatedDate = DateTime.UtcNow },
        new Bank { BankCode = "bank-beta", ViewCount = 95, CreatedDate = DateTime.UtcNow },
        new Bank { BankCode = "bank-gamma", ViewCount = 220, CreatedDate = DateTime.UtcNow },
        new Bank { BankCode = "bank-delta", ViewCount = 45, CreatedDate = DateTime.UtcNow },
        new Bank { BankCode = "bank-epsilon", ViewCount = 180, CreatedDate = DateTime.UtcNow }
    };

    context.Banks.AddRange(banks);
    await context.SaveChangesAsync();

    // Add ratings for each bank
    var criteriaIds = context.RatingCriterias.Select(c => c.CriteriaId).ToList();
    var bankIds = context.Banks.ToDictionary(b => b.BankCode, b => b.BankId);

    var ratings = new List<BankRating>
    {
        // Alpha Bank - High quality
        new() { BankId = bankIds["bank-alpha"], CriteriaId = 1, RatingValue = 9.2m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-alpha"], CriteriaId = 2, RatingValue = 8.5m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-alpha"], CriteriaId = 3, RatingValue = 9.0m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-alpha"], CriteriaId = 4, RatingValue = 8.8m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-alpha"], CriteriaId = 5, RatingValue = 9.1m, RatingDate = DateTime.UtcNow },
        
        // Beta Bank - Moderate
        new() { BankId = bankIds["bank-beta"], CriteriaId = 1, RatingValue = 7.5m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-beta"], CriteriaId = 2, RatingValue = 7.0m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-beta"], CriteriaId = 3, RatingValue = 6.8m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-beta"], CriteriaId = 4, RatingValue = 6.5m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-beta"], CriteriaId = 5, RatingValue = 7.8m, RatingDate = DateTime.UtcNow },
        
        // Gamma Bank - Excellent digital
        new() { BankId = bankIds["bank-gamma"], CriteriaId = 1, RatingValue = 8.9m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-gamma"], CriteriaId = 2, RatingValue = 9.5m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-gamma"], CriteriaId = 3, RatingValue = 9.3m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-gamma"], CriteriaId = 4, RatingValue = 9.8m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-gamma"], CriteriaId = 5, RatingValue = 9.0m, RatingDate = DateTime.UtcNow },
        
        // Delta Bank - Lower ratings
        new() { BankId = bankIds["bank-delta"], CriteriaId = 1, RatingValue = 6.2m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-delta"], CriteriaId = 2, RatingValue = 5.8m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-delta"], CriteriaId = 3, RatingValue = 6.5m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-delta"], CriteriaId = 4, RatingValue = 5.5m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-delta"], CriteriaId = 5, RatingValue = 6.8m, RatingDate = DateTime.UtcNow },
        
        // Epsilon Bank - Premium
        new() { BankId = bankIds["bank-epsilon"], CriteriaId = 1, RatingValue = 9.5m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-epsilon"], CriteriaId = 2, RatingValue = 7.8m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-epsilon"], CriteriaId = 3, RatingValue = 9.2m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-epsilon"], CriteriaId = 4, RatingValue = 8.9m, RatingDate = DateTime.UtcNow },
        new() { BankId = bankIds["bank-epsilon"], CriteriaId = 5, RatingValue = 9.4m, RatingDate = DateTime.UtcNow }
    };

    context.BankRatings.AddRange(ratings);
    await context.SaveChangesAsync();

    Console.WriteLine("Seeding completed!");
}
else
{
    Console.WriteLine("Database already seeded.");
}
