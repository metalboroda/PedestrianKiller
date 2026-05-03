using System;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.SaveSystem
{
    public static class SaveManager
    {
        private static void Save<T>(T data, string fileName)
        {
            try
            {
                string path = GetFilePath(fileName);
                string jsonData = JsonUtility.ToJson(data, true);

                File.WriteAllText(path, jsonData);
                
                Debug.Log($"{typeof(T).Name} saved to {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save {typeof(T).Name}: {e.Message}");
            }
        }

        private static T Load<T>(string fileName) where T : new()
        {
            string path = GetFilePath(fileName);

            if (File.Exists(path))
            {
                try
                {
                    string jsonData = File.ReadAllText(path);
                    return JsonUtility.FromJson<T>(jsonData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load {typeof(T).Name}: {e.Message}");
                    return new T();
                }
            }

            // Debug.LogWarning($"{typeof(T).Name} file not found, returning default settings.");
            return new T();
        }

        public static void DeleteSaveFile(string fileName)
        {
            string path = GetFilePath(fileName);

            if (File.Exists(path))
            {
                File.Delete(path);
                
                Debug.Log($"Deleted save file: {path}");
            }
            else
            {
                Debug.LogWarning($"Save file not found: {path}");
            }
        }

        private static string GetFilePath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        public static void SaveSettings(SettingsSave data) => Save(data, "settings.json");
        public static SettingsSave LoadSettings() => Load<SettingsSave>("settings.json");

        public static void SaveLevelSettings(LevelSave data) => Save(data, "levelSettings.json");
        public static LevelSave LoadLevelSettings() => Load<LevelSave>("levelSettings.json");

        public static void SaveCoinSettings(CoinSave data) => Save(data, "coinSettings.json");
        public static CoinSave LoadCoinSettings() => Load<CoinSave>("coinSettings.json");

        public static void SaveShopSettings(ShopSave data) => Save(data, "shopSettings.json");
        public static ShopSave LoadShopSettings() => Load<ShopSave>("shopSettings.json");
    }
}