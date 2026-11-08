namespace JobBoard.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTimeExpiry { get; set; }
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
