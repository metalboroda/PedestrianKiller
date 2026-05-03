using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace B93Tools.SpriteCompressor.Editor
{
    public class SpriteCompressorEditor : EditorWindow
    {
        private const string LastFolderKey = "SpriteCompressor_LastFolder";
        private readonly Dictionary<string, SpriteProcessor> _spriteProcessors = new Dictionary<string, SpriteProcessor>();
        private readonly List<string> _spritePaths = new List<string>();

        private float _compressionFactor = 0.8f;
        private Vector2 _scrollPosition;
        private bool _trimAlpha = true;
        private bool _isProcessing;

        private readonly string[] _supportedExtensions = { ".png", ".jpg", ".jpeg" };

        [MenuItem("Tools/Sprite Compressor")]
        public static void ShowWindow()
        {
            SpriteCompressorEditor window = GetWindow<SpriteCompressorEditor>("Sprite Compressor");

            window.minSize = new Vector2(500, 400);
        }

        private void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(_isProcessing);

            DrawHeader();
            DrawDragAndDropArea();
            DrawSpriteList();

            EditorGUI.EndDisabledGroup();

            if (_isProcessing)
            {
                EditorGUILayout.HelpBox("Processing... Please wait.", MessageType.Info);
            }
        }

        private void DrawHeader()
        {
            GUILayout.Label("Sprite Compressor", EditorStyles.boldLabel);
            GUILayout.Space(5);
        }

        private void DrawDragAndDropArea()
        {
            Event currentEvent = Event.current;
            Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter
            };

            GUI.Box(dropArea, "Drag & Drop Images Here", boxStyle);

            EventType eventType = currentEvent.type;
            bool isMouseOverDropArea = dropArea.Contains(currentEvent.mousePosition);

            if (isMouseOverDropArea)
            {
                if (eventType == EventType.DragExited) {}
                else if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.None;

                    if (IsDragDataValid())
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                        if (eventType == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            HandleDroppedItems();

                            currentEvent.Use();
                        }
                    }

                    if (true)
                    {
                        currentEvent.Use();
                    }
                }
            }
        }

        private bool IsDragDataValid()
        {
            foreach (Object obj in DragAndDrop.objectReferences)
            {
                if (obj is Texture2D) return true;
            }

            foreach (string path in DragAndDrop.paths)
            {
                string extension = Path.GetExtension(path).ToLowerInvariant();

                if (_supportedExtensions.Contains(extension))
                {
                    return true;
                }
            }

            return false;
        }

        private void HandleDroppedItems()
        {
            List<string> pathsToAdd = new List<string>();
            bool added = false;

            foreach (Object obj in DragAndDrop.objectReferences)
            {
                if (obj is Texture2D)
                {
                    string path = AssetDatabase.GetAssetPath(obj);

                    if (!string.IsNullOrEmpty(path) && !_spriteProcessors.ContainsKey(path))
                    {
                        pathsToAdd.Add(path);
                    }
                }
            }

            foreach (string path in DragAndDrop.paths)
            {
                string extension = Path.GetExtension(path).ToLowerInvariant();

                if (_supportedExtensions.Contains(extension) && !_spriteProcessors.ContainsKey(path))
                {
                    pathsToAdd.Add(path);
                }
            }

            foreach (string path in pathsToAdd.Distinct())
            {
                AddSpriteProcessor(path);

                added = true;
            }


            if (added)
            {
                Debug.Log($"Added {pathsToAdd.Distinct().Count()} images via drag & drop.");

                Repaint();
            }
        }

        private void DrawSpriteList()
        {
            if (_spritePaths.Count > 0) GUILayout.Space(10);

            if (_spritePaths.Count > 0)
            {
                DrawSettings();
                DrawSpriteScrollView();
                DrawActionButtons();
            }
            else
            {
                GUILayout.Label("Drag & Drop images onto the area above to begin.", EditorStyles.centeredGreyMiniLabel);
            }
        }

        private void DrawSettings()
        {
            GUILayout.Label("Processing Settings", EditorStyles.boldLabel);

            GUIContent compressionLabel = new GUIContent(
                "Compression Factor (Scale)",
                "Determines the output sprite size relative to the original (after trimming, if enabled).\n0.1 = 10% of original size, 1.0 = 100% of original size."
            );
            EditorGUI.BeginChangeCheck();
            _compressionFactor = EditorGUILayout.Slider(compressionLabel, _compressionFactor, 0.1f, 1.0f);
            _trimAlpha = EditorGUILayout.Toggle(
                new GUIContent("Trim Alpha", "Remove transparent borders before resizing."),
                _trimAlpha
            );

            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }

            GUILayout.Space(10);
        }


        private void DrawSpriteScrollView()
        {
            GUILayout.Label("Loaded Sprites", EditorStyles.boldLabel);

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(Mathf.Max(200, position.height - 230)));
            List<string> pathsToRemove = new List<string>();


            for (int i = 0; i < _spritePaths.Count; i++)
            {
                string path = _spritePaths[i];

                if (_spriteProcessors.TryGetValue(path, out SpriteProcessor processor))
                {
                    DrawSpriteItem(processor, path);
                }
                else
                {
                    EditorGUILayout.LabelField($"Error: Processor not found for {Path.GetFileName(path)}");

                    if (GUILayout.Button("Remove Entry", GUILayout.Width(100)))
                    {
                        pathsToRemove.Add(path);
                    }
                }
            }

            GUILayout.EndScrollView();

            foreach (string pathToRemove in pathsToRemove)
            {
                RemoveSprite(pathToRemove);
            }
        }

        private void DrawSpriteItem(SpriteProcessor processor, string path)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            string originalDimensions = processor.OriginalWidth > 0 ? $"({processor.OriginalWidth}x{processor.OriginalHeight})" : "(Loading...)";
            Vector2Int processedSize = processor.CalculateProcessedSize(_compressionFactor, _trimAlpha);
            string processedDimensions = (processedSize.x > 0 && processedSize.y > 0) ? $" -> ({processedSize.x}x{processedSize.y})" : "";
            string labelText = $"{processor.OriginalFileName} {originalDimensions}{processedDimensions}";

            GUILayout.Label(new GUIContent(labelText, path), GUILayout.MinWidth(200), GUILayout.ExpandWidth(true));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save As...", GUILayout.Width(80)))
            {
                SaveSingleSprite(processor);
            }

            if (GUILayout.Button("Overwrite", GUILayout.Width(80)))
            {
                if (EditorUtility.DisplayDialog("Overwrite Sprite",
                    $"Are you sure you want to overwrite the original file?\n{processor.OriginalFileName}\nThis action cannot be undone.",
                    "Overwrite", "Cancel"))
                {
                    OverwriteSingleSprite(processor);
                }
            }

            if (GUILayout.Button("Remove", GUILayout.Width(80)))
            {
                RemoveSprite(path);

                GUIUtility.ExitGUI();
            }

            GUILayout.EndHorizontal();
        }

        private void RemoveSprite(string path)
        {
            if (_spriteProcessors.ContainsKey(path))
            {
                _spriteProcessors[path].Unload();
                _spriteProcessors.Remove(path);
                _spritePaths.Remove(path);

                Repaint();
            }
        }

        private void DrawActionButtons()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save All as ZIP...", GUILayout.Width(150)))
            {
                SaveAllSpritesAsZip();
            }

            if (GUILayout.Button("Clear List", GUILayout.Width(100)))
            {
                ClearList();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        private void AddSpriteProcessor(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            if (_spriteProcessors.ContainsKey(path)) return;

            if (File.Exists(path))
            {
                string extension = Path.GetExtension(path).ToLowerInvariant();
                if (!_supportedExtensions.Contains(extension))
                {
                    Debug.LogWarning($"Skipping unsupported file type: {Path.GetFileName(path)}");
                    return;
                }

                SpriteProcessor processor = new SpriteProcessor(path);
                if (processor.LoadOriginalSprite())
                {
                    _spriteProcessors.Add(path, processor);
                    _spritePaths.Add(path);
                }
            }
            else
            {
                Debug.LogError($"File not found: {path}");
            }
        }

        private void SaveSingleSprite(SpriteProcessor processor)
        {
            string lastFolder = EditorPrefs.GetString(LastFolderKey, GetDefaultPath());
            string suggestedName = Path.GetFileNameWithoutExtension(processor.OriginalFileName) + "_compressed.png";

            string savePath = EditorUtility.SaveFilePanel(
                "Save Compressed Sprite",
                lastFolder,
                suggestedName,
                "png"
            );

            if (!string.IsNullOrEmpty(savePath))
            {
                _isProcessing = true;
                Repaint();
                EditorUtility.DisplayProgressBar("Saving Sprite", $"Processing: {processor.OriginalFileName}", 0.5f);

                try
                {
                    bool success = processor.ProcessAndSaveSprite(savePath, _compressionFactor, _trimAlpha);
                    if (success)
                    {
                        Debug.Log($"Saved: {savePath}");
                        UpdateLastFolder(Path.GetDirectoryName(savePath));
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Unexpected error saving sprite {processor.OriginalFileName}: {ex.Message}\n{ex.StackTrace}");
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                    _isProcessing = false;
                    Repaint();
                }
            }
        }

        private void OverwriteSingleSprite(SpriteProcessor processor)
        {
            _isProcessing = true;
            Repaint();
            EditorUtility.DisplayProgressBar("Overwriting Sprite", $"Processing: {processor.OriginalFileName}", 0.5f);

            try
            {
                bool success = processor.ProcessAndSaveSprite(processor.OriginalFilePath, _compressionFactor, _trimAlpha);
                if (success)
                {
                    Debug.Log($"Overwritten: {processor.OriginalFilePath}");
                    if (processor.IsAssetInProject())
                    {
                        AssetDatabase.ImportAsset(processor.OriginalFilePath, ImportAssetOptions.ForceUpdate);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Unexpected error overwriting sprite {processor.OriginalFileName}: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _isProcessing = false;
                Repaint();
            }
        }

        private void SaveAllSpritesAsZip()
        {
            if (_spritePaths.Count == 0)
            {
                EditorUtility.DisplayDialog("Save All as ZIP", "No sprites loaded to save.", "OK");
                return;
            }

            string lastFolder = EditorPrefs.GetString(LastFolderKey, GetDefaultPath());
            string zipPath = EditorUtility.SaveFilePanel(
                "Save Sprites as ZIP Archive",
                lastFolder,
                "CompressedSprites.zip",
                "zip"
            );

            if (!string.IsNullOrEmpty(zipPath))
            {
                _isProcessing = true;

                Repaint();

                string tempDir = Path.Combine(Application.temporaryCachePath, "SpriteCompressor_" + Path.GetRandomFileName());

                Directory.CreateDirectory(tempDir);

                List<string> savedFilePaths = new List<string>();
                List<string> failedFiles = new List<string>();

                try
                {
                    float progressStep = 1.0f / _spritePaths.Count;
                    for (int i = 0; i < _spritePaths.Count; i++)
                    {
                        string originalPath = _spritePaths[i];

                        if (_spriteProcessors.TryGetValue(originalPath, out SpriteProcessor processor))
                        {
                            string progressInfo = $"Processing: {processor.OriginalFileName} ({i + 1}/{_spritePaths.Count})";

                            if (EditorUtility.DisplayCancelableProgressBar("Saving Sprites to ZIP", progressInfo, (i + 1) * progressStep))
                            {
                                failedFiles.AddRange(_spritePaths.GetRange(i, _spritePaths.Count - i).Select(Path.GetFileName));

                                Debug.LogWarning("Save All operation cancelled by user.");
                                break;
                            }

                            string fileName = Path.GetFileNameWithoutExtension(processor.OriginalFileName) + "_compressed.png";
                            string tempSavePath = Path.Combine(tempDir, fileName);

                            bool success = processor.ProcessAndSaveSprite(tempSavePath, _compressionFactor, _trimAlpha);

                            if (success)
                            {
                                savedFilePaths.Add(tempSavePath);
                            }
                            else
                            {
                                failedFiles.Add(processor.OriginalFileName);
                            }
                        }
                    }

                    if (savedFilePaths.Count > 0 && failedFiles.Count < _spritePaths.Count)
                    {
                        EditorUtility.DisplayProgressBar("Saving Sprites to ZIP", "Creating ZIP archive...", 1.0f);
                        ZipExporter.CreateZip(zipPath, savedFilePaths);

                        Debug.Log($"Successfully saved {savedFilePaths.Count} sprites to ZIP: {zipPath}");

                        UpdateLastFolder(Path.GetDirectoryName(zipPath));

                        if (failedFiles.Count > 0)
                        {
                            EditorUtility.DisplayDialog("Save All Complete (with errors)",
                                $"Successfully saved {savedFilePaths.Count} sprites.\nFailed to process {failedFiles.Count} sprites:\n" + string.Join("\n", failedFiles), "OK");
                        }
                    }
                    else if (savedFilePaths.Count == 0 && failedFiles.Count == 0)
                    {
                        Debug.LogWarning("No sprites were processed or saved.");
                    }
                    else
                    {
                        Debug.LogError($"ZIP archive not created. {failedFiles.Count} sprites failed or the operation was cancelled.");

                        EditorUtility.DisplayDialog("Save All Failed", $"ZIP archive was not created.\n{failedFiles.Count} sprites failed or the operation was cancelled.", "OK");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error during Save All operation: {ex.Message}\n{ex.StackTrace}");

                    EditorUtility.DisplayDialog("Save All Error", $"An unexpected error occurred: {ex.Message}", "OK");
                }
                finally
                {
                    EditorUtility.ClearProgressBar();

                    if (Directory.Exists(tempDir))
                    {
                        try { Directory.Delete(tempDir, true); }
                        catch (IOException ioEx) { Debug.LogError($"Could not delete temporary directory {tempDir}: {ioEx.Message}"); }
                    }

                    _isProcessing = false;

                    Repaint();
                }
            }
        }

        private void ClearList()
        {
            if (_spritePaths.Count > 0 && EditorUtility.DisplayDialog("Clear List", "Are you sure you want to remove all loaded sprites from the list?", "Yes, Clear", "Cancel"))
            {
                foreach (SpriteProcessor processor in _spriteProcessors.Values)
                {
                    processor.Unload();
                }
                
                _spriteProcessors.Clear();
                _spritePaths.Clear();

                Repaint();

                Debug.Log("Sprite list cleared.");
            }
        }

        private void UpdateLastFolder(string folderPath)
        {
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                EditorPrefs.SetString(LastFolderKey, folderPath);
            }
        }

        private string GetDefaultPath()
        {
            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);

            if (!string.IsNullOrEmpty(desktopPath) && Directory.Exists(desktopPath)) return desktopPath;

            string userFolderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

            if (!string.IsNullOrEmpty(userFolderPath) && Directory.Exists(userFolderPath)) return userFolderPath;

            return Application.persistentDataPath;
        }


        private void OnDestroy()
        {
            foreach (SpriteProcessor processor in _spriteProcessors.Values) { processor.Unload(); }

            _spriteProcessors.Clear();
            _spritePaths.Clear();
        }
    }
}