
using JobBoard.Domain.Entities;
using System.Net.WebSockets;

namespace JobBoard.Infrastructure.Data;

public class AppDbContextSeeder
{
    public static async Task SeedAsync(AppDbContext context) 
    { 
        if (context.Companies.Any())
        {
            return; // Database has already been seeded
        }

        // Seed companies
        var companies = new List<Company>
        {
                new Company { Name = "TechCorp", Email = "info@techcorp.com", Location = "San Francisco, CA" },
                new Company { Name = "HealthPlus", Email = "info@healthplus.com", Location = "New York, NY" },
                new Company { Name = "EduWorld", Email = "info@eduworld.com", Location = "Los Angeles, CA" }
        };
        await context.Companies.AddRangeAsync(companies);
        await context.SaveChangesAsync();
       
        // Seed Users
        var users = new List<User>
        {
                new User { Name = "Alice Johnson", Email = "alice@techcorp.com", Password = BCrypt.Net.BCrypt.HashPassword("password123") },
                new User { Name = "Bob Smith", Email = "bob@healthplus.com" , Password = BCrypt.Net.BCrypt.HashPassword("password123")},
                new User { Name = "Charlie Brown", Email = "charlie@eduworld.com" , Password = BCrypt.Net.BCrypt.HashPassword("password123")}
        };
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
        // Seed Jobs
        var jobs = new List<Job>
        {
                new Job { Title = "Software Engineer", Description = "Develop and maintain software applications.", CompanyId = companies[0].Id },
                new Job { Title = "Data Analyst", Description = "Analyze data and generate insights.", CompanyId = companies[1].Id },
                new Job { Title = "Project Manager", Description = "Manage projects and coordinate teams.", CompanyId = companies[2].Id }
        };
        await context.Jobs.AddRangeAsync(jobs);
        await context.SaveChangesAsync();
    }
}