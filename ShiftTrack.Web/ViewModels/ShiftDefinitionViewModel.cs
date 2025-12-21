namespace ShiftTrack.Web.ViewModels
{
    public class ShiftDefinitionViewModel
    {
        public int Id { get; set; }

        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;

        public string BreakStartTime { get; set; } = string.Empty;
        public string BreakEndTime { get; set; } = string.Empty;

        public bool IsOvertimeEligible { get; set; }
    }
}
