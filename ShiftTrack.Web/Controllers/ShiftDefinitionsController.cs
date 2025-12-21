using Microsoft.AspNetCore.Mvc;
using ShiftTrack.Web.Services;
using ShiftTrack.Web.ViewModels;

namespace ShiftTrack.Web.Controllers
{
    public class ShiftDefinitionsController : Controller
    {
        private readonly ShiftTrackApiClient _api;

        public ShiftDefinitionsController(ShiftTrackApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new ShiftDefinitionsIndexViewModel
            {
                Items = await _api.GetShiftDefinitionsAsync(),
                Form = new ShiftDefinitionFormViewModel()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ShiftDefinitionsIndexViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Items = await _api.GetShiftDefinitionsAsync();
                return View("Index", vm);
            }

            try
            {
                if (vm.Form.Id.HasValue && vm.Form.Id.Value > 0)
                {
                    await _api.UpdateShiftDefinitionAsync(vm.Form.Id.Value, vm.Form);
                    TempData["Success"] = "Mesai güncellendi.";
                }
                else
                {
                    await _api.CreateShiftDefinitionAsync(vm.Form);
                    TempData["Success"] = "Mesai eklendi.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                
                ModelState.AddModelError(string.Empty, "Ýþlem sýrasýnda hata oluþtu. API baðlantýsýný ve Docker'ýn açýk olduðunu kontrol edin.");
                vm.Items = await _api.GetShiftDefinitionsAsync();
                return View("Index", vm);
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _api.DeleteShiftDefinitionAsync(id);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Silme iþlemi baþarýsýz oldu. API/Docker kontrol edin." });
            }
        }
    }
}
