namespace BankProfiles.Web.Data.Entities;

public class ViewHistory
{
    public int HistoryId { get; set; }
    public int BankId { get; set; }
    public long ViewCount { get; set; }  // Changed from int to long to match Bank.ViewCount
    public DateTime RecordedDate { get; set; }

    // Navigation property
    public Bank Bank { get; set; } = null!;
}
