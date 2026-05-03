using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace B93Tools.SpriteCompressor
{
    public static class ZipExporter
    {
        public static void CreateZip(string zipPath, List<string> filesToInclude)
        {
            if (string.IsNullOrEmpty(zipPath))
            {
                Debug.LogError("ZIP Exporter: Provided zip path is null or empty.");
                return;
            }
            if (filesToInclude == null || filesToInclude.Count == 0)
            {
                Debug.LogWarning("ZIP Exporter: No files provided to add to the archive.");
                return;
            }

            try
            {
                string zipDirectory = Path.GetDirectoryName(zipPath);

                if (!string.IsNullOrEmpty(zipDirectory) && !Directory.Exists(zipDirectory))
                {
                    Directory.CreateDirectory(zipDirectory);
                }

                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                using ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);

                foreach (string filePath in filesToInclude)
                {
                    if (File.Exists(filePath))
                    {
                        string entryName = Path.GetFileName(filePath);

                        archive.CreateEntryFromFile(filePath, entryName);
                    }
                    else
                    {
                        Debug.LogWarning($"ZIP Exporter: File not found, skipping: {filePath}");
                    }
                }
            }
            catch (IOException ioEx)
            {
                Debug.LogError($"ZIP Exporter IO Error: Failed to create ZIP archive at {zipPath}. Error: {ioEx.Message}\n{ioEx.StackTrace}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ZIP Exporter Error: An unexpected error occurred. Error: {ex.Message}\n{ex.StackTrace}");

                if (File.Exists(zipPath))
                {
                    try { File.Delete(zipPath); }
                    catch
                    { /* Ignore delete error */
                    }
                }
            }
        }
    }
}