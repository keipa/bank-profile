namespace BankProfiles.Web.Data.Entities;

public class BankSnapshot
{
    public long SnapshotId { get; set; }
    public required string BankCode { get; set; }
    public required string ProfileJson { get; set; }
    public long EventSequenceUpTo { get; set; }
    public DateTime CreatedDate { get; set; }
}
