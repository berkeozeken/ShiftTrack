using System.ComponentModel.DataAnnotations;

namespace ShiftTrack.Api.Dtos
{
    public class ShiftDefinitionCreateUpdateDto
    {
        // "HH:mm" þeklinde parse edeceðiz (MVC'den böyle gelecek)
        [Required]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "StartTime format must be HH:mm")]
        public string StartTime { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "EndTime format must be HH:mm")]
        public string EndTime { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "BreakStartTime format must be HH:mm")]
        public string BreakStartTime { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{2}:\d{2}$", ErrorMessage = "BreakEndTime format must be HH:mm")]
        public string BreakEndTime { get; set; } = string.Empty;

        public bool IsOvertimeEligible { get; set; }
    }
}
