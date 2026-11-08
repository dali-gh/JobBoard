using HotChocolate.Authorization;
using HotChocolate.Subscriptions;
using JobBoard.Application.Interfaces;
using JobBoard.Domain.Entities;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace JobBoard.API.Queries;

public class Mutation
{
    [Authorize]
    public async Task<Job> AddJob(
        string title,
        string description,
        decimal salary,
        string location,
        string jobtype,
        int companyId,
        [Service] IJobRepository jobRepository,
        [Service] ITopicEventSender sender)
    {
        // GraphQl input validation
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage("Title is required.")
                .SetCode("VALIDATION_ERROR")
                .Build());
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new GraphQLException("Description is required.");
        }
        if (salary <= 0)
        {
            throw new GraphQLException("Salary must be greater than zero.");
        }
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new GraphQLException("Location is required.");
        }

        if (string.IsNullOrWhiteSpace(jobtype))
        {
            throw new GraphQLException("Job type is required.");
        }
        if (companyId <= 0)
        {
            throw new GraphQLException("Company ID must be greater than zero.");
        }
        var job = new Job
        {
            Title = title,
            Description = description,
            Salary = salary,
            Location = location,
            JobType = jobtype,
            CreatedAt = DateTime.UtcNow,
            CompanyId = companyId

        };
        await jobRepository.AddJob(job);
        await sender.SendAsync("JobPosted", job);
        return job;
        
    }
    [Authorize]
    public async Task<JobApplication> ApplyForJob(
        int jobId,
        int userId,
        [Service] IJobRepository jobRepository
        )
    {
        var application = new JobApplication
        {
            UserId = userId,
            JobId = jobId,
            Status = "pending",
            AppliedAt = DateTime.UtcNow
        };
        return await jobRepository.AddApplication(application);

    }

    public async Task<JobApplication> UpdateApplicationStatus(int applicationId, string status,
        [Service] IJobRepository jobRepository,
        [Service] ITopicEventSender sender)
    {
        var application = await jobRepository.UpdateApplicationStatus(applicationId, status);
        await sender.SendAsync($"ApplicationStatusChanged_{application.UserId}", application);
        return application;
    }
    [Authorize]
    public async Task<bool> DeleteJob(int jobId, [Service] IJobRepository jobRepository)
    {
        return await jobRepository.DeleteJob(jobId);
    }

    public async Task<LoginResponse> Login(string email, string password, [Service] IJobRepository jobRepository,
        [Service] IConfiguration configuration)
    {
        var user = await jobRepository.GetUserByEmail(email);
        if (user is null)
        {
            throw new GraphQLException("Sorry User not found!");
        }
        bool passwordIsValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

        if (!passwordIsValid)
        {
            throw new GraphQLException("Password is not valid!");
        }
        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("userPassword", user.Password),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials);

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = Guid.NewGuid().ToString();
        await jobRepository.UpdateRefreshToken(user.Id, refreshToken, DateTime.UtcNow.AddDays(7));

        return new LoginResponse
        {
            Token = jwtToken,
            RefreshToken = refreshToken,
        };
    }

    public async Task<string> RefreshToken(string refreshToken, [Service] IJobRepository jobRepository,
        [Service] IConfiguration configuration)
    {
        var user = await jobRepository.GetUserByRefreshToken(refreshToken);
        if (user is null)
        {
            throw new GraphQLException("Invalid Refresh Token!");
        }
        if (user.RefreshTimeExpiry < DateTime.UtcNow)
        {
            throw new GraphQLException("Refresh Token is expired");
        }
        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("userPassword", user.Password),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: credentials);

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
        return jwtToken;
    }
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
