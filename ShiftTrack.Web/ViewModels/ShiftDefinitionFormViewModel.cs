using System.ComponentModel.DataAnnotations;

namespace ShiftTrack.Web.ViewModels
{
    public class ShiftDefinitionFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Mesai baþlangýcý HH:mm formatýnda olmalýdýr (örn: 08:00).")]
        public string StartTime { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Mesai bitiþi HH:mm formatýnda olmalýdýr (örn: 18:00).")]
        public string EndTime { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Mola baþlangýcý HH:mm formatýnda olmalýdýr (örn: 12:00).")]
        public string BreakStartTime { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "Mola bitiþi HH:mm formatýnda olmalýdýr (örn: 13:00).")]
        public string BreakEndTime { get; set; } = string.Empty;

        public bool IsOvertimeEligible { get; set; }
    }
}
