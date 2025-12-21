using System.ComponentModel.DataAnnotations;

namespace ShiftTrack.Web.ViewModels
{
    public class EmployeePassViewModel
    {
        [Required]
        public string EmployeeName { get; set; } = string.Empty;

        // "Giriþ" / "Çýkýþ"
        [Required]
        public string Type { get; set; } = string.Empty;

        // "01.10.2024 07:50"
        [Required]
        public string DateTimeText { get; set; } = string.Empty;
    }
}
