using System.IO;
using UnityEngine;

namespace Save
{
    public static class SaveSystem
    {
        private static string SavePath => Application.persistentDataPath + "/save.json";

        public static SaveData CurrentSave { get; private set; }

        public static void Load()
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                CurrentSave = JsonUtility.FromJson<SaveData>(json);
            }
            else
            {
                CurrentSave = new SaveData();
                Save();
            }
        }

        public static void Save()
        {
            string json = JsonUtility.ToJson(CurrentSave, true);
            File.WriteAllText(SavePath, json);
        }
        public static void ResetSave()
        {
            CurrentSave = new SaveData();
            Save();
        }
        
        public static void UnlockNextLevel(int currentLevel)
        {
            if (currentLevel >= CurrentSave.highestUnlockedLevel)
            {
                CurrentSave.highestUnlockedLevel = currentLevel + 1;
                Save();
            }
        }
        
    }
}