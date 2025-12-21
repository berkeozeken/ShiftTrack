namespace ShiftTrack.Web.ViewModels
{
    public class WorkResultRowViewModel
    {
        public string EmployeeName { get; set; } = string.Empty;
        public string WorkDate { get; set; } = string.Empty;

        public string EntryTime { get; set; } = string.Empty;
        public string ExitTime { get; set; } = string.Empty;

        public string WorkDuration { get; set; } = string.Empty;
        public string OvertimeDuration { get; set; } = string.Empty;
    }
}
