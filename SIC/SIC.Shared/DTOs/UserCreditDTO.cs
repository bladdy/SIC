public class UserCreditDTO
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string FullName { get; set; } = string.Empty;
    public int TotalCredits { get; set; }
    public int AvailableCredits { get; set; }
    public int ConsumedCredits { get; set; }
    public int PendingCredits { get; set; }
    public DateTime LastUpdated { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Notes { get; set; }
    public string Summary => $"{AvailableCredits}/{TotalCredits}";
}