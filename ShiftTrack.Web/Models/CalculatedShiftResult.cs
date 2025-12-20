namespace ShiftTrack.Web.Models;

public class CalculatedShiftResult
{
    public string PersonnelName { get; set; } = string.Empty;

    public DateTime ShiftDate { get; set; }

    public DateTime EntryTime { get; set; }
    public DateTime ExitTime { get; set; }

    public TimeSpan WorkDuration { get; set; }
    public TimeSpan OvertimeDuration { get; set; }
}
