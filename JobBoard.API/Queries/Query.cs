using JobBoard.Application.Interfaces;
using JobBoard.Domain.Entities;
using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Http;


namespace JobBoard.API.Queries;

public class Query
{
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    
    // [Service]: Tells Hot Chocolate to inject IJobRepository from Dependency Injection
    public IQueryable<Job> GetJobs([Service] IJobRepository jobrepository)
    {
        return jobrepository.GetAllJobs();
    }
    public async Task<Job?> GetJob(int Id, [Service] IJobRepository jobrepository)
    {
        return await jobrepository.GetJobById(Id);
    }
    
    [UseFiltering]
    [UseSorting]
    public IQueryable<JobApplication> GetApplicationByJobId(int jobId, [Service] IJobRepository jobRepository)
    {
        return jobRepository.GetApplicationByJobId(jobId);
    }
   
    [UseFiltering]
    [UseSorting]
    public IQueryable<Job> GetJobsByCompany(int companyId, [Service] IJobRepository jobRepository)
    {
        return jobRepository.GetJobsByCompany(companyId);
    }
    
    public string GetCurrentUser(
        [Service] IHttpContextAccessor accessor)
    {
        var user = accessor.HttpContext?.User;
        var userId = user?.FindFirst("userId")?.Value;

        return userId is not null
            ? $"Current User ID: {userId}"
            : "Anonymous";
    }
}
