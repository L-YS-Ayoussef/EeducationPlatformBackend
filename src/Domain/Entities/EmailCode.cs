namespace Domain.Entities;

public class EmailCode
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string Purpose { get; set; } = "password_reset"; // future friendly
    public string CodeHash { get; set; } = default!;        // SHA256 of the code
    public DateTime ExpiresAt { get; set; }                 // UTC
    public DateTime CreatedAt { get; set; }                 // UTC
    public DateTime? ConsumedAt { get; set; }
    public int Attempts { get; set; }                       // simple rate limiting
}
