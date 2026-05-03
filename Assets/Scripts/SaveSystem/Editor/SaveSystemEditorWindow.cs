using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.SaveSystem.Editor
{
    public static class SaveSystemEditorWindow
    {
        [MenuItem("Tools/SaveSystem/Reset Game Saves")]
        public static void ResetGameSaves()
        {
            DeleteSaveFile("/settings.json");
            DeleteSaveFile("/levelSettings.json");
            DeleteSaveFile("/coinSettings.json");
            DeleteSaveFile("/shopSettings.json");

            Debug.Log("All game saves have been reset.");
        }

        private static void DeleteSaveFile(string relativePath)
        {
            string path = Application.persistentDataPath + relativePath;

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
    }
}