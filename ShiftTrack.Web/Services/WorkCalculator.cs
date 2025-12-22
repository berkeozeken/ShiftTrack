using System.Globalization;
using ShiftTrack.Web.ViewModels;

namespace ShiftTrack.Web.Services
{
    public class WorkCalculator
    {
        // Task kuralý: Erken gelmek fazla mesai deðildir. Fazla mesai, seçilen mesainin bitiþinden sonra baþlar.
        private static readonly TimeSpan ShiftStartTolerance = TimeSpan.FromMinutes(30);

        public List<WorkResultRowViewModel> Calculate(
            List<EmployeePassViewModel> passes,
            List<ShiftDefinitionViewModel> shifts)
        {
            var results = new List<WorkResultRowViewModel>();

            if (passes == null || passes.Count == 0)
                return results;

            if (shifts == null || shifts.Count == 0)
                throw new InvalidOperationException("Mesai tanýmý bulunamadý. Lütfen önce Mesai Tanýmlarý ekranýndan taným ekleyin.");

            // Parse + normalize
            var parsedPasses = new List<ParsedPass>();
            foreach (var p in passes)
            {
                if (string.IsNullOrWhiteSpace(p.EmployeeName) ||
                    string.IsNullOrWhiteSpace(p.Type) ||
                    string.IsNullOrWhiteSpace(p.DateTimeText))
                    continue;

                if (!TryParsePassDateTime(p.DateTimeText, out var dt))
                    throw new InvalidOperationException($"Tarih/Saat formatý hatalý: '{p.DateTimeText}'. Beklenen format: dd.MM.yyyy HH:mm");

                var type = p.Type.Trim();
                if (!IsEntry(type) && !IsExit(type))
                    throw new InvalidOperationException($"Tür alaný 'Giriþ' veya 'Çýkýþ' olmalýdýr. Gelen: '{p.Type}'");

                parsedPasses.Add(new ParsedPass
                {
                    EmployeeName = p.EmployeeName.Trim(),
                    Type = type,
                    Time = dt
                });
            }

            // Personel bazlý hesap
            foreach (var group in parsedPasses
                         .OrderBy(x => x.EmployeeName)
                         .GroupBy(x => x.EmployeeName))
            {
                var ordered = group.OrderBy(x => x.Time).ToList();

                DateTime? openEntry = null;

                foreach (var pass in ordered)
                {
                    if (IsEntry(pass.Type))
                    {
                        // Birden fazla giriþ gelirse: son giriþe güncelle (turnike/yanlýþ basma senaryosu)
                        openEntry = pass.Time;
                        continue;
                    }

                    // Çýkýþ
                    if (openEntry == null)
                        continue; // giriþ yoksa çýkýþý yok say

                    var entryTime = openEntry.Value;
                    var exitTime = pass.Time;

                    // Çýkýþ giriþten önceyse: bu çifti yok say (format veya veri hatasý)
                    if (exitTime <= entryTime)
                    {
                        openEntry = null;
                        continue;
                    }

                    var selectedShift = SelectShift(entryTime, shifts);
                    var shiftStart = BuildShiftDateTime(entryTime.Date, selectedShift.StartTime);
                    var shiftEnd = BuildShiftDateTime(entryTime.Date, selectedShift.EndTime);

                    // Gece vardiyasý normalizasyonu
                    if (shiftEnd <= shiftStart)
                        shiftEnd = shiftEnd.AddDays(1);

                    // Mola aralýðý (mesaiye göre)
                    var breakStart = BuildShiftDateTime(shiftStart.Date, selectedShift.BreakStartTime);
                    var breakEnd = BuildShiftDateTime(shiftStart.Date, selectedShift.BreakEndTime);

                    if (breakEnd <= breakStart)
                        breakEnd = breakEnd.AddDays(1);

                    // Çalýþma süresi = (çýkýþ-giriþ) - (mola ile kesiþen süre)
                    var total = exitTime - entryTime;
                    var breakOverlap = GetOverlap(entryTime, exitTime, breakStart, breakEnd);
                    var netWork = total - breakOverlap;
                    if (netWork < TimeSpan.Zero)
                        netWork = TimeSpan.Zero;

                    // Fazla mesai = max(0, çýkýþ - mesai bitiþi) (erken giriþ asla overtime deðildir)
                    TimeSpan overtime = TimeSpan.Zero;
                    if (selectedShift.IsOvertimeEligible && exitTime > shiftEnd)
                        overtime = exitTime - shiftEnd;

                    results.Add(new WorkResultRowViewModel
                    {
                        EmployeeName = group.Key,
                        WorkDate = entryTime.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                        EntryTime = entryTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
                        ExitTime = exitTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
                        WorkDuration = FormatHm(netWork),
                        OvertimeDuration = FormatHm(overtime)
                    });

                    openEntry = null;
                }
            }

            return results;
        }

        /// <summary>
        /// NET KURAL (uygulanan):
        /// - entryTime için aday mesailer: (Start - 30dk) <= entryTime < End (gece vardiyasý normalize edilir)
        /// - aday varsa: |entry - Start| en küçük olan seçilir; eþitlikte daha erken baþlayan seçilir
        /// - aday yoksa: tüm mesailer içinde |entry - Start| en küçük olan seçilir; eþitlikte daha erken baþlayan
        /// </summary>
        private static ShiftDefinitionViewModel SelectShift(DateTime entryTime, List<ShiftDefinitionViewModel> shifts)
        {
            var candidates = new List<(ShiftDefinitionViewModel shift, DateTime startDt, DateTime endDt, TimeSpan score)>();

            foreach (var s in shifts)
            {
                var startDt = BuildShiftDateTime(entryTime.Date, s.StartTime);
                var endDt = BuildShiftDateTime(entryTime.Date, s.EndTime);

                if (endDt <= startDt)
                    endDt = endDt.AddDays(1);

                var windowStart = startDt - ShiftStartTolerance;

                if (entryTime >= windowStart && entryTime < endDt)
                {
                    var score = (entryTime - startDt).Duration();
                    candidates.Add((s, startDt, endDt, score));
                }
            }

            if (candidates.Count > 0)
            {
                return candidates
                    .OrderBy(x => x.score)
                    .ThenBy(x => x.startDt) // tie-break: daha erken baþlayan
                    .First()
                    .shift;
            }

            // fallback: hiç aday yoksa en yakýn baþlangýca göre seç
            var all = shifts
                .Select(s =>
                {
                    var startDt = BuildShiftDateTime(entryTime.Date, s.StartTime);
                    return (shift: s, startDt, score: (entryTime - startDt).Duration());
                })
                .OrderBy(x => x.score)
                .ThenBy(x => x.startDt)
                .First();

            return all.shift;
        }

        private static TimeSpan GetOverlap(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        {
            var start = aStart > bStart ? aStart : bStart;
            var end = aEnd < bEnd ? aEnd : bEnd;

            if (end <= start)
                return TimeSpan.Zero;

            return end - start;
        }

        private static DateTime BuildShiftDateTime(DateTime baseDate, string timeText)
        {
            var t = ParseTimeSpan(timeText);
            return baseDate.Date.Add(t);
        }

        private static TimeSpan ParseTimeSpan(string hhmm)
        {
            if (string.IsNullOrWhiteSpace(hhmm))
                return TimeSpan.Zero;

            if (TimeSpan.TryParseExact(hhmm.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out var ts))
                return ts;

            // Bazý durumlarda "H:mm" gelebilir
            if (TimeSpan.TryParse(hhmm.Trim(), CultureInfo.InvariantCulture, out ts))
                return ts;

            throw new InvalidOperationException($"Saat formatý hatalý: '{hhmm}'. Beklenen format: HH:mm");
        }

        private static bool TryParsePassDateTime(string text, out DateTime dt)
        {
            return DateTime.TryParseExact(
                text.Trim(),
                "dd.MM.yyyy HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dt);
        }

        private static bool IsEntry(string type) =>
            type.Equals("Giriþ", StringComparison.OrdinalIgnoreCase) ||
            type.Equals("Giris", StringComparison.OrdinalIgnoreCase);

        private static bool IsExit(string type) =>
            type.Equals("Çýkýþ", StringComparison.OrdinalIgnoreCase) ||
            type.Equals("Cikis", StringComparison.OrdinalIgnoreCase);

        private static string FormatHm(TimeSpan ts)
        {
            if (ts < TimeSpan.Zero) ts = TimeSpan.Zero;
            // 24 saati aþarsa da düzgün gözüksün
            var totalMinutes = (int)Math.Round(ts.TotalMinutes, MidpointRounding.AwayFromZero);
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;
            return $"{hours:00}:{minutes:00}";
        }

        private sealed class ParsedPass
        {
            public string EmployeeName { get; set; } = "";
            public string Type { get; set; } = "";
            public DateTime Time { get; set; }
        }
    }
}
