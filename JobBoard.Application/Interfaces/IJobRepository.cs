using JobBoard.Domain.Entities;


namespace JobBoard.Application.Interfaces;

public interface IJobRepository
{
    IQueryable<Job> GetAllJobs();
    Task<Job?> GetJobById(int Id);

    IQueryable<Job> GetJobsByCompany(int companyId);
    IQueryable<JobApplication> GetApplicationByJobId(int jobId);
    Task<Job> AddJob(Job job);
    Task<JobApplication> AddApplication(JobApplication application);
    Task<JobApplication> UpdateApplicationStatus(int applicationId, string status);
    Task<bool> DeleteJob(int jobId);
    Task<User?> GetUserByEmail(string email);
    Task UpdateRefreshToken(int userId, string refreshToken, DateTime expiry);
    Task<User?> GetUserByRefreshToken(string refreshToken);
}
