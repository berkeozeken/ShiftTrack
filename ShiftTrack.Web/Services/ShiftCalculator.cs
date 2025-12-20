using ShiftTrack.Web.Models;

namespace ShiftTrack.Web.Services;

public class ShiftCalculator
{
    public List<CalculatedShiftResult> Calculate(
        List<PersonnelPassLog> logs,
        List<ShiftDefinitionDto> shiftDefinitions)
    {
        var results = new List<CalculatedShiftResult>();

        // Personel bazýnda sýrala
        var grouped = logs
            .OrderBy(x => x.PassTime)
            .GroupBy(x => x.PersonnelName);

        foreach (var personGroup in grouped)
        {
            var personLogs = personGroup.OrderBy(x => x.PassTime).ToList();

            // Basit eþleþtirme: giriþten sonra ilk çýkýþ
            for (int i = 0; i < personLogs.Count; i++)
            {
                if (!personLogs[i].IsEntry) continue;

                var entry = personLogs[i];

                var exit = personLogs
                    .Skip(i + 1)
                    .FirstOrDefault(x => !x.IsEntry);

                if (exit is null) break;

                var (matchedShift, shiftStart, shiftEnd, breakStart, breakEnd) =
                    MatchShift(entry.PassTime, exit.PassTime, shiftDefinitions);

                // Mola süresi: çalýþma aralýðý ile mola aralýðýnýn kesiþimi
                var breakOverlapMinutes = OverlapMinutes(entry.PassTime, exit.PassTime, breakStart, breakEnd);

                var totalWork = exit.PassTime - entry.PassTime;
                var workMinusBreak = totalWork - TimeSpan.FromMinutes(breakOverlapMinutes);

                // Fazla mesai: mesai bitiþinden sonra kalan dakika (erken gelmek fazla mesai deðildir)
                var overtimeMinutes = 0;
                if (matchedShift is not null && matchedShift.IsOvertimeEligible)
                {
                    if (exit.PassTime > shiftEnd)
                    {
                        overtimeMinutes = (int)Math.Floor((exit.PassTime - shiftEnd).TotalMinutes);
                        if (overtimeMinutes < 0) overtimeMinutes = 0;
                    }
                }

                results.Add(new CalculatedShiftResult
                {
                    PersonnelName = personGroup.Key,
                    ShiftDate = shiftStart.Date,
                    EntryTime = entry.PassTime,
                    ExitTime = exit.PassTime,
                    WorkDuration = workMinusBreak,
                    OvertimeDuration = TimeSpan.FromMinutes(overtimeMinutes)
                });

                // çýkýþý kullandýk, i’yi exit indexine çek
                i = personLogs.IndexOf(exit);
            }
        }

        return results;
    }

    private (ShiftDefinitionDto? shift, DateTime shiftStart, DateTime shiftEnd, DateTime breakStart, DateTime breakEnd)
        MatchShift(DateTime entry, DateTime exit, List<ShiftDefinitionDto> shifts)
    {
        // Varsayýlan: giriþ gününe göre baz al
        ShiftDefinitionDto? best = null;
        double bestOverlap = -1;

        DateTime bestStart = entry.Date;
        DateTime bestEnd = entry.Date;
        DateTime bestBreakStart = entry.Date;
        DateTime bestBreakEnd = entry.Date;

        foreach (var s in shifts)
        {
            var shiftStart = entry.Date + s.StartTime.ToTimeSpan();
            var shiftEnd = entry.Date + s.EndTime.ToTimeSpan();
            if (s.EndTime < s.StartTime)
            {
                shiftEnd = shiftEnd.AddDays(1); // gece vardiyasý
            }

            var breakStart = entry.Date + s.BreakStartTime.ToTimeSpan();
            var breakEnd = entry.Date + s.BreakEndTime.ToTimeSpan();
            if (s.BreakEndTime < s.BreakStartTime)
            {
                breakEnd = breakEnd.AddDays(1);
            }
            // Eðer shift geceye taþýyorsa ve mola saatleri de geceye denk geliyorsa, mola gününü kaydýr
            if (s.EndTime < s.StartTime)
            {
                // mola baþlangýcý vardiya baþlangýcýndan "önce" kalýyorsa ertesi güne al
                if (breakStart < shiftStart) breakStart = breakStart.AddDays(1);
                if (breakEnd < shiftStart) breakEnd = breakEnd.AddDays(1);
            }

            var overlap = OverlapMinutes(entry, exit, shiftStart, shiftEnd);
            if (overlap > bestOverlap)
            {
                bestOverlap = overlap;
                best = s;
                bestStart = shiftStart;
                bestEnd = shiftEnd;
                bestBreakStart = breakStart;
                bestBreakEnd = breakEnd;
            }
        }

        return (best, bestStart, bestEnd, bestBreakStart, bestBreakEnd);
    }

    private int OverlapMinutes(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
    {
        var start = aStart > bStart ? aStart : bStart;
        var end = aEnd < bEnd ? aEnd : bEnd;

        if (end <= start) return 0;

        return (int)Math.Floor((end - start).TotalMinutes);
    }
}
