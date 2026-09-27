using System;
using System.Globalization;

namespace CapybaraGame.Challenges
{
    [Serializable] public sealed class WeeklyScore
    {
        public string weekId;
        public int treatScore;
        public int perfectDays;
        public long finalQualifyingTimestamp;
    }

    public static class WeeklyLeaderboardService
    {
        public static WeeklyScore GetLocalScore()
        {
            var week = ChallengeService.CurrentWeekIdUtc();
            int total = ChallengeService.WeeklyTreatScore();
            int perfect = 0;
            long finalTime = 0;
            var monday = DateTime.ParseExact(week, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;
            for (int i = 0; i < 7; i++)
            {
                string day = monday.AddDays(i).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var result = ChallengeService.GetDailyResult(day);
                if (result == null) continue;
                if (result.treatScore == 3) perfect++;
                if (result.completedAtUnix > finalTime) finalTime = result.completedAtUnix;
            }
            return new WeeklyScore
            {
                weekId = week,
                treatScore = total,
                perfectDays = perfect,
                finalQualifyingTimestamp = finalTime
            };
        }
    }
}