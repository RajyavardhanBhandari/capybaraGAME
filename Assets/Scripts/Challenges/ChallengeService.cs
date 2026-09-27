using System;
using System.Globalization;
using UnityEngine;

namespace CapybaraGame.Challenges
{
    [Serializable]
    public sealed class DailyResultRecord
    {
        public string dayId;
        public int levelId;
        public int livesRemaining;
        public int treatScore;
        public long completedAtUnix;
    }

    public static class ChallengeService
    {
        private const string DailyPrefix = "CAPY_DAILY_";
        private const string GoldenPrefix = "CAPY_GOLDEN_";

        public static string TodayIdUtc()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public static string CurrentWeekIdUtc()
        {
            var now = DateTime.UtcNow.Date;
            int delta = ((int)now.DayOfWeek + 6) % 7;
            return now.AddDays(-delta).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public static int DailyLevelId(string dayId)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < dayId.Length; i++) hash = hash * 31 + dayId[i];
                hash = Mathf.Abs(hash == int.MinValue ? int.MaxValue : hash);
                return (hash % 500) + 1;
            }
        }

        public static bool HasDailyResult(string dayId) => PlayerPrefs.HasKey(DailyPrefix + dayId);

        public static DailyResultRecord GetDailyResult(string dayId)
        {
            if (!HasDailyResult(dayId)) return null;
            try { return JsonUtility.FromJson<DailyResultRecord>(PlayerPrefs.GetString(DailyPrefix + dayId)); }
            catch { return null; }
        }

        public static bool TryRecordDaily(string dayId, int levelId, int livesRemaining, int treatScore)
        {
            if (HasDailyResult(dayId)) return false;
            var record = new DailyResultRecord
            {
                dayId = dayId,
                levelId = levelId,
                livesRemaining = Mathf.Clamp(livesRemaining, 0, 3),
                treatScore = Mathf.Clamp(treatScore, 0, 3),
                completedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            PlayerPrefs.SetString(DailyPrefix + dayId, JsonUtility.ToJson(record));
            PlayerPrefs.Save();
            return true;
        }

        public static int WeeklyTreatScore()
        {
            var today = DateTime.UtcNow.Date;
            int delta = ((int)today.DayOfWeek + 6) % 7;
            var monday = today.AddDays(-delta);
            int total = 0;
            for (int i = 0; i < 7; i++)
            {
                var day = monday.AddDays(i).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                var result = GetDailyResult(day);
                if (result != null) total += result.treatScore;
            }
            return total;
        }

        public static bool GoldenPlayedToday(string dayId) => PlayerPrefs.GetInt(GoldenPrefix + dayId, 0) != 0;

        public static bool TryClaimGoldenAttempt(string dayId)
        {
            if (GoldenPlayedToday(dayId)) return false;
            PlayerPrefs.SetInt(GoldenPrefix + dayId, 1);
            PlayerPrefs.Save();
            return true;
        }

        public static int GoldenTreats(CharacterId character)
        {
            return PlayerPrefs.GetInt(GoldenPrefix + "TREATS_" + (int)character, 0);
        }

        public static void AddGoldenTreats(CharacterId character, int amount)
        {
            if (amount <= 0) return;
            string key = GoldenPrefix + "TREATS_" + (int)character;
            PlayerPrefs.SetInt(key, GoldenTreats(character) + amount);
            PlayerPrefs.Save();
        }
    }
}