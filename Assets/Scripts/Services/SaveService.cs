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
                if (save == null) return new LocalSave();
                if (save.unlockedLevel < 1) save.unlockedLevel = 1;
                if (save.unlockedLevel > 500) save.unlockedLevel = 500;
                if (save.completedLevels == null) save.completedLevels = new System.Collections.Generic.List<int>();
                if (save.activeCharacter < 0 || save.activeCharacter > (int)CharacterId.Panda)
                    save.activeCharacter = (int)CharacterId.Capybara;
                return save;
            }
            catch
            {
                return new LocalSave();
            }
        }

        public static void Save(LocalSave save)
        {
            if (save == null) return;
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(save));
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }
}