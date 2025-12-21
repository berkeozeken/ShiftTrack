using System.ComponentModel.DataAnnotations;

namespace ShiftTrack.Api.Models
{
    public class ShiftDefinition
    {
        public int Id { get; set; }

        // Mesai baþlangýç / bitiþ (sadece saat-dakika)
        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        // Yemek molasý
        [Required]
        public TimeSpan BreakStartTime { get; set; }

        [Required]
        public TimeSpan BreakEndTime { get; set; }

        // Fazla mesai hakeder/etmez
        public bool IsOvertimeEligible { get; set; }
    }
}
