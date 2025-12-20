using Microsoft.AspNetCore.Mvc;
using ShiftTrack.Web.Models;
using ShiftTrack.Web.Services;

namespace ShiftTrack.Web.Controllers;

public class CalculatorController : Controller
{
	private readonly ShiftTrackApiClient _api;
	private readonly ShiftCalculator _calculator;

	public CalculatorController(ShiftTrackApiClient api)
	{
		_api = api;
		_calculator = new ShiftCalculator();
	}

	public async Task<IActionResult> Index()
	{
		var shifts = await _api.GetAllAsync();

		// Dokümandaki örnek geçiþler (Satýlmýþ Dizman)
		var logs = new List<PersonnelPassLog>
		{
			new PersonnelPassLog { PersonnelName = "Satýlmýþ Dizman", PassTime = new DateTime(2024,10,1,7,50,0), IsEntry = true },
			new PersonnelPassLog { PersonnelName = "Satýlmýþ Dizman", PassTime = new DateTime(2024,10,1,17,25,0), IsEntry = false },

			new PersonnelPassLog { PersonnelName = "Satýlmýþ Dizman", PassTime = new DateTime(2024,10,1,23,59,0), IsEntry = true },
			new PersonnelPassLog { PersonnelName = "Satýlmýþ Dizman", PassTime = new DateTime(2024,10,2,8,1,0), IsEntry = false },

            // Dokümanda tekrar eden çýkýþ satýrý var; algoritma giriþ-çýkýþ eþleþtirmede ilk çýkýþý kullanýr.
            new PersonnelPassLog { PersonnelName = "Satýlmýþ Dizman", PassTime = new DateTime(2024,10,2,8,15,0), IsEntry = false },

			new PersonnelPassLog { PersonnelName = "Satýlmýþ Dizman", PassTime = new DateTime(2024,10,3,15,45,0), IsEntry = true },
			new PersonnelPassLog { PersonnelName = "Satýlmýþ Dizman", PassTime = new DateTime(2024,10,4,16,5,0), IsEntry = true },
			new PersonnelPassLog { PersonnelName = "Satýlmýþ Dizman", PassTime = new DateTime(2024,10,5,1,15,0), IsEntry = false },
		};

		var results = _calculator.Calculate(logs, shifts);

		return View(results);
	}
}
