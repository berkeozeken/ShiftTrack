namespace ShiftTrack.Web.Models;

public class ShiftDefinitionDto
{
    public int Id { get; set; }

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public TimeOnly BreakStartTime { get; set; }
    public TimeOnly BreakEndTime { get; set; }

    public bool IsOvertimeEligible { get; set; }
}
