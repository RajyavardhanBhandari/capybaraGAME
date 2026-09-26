using CapybaraGame.Core;
using UnityEngine;

namespace CapybaraGame.Services
{
    public static class SaveService
    {
        private const string Key = "CAPYBARA_GAME_SAVE";

        public static LocalSave Load()
        {
            if (!PlayerPrefs.HasKey(Key)) return new LocalSave();
            try
            {
                var save = JsonUtility.FromJson<LocalSave>(PlayerPrefs.GetString(Key));
                return save ?? new LocalSave();
            }
            catch
            {
                return new LocalSave();
            }
        }

        public static void Save(LocalSave save)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(save));
            PlayerPrefs.Save();
        }
    }
}
