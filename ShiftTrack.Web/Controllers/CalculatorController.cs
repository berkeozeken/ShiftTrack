using Microsoft.AspNetCore.Mvc;
using ShiftTrack.Web.Services;
using ShiftTrack.Web.ViewModels;

namespace ShiftTrack.Web.Controllers
{
    public class CalculatorController : Controller
    {
        private readonly ShiftTrackApiClient _api;
        private readonly WorkCalculator _calculator;

        public CalculatorController(ShiftTrackApiClient api)
        {
            _api = api;
            _calculator = new WorkCalculator();
        }

        [HttpGet]
        public IActionResult Index()
        {
            var vm = new CalculatorIndexViewModel
            {
                Passes = new List<EmployeePassViewModel>(),
                Results = new List<WorkResultRowViewModel>(),
                ErrorMessage = null
            };

            return View(vm);
        }

        // Satır ekleme: mevcut Passes gelir, üstüne 1 boş satır ekler
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddRow(CalculatorIndexViewModel vm)
        {
            vm.Passes ??= new List<EmployeePassViewModel>();
            vm.Results ??= new List<WorkResultRowViewModel>();

            vm.Passes.Add(new EmployeePassViewModel
            {
                EmployeeName = "",
                Type = "Giriş",
                DateTimeText = ""
            });

            return View("Index", vm);
        }

        // Satır silme: index'e göre kaldırır
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveRow(CalculatorIndexViewModel vm, int index)
        {
            vm.Passes ??= new List<EmployeePassViewModel>();
            vm.Results ??= new List<WorkResultRowViewModel>();

            if (index >= 0 && index < vm.Passes.Count)
                vm.Passes.RemoveAt(index);

            return View("Index", vm);
        }

        // Hesaplama: boş satırları temizler, API'den shiftleri çeker, hesaplar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(CalculatorIndexViewModel vm)
        {
            try
            {
                vm.Passes ??= new List<EmployeePassViewModel>();

                // Boş satırları dahil etme (task'a uygun, UI rahat)
                vm.Passes = vm.Passes
                    .Where(p =>
                        !string.IsNullOrWhiteSpace(p.EmployeeName) &&
                        !string.IsNullOrWhiteSpace(p.Type) &&
                        !string.IsNullOrWhiteSpace(p.DateTimeText))
                    .ToList();

                var shifts = await _api.GetShiftDefinitionsAsync();
                vm.Results = _calculator.Calculate(vm.Passes, shifts);

                vm.ErrorMessage = null;
                return View("Index", vm);
            }
            catch (Exception ex)
            {
                vm.Results = new List<WorkResultRowViewModel>();
                vm.ErrorMessage = ex.Message;
                return View("Index", vm);
            }
        }
    }
}
