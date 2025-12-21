namespace ShiftTrack.Web.ViewModels
{
    public class CalculatorIndexViewModel
    {
        public List<EmployeePassViewModel> Passes { get; set; } = new List<EmployeePassViewModel>();

        // Hesap sonucu tablo
        public List<WorkResultRowViewModel> Results { get; set; } = new List<WorkResultRowViewModel>();

        // Sayfada gösterim için
        public string? ErrorMessage { get; set; }
    }
}
