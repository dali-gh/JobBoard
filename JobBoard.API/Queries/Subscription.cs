using JobBoard.Domain.Entities;

namespace JobBoard.API.Queries;

public class Subscription
{
    [Subscribe]
    [Topic("JobPosted")]
    public Job OnJobPosted([EventMessage] Job job)
    {
        return job;
    }

    [Subscribe]
    [Topic("ApplicationStatusChanged_{userId}")]
    public JobApplication OnApplicationStatusChanged(int userId, [EventMessage] JobApplication jobApplication)
    {
        return jobApplication;
    }
}
