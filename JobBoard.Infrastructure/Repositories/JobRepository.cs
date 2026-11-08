using JobBoard.Application.Interfaces;
using JobBoard.Domain.Entities;
using JobBoard.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace JobBoard.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;
    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JobApplication> AddApplication(JobApplication application)
    {
        _context.Applications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<Job> AddJob(Job job)
    {
        _context.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task<bool> DeleteJob(int jobId)
    {
        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        if (job is null)
        {
            return false;
        }
        _context.Jobs.Remove(job);
        return true;
    }

    public IQueryable<Job> GetAllJobs()
    {
        return _context.Jobs.Include(j => j.Company).OrderBy(j => j.Id);
    }

    public IQueryable<JobApplication> GetApplicationByJobId(int jobId)
    {
        return _context.Applications
            .Where(a => a.JobId == jobId)
            .OrderBy(a => a.Id);
    }

    public async Task<Job?> GetJobById(int Id)
    {
        return await _context.Jobs.Include(j => j.Company).FirstOrDefaultAsync(j => j.Id == Id);
    }

    public IQueryable<Job> GetJobsByCompany(int companyId)
    {
        return _context.Jobs
            .Include(j => j.Company)
            .Where(j => j.CompanyId == companyId)
            .OrderBy(j => j.Id);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByRefreshToken(string refreshToken)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public async Task<JobApplication> UpdateApplicationStatus(int applicationId, string status)
    {
        var application = await _context.Applications.FirstOrDefaultAsync(a => a.Id == applicationId);
        if(application is null)
        {
            throw new Exception("Sorry this applicatin doesn't exit!");
        }
        application.Status = status;
        await _context.SaveChangesAsync();
        return application;

    }

    public async Task UpdateRefreshToken(int userId, string refreshToken, DateTime expiry)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;
        user.RefreshToken = refreshToken;
        user.RefreshTimeExpiry = expiry;
        await _context.SaveChangesAsync();

    }
}
