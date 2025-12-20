namespace ShiftTrack.Web.Models;

public class PersonnelPassLog
{
    public string PersonnelName { get; set; } = string.Empty;
    public DateTime PassTime { get; set; }
    public bool IsEntry { get; set; } 
}
