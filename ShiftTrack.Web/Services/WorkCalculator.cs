using System.Globalization;
using ShiftTrack.Web.ViewModels;

namespace ShiftTrack.Web.Services
{
    public class WorkCalculator
    {
        // Task formatý: "01.10.2024 07:50"
        private static readonly string[] AllowedFormats = new[] { "dd.MM.yyyy HH:mm" };
        private static readonly CultureInfo TrCulture = new CultureInfo("tr-TR");

        // Erken gelme toleransý (dk)
        private const int EntryToleranceMinutes = 30;

        public List<WorkResultRowViewModel> Calculate(
            List<EmployeePassViewModel> passes,
            List<ShiftDefinitionViewModel> shiftDefinitions)
        {
            // 1) Parse + normalize
            var parsed = passes
                .Select(p => new
                {
                    p.EmployeeName,
                    Type = p.Type.Trim(),
                    Time = ParseDateTime(p.DateTimeText)
                })
                .OrderBy(x => x.EmployeeName)
                .ThenBy(x => x.Time)
                .ToList();

            // 2) Employee bazlý iþlem
            var results = new List<WorkResultRowViewModel>();

            var byEmployee = parsed.GroupBy(x => x.EmployeeName);

            foreach (var empGroup in byEmployee)
            {
                var empName = empGroup.Key;
                var events = empGroup.ToList();

                // 3) Giriþ-çýkýþ çiftleme (sýrayla)
                DateTime? openEntry = null;

                foreach (var e in events)
                {
                    var isEntry = e.Type.Equals("Giriþ", StringComparison.OrdinalIgnoreCase);
                    var isExit = e.Type.Equals("Çýkýþ", StringComparison.OrdinalIgnoreCase);

                    if (isEntry)
                    {
                        // Zaten açýk giriþ varsa, en erkeni koru (basit, task dýþýna taþmadan)
                        if (openEntry == null)
                            openEntry = e.Time;
                    }
                    else if (isExit)
                    {
                        if (openEntry == null)
                        {
                            // Giriþ olmadan çýkýþ: görmezden gel
                            continue;
                        }

                        var entry = openEntry.Value;
                        var exit = e.Time;

                        // Çýkýþ giriþten önceyse anlamsýz, görmezden gel
                        if (exit <= entry)
                        {
                            openEntry = null;
                            continue;
                        }

                        // 4) Hangi mesaiye denk geldi? (tolerance + pencere bazlý seçim)
                        var shift = FindBestShift(entry, shiftDefinitions);

                        // 5) Süre hesaplarý
                        var (work, overtime) = CalculateDurations(entry, exit, shift);

                        results.Add(new WorkResultRowViewModel
                        {
                            EmployeeName = empName,
                            WorkDate = entry.ToString("dd.MM.yyyy"),
                            EntryTime = entry.ToString("dd.MM.yyyy HH:mm"),
                            ExitTime = exit.ToString("dd.MM.yyyy HH:mm"),
                            WorkDuration = ToHm(work),
                            OvertimeDuration = ToHm(overtime)
                        });

                        openEntry = null;
                    }
                }
            }

            return results;
        }

        private static DateTime ParseDateTime(string text)
        {
            if (!DateTime.TryParseExact(text.Trim(), AllowedFormats, TrCulture, DateTimeStyles.None, out var dt))
                throw new FormatException($"Tarih formatý geçersiz: {text}. Beklenen format: dd.MM.yyyy HH:mm");

            return dt;
        }

        // Yeni seçim mantýðý:
        // 1) Entry, (ShiftStart - tolerance) ile ShiftEnd arasýnda olan mesailer aday.
        // 2) Aday varsa: ShiftStart'a en yakýn olaný seç.
        // 3) Aday yoksa: fallback olarak yine ShiftStart'a en yakýn olaný seç.
        private static ShiftDefinitionViewModel? FindBestShift(DateTime entry, List<ShiftDefinitionViewModel> shifts)
        {
            if (shifts == null || shifts.Count == 0) return null;

            var candidates = new List<(ShiftDefinitionViewModel Shift, DateTime Start, DateTime End)>();
            var all = new List<(ShiftDefinitionViewModel Shift, DateTime Start, DateTime End)>();

            foreach (var s in shifts)
            {
                if (!TimeSpan.TryParseExact(s.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out var startTs))
                    continue;
                if (!TimeSpan.TryParseExact(s.EndTime, @"hh\:mm", CultureInfo.InvariantCulture, out var endTs))
                    continue;

                var shiftStart = entry.Date + startTs;
                var shiftEnd = entry.Date + endTs;

                // Gece vardiyasý (end < start) => ertesi güne taþýr
                if (endTs < startTs)
                    shiftEnd = shiftEnd.AddDays(1);

                all.Add((s, shiftStart, shiftEnd));

                var windowStart = shiftStart.AddMinutes(-EntryToleranceMinutes);
                var windowEnd = shiftEnd;

                if (entry >= windowStart && entry <= windowEnd)
                {
                    candidates.Add((s, shiftStart, shiftEnd));
                }
            }

            // Aday varsa adaylar arasýndan seç
            if (candidates.Count > 0)
            {
                return candidates
                    .OrderBy(x => Math.Abs((entry - x.Start).TotalMinutes))
                    .First()
                    .Shift;
            }

            // Aday yoksa: fallback (en yakýn baþlangýç)
            if (all.Count > 0)
            {
                return all
                    .OrderBy(x => Math.Abs((entry - x.Start).TotalMinutes))
                    .First()
                    .Shift;
            }

            return null;
        }

        private static (TimeSpan Work, TimeSpan Overtime) CalculateDurations(DateTime entry, DateTime exit, ShiftDefinitionViewModel? shift)
        {
            var total = exit - entry;

            // Shift yoksa mola düþmeden total work yazalým (taným yoksa hesaplayamayýz)
            if (shift == null)
                return (total, TimeSpan.Zero);

            // Shift times
            var start = ParseHm(shift.StartTime);
            var end = ParseHm(shift.EndTime);
            var breakStart = ParseHm(shift.BreakStartTime);
            var breakEnd = ParseHm(shift.BreakEndTime);

            // Mesai baþlangýcý/bitiþi entry date üzerinden oluþturulur
            var shiftStartDt = entry.Date + start;
            var shiftEndDt = entry.Date + end;

            // Gece vardiyasý (end < start) => ertesi güne taþýr
            if (end < start)
                shiftEndDt = shiftEndDt.AddDays(1);

            // Mola zamaný
            var breakStartDt = entry.Date + breakStart;
            var breakEndDt = entry.Date + breakEnd;
            if (breakEnd < breakStart)
                breakEndDt = breakEndDt.AddDays(1);

            // 1) Fazla mesai: sadece mesai bitiþinden sonrasý
            TimeSpan overtime = TimeSpan.Zero;
            if (exit > shiftEndDt && shift.IsOvertimeEligible)
            {
                overtime = exit - shiftEndDt;
            }

            // 2) Çalýþma süresi: (exit-entry) - (mola overlap)
            var work = total - CalculateOverlap(entry, exit, breakStartDt, breakEndDt);
            if (work < TimeSpan.Zero) work = TimeSpan.Zero;

            return (work, overtime);
        }

        private static TimeSpan CalculateOverlap(DateTime rangeStart, DateTime rangeEnd, DateTime blockStart, DateTime blockEnd)
        {
            var start = rangeStart > blockStart ? rangeStart : blockStart;
            var end = rangeEnd < blockEnd ? rangeEnd : blockEnd;

            if (end <= start) return TimeSpan.Zero;
            return end - start;
        }

        private static TimeSpan ParseHm(string hm)
        {
            if (!TimeSpan.TryParseExact(hm, @"hh\:mm", CultureInfo.InvariantCulture, out var ts))
                return TimeSpan.Zero;
            return ts;
        }

        private static string ToHm(TimeSpan ts)
        {
            // 08:35 formatý
            var totalMinutes = (int)Math.Round(ts.TotalMinutes);
            if (totalMinutes < 0) totalMinutes = 0;

            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;

            return $"{hours:00}:{minutes:00}";
        }
    }
}
