using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace CapybaraGame.Analytics
{
    [Serializable] public sealed class AnalyticsEventRecord
    {
        public string name;
        public string timestampUtc;
        public string appVersion;
        public string platform;
        public string properties;
    }

    public static class AnalyticsService
    {
        private const string Key = "CAPY_ANALYTICS_BUFFER";
        private const int MaxBuffered = 100;
        private static readonly List<AnalyticsEventRecord> Buffer = new List<AnalyticsEventRecord>();

        public static void Track(string name, string properties = "")
        {
            if (string.IsNullOrEmpty(name)) return;
            Buffer.Add(new AnalyticsEventRecord
            {
                name = name,
                timestampUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                appVersion = Application.version,
                platform = Application.platform.ToString(),
                properties = properties ?? ""
            });
            while (Buffer.Count > MaxBuffered) Buffer.RemoveAt(0);
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(new BufferWrapper { events = Buffer.ToArray() }));
        }

        public static AnalyticsEventRecord[] GetBuffered()
        {
            try
            {
                if (Buffer.Count == 0 && PlayerPrefs.HasKey(Key))
                {
                    var wrapper = JsonUtility.FromJson<BufferWrapper>(PlayerPrefs.GetString(Key));
                    if (wrapper != null && wrapper.events != null) Buffer.AddRange(wrapper.events);
                }
            }
            catch { }
            return Buffer.ToArray();
        }

        public static void ClearBuffered()
        {
            Buffer.Clear();
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }

        [Serializable] private sealed class BufferWrapper { public AnalyticsEventRecord[] events; }
    }
}