using System.IO;
using UnityEngine;

namespace B93Tools.SpriteCompressor
{
    public class SpriteProcessor
    {
        private Texture2D _originalSprite;
        private bool _trimBoundsCalculated;
        private int _trimmedMinWidth = -1, _trimmedMaxWidth = -1, _trimmedMinHeight = -1, _trimmedMaxHeight = -1;

        public string OriginalFilePath { get; }
        public string OriginalFileName { get; }
        public int OriginalWidth => _originalSprite ? _originalSprite.width : 0;
        public int OriginalHeight => _originalSprite ? _originalSprite.height : 0;


        public SpriteProcessor(string path)
        {
            OriginalFilePath = path;
            OriginalFileName = Path.GetFileName(OriginalFilePath);
        }

        public bool IsAssetInProject()
        {
            return OriginalFilePath.StartsWith(Application.dataPath);
        }

        public bool LoadOriginalSprite()
        {
            if (_originalSprite) return true;
            if (!File.Exists(OriginalFilePath))
            {
                Debug.LogError($"Sprite file not found: {OriginalFilePath}");
                return false;
            }

            try
            {
                byte[] fileData = File.ReadAllBytes(OriginalFilePath);

                _originalSprite = new Texture2D(2, 2, TextureFormat.RGBA32, false);

                if (_originalSprite.LoadImage(fileData))
                {
                    _trimBoundsCalculated = false;
                    return true;
                }

                Debug.LogError($"Failed to load image data for: {OriginalFileName}");
                Object.DestroyImmediate(_originalSprite);

                _originalSprite = null;
                return false;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error loading sprite {OriginalFileName}: {ex.Message}\n{ex.StackTrace}");

                if (_originalSprite) Object.DestroyImmediate(_originalSprite);

                _originalSprite = null;
                return false;
            }
        }

        public void Unload()
        {
            if (_originalSprite)
            {
                Object.DestroyImmediate(_originalSprite);

                _originalSprite = null;
                _trimBoundsCalculated = false;
            }
        }

        public Vector2Int CalculateProcessedSize(float compressionFactor, bool trimAlpha)
        {
            if (!_originalSprite)
            {
                if (!LoadOriginalSprite())
                {
                    return Vector2Int.zero;
                }
            }

            int baseWidth = OriginalWidth;
            int baseHeight = OriginalHeight;

            if (trimAlpha)
            {
                CalculateAndCacheTrimBounds();

                if (_trimBoundsCalculated && _trimmedMaxWidth >= _trimmedMinWidth && _trimmedMaxHeight >= _trimmedMinHeight)
                {
                    int potentialTrimmedWidth = _trimmedMaxWidth - _trimmedMinWidth + 1;
                    int potentialTrimmedHeight = _trimmedMaxHeight - _trimmedMinHeight + 1;

                    if (potentialTrimmedWidth != baseWidth || potentialTrimmedHeight != baseHeight)
                    {
                        baseWidth = potentialTrimmedWidth;
                        baseHeight = potentialTrimmedHeight;
                    }
                }
            }

            if (baseWidth <= 0 || baseHeight <= 0) return Vector2Int.zero;

            return CalculateAdjustedSize(baseWidth, baseHeight, compressionFactor);
        }


        public bool ProcessAndSaveSprite(string savePath, float compressionFactor, bool trimAlpha)
        {
            if (!_originalSprite)
            {
                if (!LoadOriginalSprite())
                {
                    Debug.LogError($"Original sprite not loaded for {OriginalFileName}. Cannot process.");
                    return false;
                }
            }

            Texture2D spriteToProcess = _originalSprite;
            Texture2D trimmedSprite = null;
            Texture2D finalSprite = null;
            RenderTexture rt = null;

            try
            {
                if (trimAlpha)
                {
                    trimmedSprite = TrimSpriteAlpha(spriteToProcess);

                    if (trimmedSprite)
                    {
                        spriteToProcess = trimmedSprite;
                    }
                }

                Vector2Int newSize = CalculateProcessedSize(compressionFactor, trimAlpha);

                if (newSize.x < 4 || newSize.y < 4)
                {
                    Debug.LogWarning($"Skipping resize for {OriginalFileName}: Target size ({newSize.x}x{newSize.y}) is too small.");

                    finalSprite = spriteToProcess;
                }
                else if (newSize.x == spriteToProcess.width && newSize.y == spriteToProcess.height)
                {
                    finalSprite = spriteToProcess;
                }
                else
                {
                    rt = RenderTexture.GetTemporary(newSize.x, newSize.y, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Default);
                    finalSprite = new Texture2D(newSize.x, newSize.y, TextureFormat.RGBA32, false);

                    Graphics.Blit(spriteToProcess, rt);

                    RenderTexture previousActive = RenderTexture.active;
                    RenderTexture.active = rt;

                    finalSprite.ReadPixels(new Rect(0, 0, newSize.x, newSize.y), 0, 0);
                    finalSprite.Apply();

                    RenderTexture.active = previousActive;
                }


                if (finalSprite)
                {
                    byte[] bytes = finalSprite.EncodeToPNG();

                    File.WriteAllBytes(savePath, bytes);
                    return true;
                }
                else
                {
                    Debug.LogError($"Failed to generate final sprite for {OriginalFileName}.");
                    return false;
                }

            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing sprite {OriginalFileName}: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
            finally
            {
                if (rt) RenderTexture.ReleaseTemporary(rt);
                if (trimmedSprite && trimmedSprite != _originalSprite) Object.DestroyImmediate(trimmedSprite);
                if (finalSprite && finalSprite != spriteToProcess) Object.DestroyImmediate(finalSprite);
            }
        }

        private void CalculateAndCacheTrimBounds()
        {
            if (_trimBoundsCalculated || !_originalSprite) return;

            if (!_originalSprite.isReadable)
            {
                Debug.LogWarning($"Sprite {OriginalFileName} is not readable. Cannot calculate trim bounds. Make sure 'Read/Write Enabled' is checked in import settings.");

                _trimBoundsCalculated = true;
                _trimmedMinWidth = _trimmedMinHeight = 0;
                _trimmedMaxWidth = _trimmedMaxHeight = -1;
                return;
            }

            try
            {
                Color[] pixels = _originalSprite.GetPixels();
                int width = _originalSprite.width;
                int height = _originalSprite.height;

                int minX = width;
                int maxX = -1;
                int minY = height;
                int maxY = -1;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (pixels[y * width + x].a > 0.01f)
                        {
                            minX = Mathf.Min(minX, x);
                            maxX = Mathf.Max(maxX, x);
                            minY = Mathf.Min(minY, y);
                            maxY = Mathf.Max(maxY, y);
                        }
                    }
                }

                _trimmedMinWidth = minX;
                _trimmedMaxWidth = maxX;
                _trimmedMinHeight = minY;
                _trimmedMaxHeight = maxY;
            }
            catch (UnityException ex)
            {
                Debug.LogError($"Failed to GetPixels for calculating trim bounds on {OriginalFileName}: {ex.Message}. Ensure sprite is readable.");

                _trimmedMinWidth = _trimmedMinHeight = 0;
                _trimmedMaxWidth = _trimmedMaxHeight = -1;
            }
            finally
            {
                _trimBoundsCalculated = true;
            }
        }

        private Texture2D TrimSpriteAlpha(Texture2D sourceSprite)
        {
            CalculateAndCacheTrimBounds();

            if (!_trimBoundsCalculated || _trimmedMaxWidth < _trimmedMinWidth || _trimmedMaxHeight < _trimmedMinHeight) return null;

            int newWidth = _trimmedMaxWidth - _trimmedMinWidth + 1;
            int newHeight = _trimmedMaxHeight - _trimmedMinHeight + 1;

            if (newWidth == sourceSprite.width && newHeight == sourceSprite.height) return null;

            if (!sourceSprite.isReadable)
            {
                Debug.LogWarning($"Sprite {sourceSprite.name} is not readable. Cannot perform final trim.");
                return null;
            }

            try
            {
                Color[] trimmedPixels = sourceSprite.GetPixels(_trimmedMinWidth, _trimmedMinHeight, newWidth, newHeight);
                Texture2D trimmedSprite = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);

                trimmedSprite.SetPixels(trimmedPixels);
                trimmedSprite.Apply();
                return trimmedSprite;
            }
            catch (UnityException ex)
            {
                Debug.LogError($"Failed to GetPixels for trimming {sourceSprite.name}: {ex.Message}. Ensure sprite is readable.");
                return null;
            }
        }

        private Vector2Int CalculateAdjustedSize(int width, int height, float compressionFactor)
        {
            int rawNewWidth = Mathf.RoundToInt(width * compressionFactor);
            int rawNewHeight = Mathf.RoundToInt(height * compressionFactor);

            int newWidth = Mathf.Max(4, rawNewWidth);
            int newHeight = Mathf.Max(4, rawNewHeight);

            newWidth = Mathf.CeilToInt(newWidth / 4f) * 4;
            newHeight = Mathf.CeilToInt(newHeight / 4f) * 4;

            newWidth = Mathf.Max(4, newWidth);
            newHeight = Mathf.Max(4, newHeight);
            return new Vector2Int(newWidth, newHeight);
        }
    }
}