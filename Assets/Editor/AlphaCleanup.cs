using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SaqrAndAqab.Editor
{
    // Cleans background and negative-space white pixels from raw art textures.
    // Preserves eyes, teeth, and off-white details while removing enclosed background patches.
    public static class AlphaCleanup
    {
        const int BorderNearWhiteThreshold = 235; // (a) Connected to border: r,g,b >= 235
        const int EnclosedPureWhiteThreshold = 250; // (b) Enclosed holes: r,g,b >= 250
        const int EnclosedMinAreaPixels = 400; // Enclosed hole must be >= 400 px (spares small eyes/teeth)
        const int BlackKeyThreshold = 35; // For fx_crescent: r,g,b <= 35

        const string ArtPath = "Assets/Resources/Art";
        const string BackupPath = "Assets/ArtBackup";

        [MenuItem("Saqr/Clean Art Backgrounds")]
        public static void CleanAllArt()
        {
            if (!Directory.Exists(ArtPath))
            {
                Debug.LogWarning("[AlphaCleanup] Art folder not found at " + ArtPath);
                return;
            }

            if (!Directory.Exists(BackupPath))
            {
                Directory.CreateDirectory(BackupPath);
            }

            string[] files = Directory.GetFiles(ArtPath, "*.png");
            int cleanedCount = 0;

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string backupFile = Path.Combine(BackupPath, fileName);

                // Copy original backup outside Resources if not already backed up
                if (!File.Exists(backupFile))
                {
                    File.Copy(file, backupFile, false);
                }

                byte[] raw = File.ReadAllBytes(file);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!tex.LoadImage(raw))
                {
                    UnityEngine.Object.DestroyImmediate(tex);
                    continue;
                }

                int w = tex.width;
                int h = tex.height;
                Color32[] pixels = tex.GetPixels32();
                bool modified = false;

                bool isFxBlackBg = fileName.StartsWith("fx_crescent", StringComparison.OrdinalIgnoreCase);

                if (isFxBlackBg)
                {
                    // Key out near-black
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        var p = pixels[i];
                        if (p.r <= BlackKeyThreshold && p.g <= BlackKeyThreshold && p.b <= BlackKeyThreshold)
                        {
                            if (p.a != 0)
                            {
                                pixels[i] = new Color32(0, 0, 0, 0);
                                modified = true;
                            }
                        }
                    }
                }
                else
                {
                    // (a) Flood fill from outer image borders for pixels with r,g,b >= BorderNearWhiteThreshold
                    bool[] visitedBorder = new bool[w * h];
                    var queue = new Queue<int>();

                    for (int x = 0; x < w; x++)
                    {
                        TryEnqueueBorder(x, 0, w, h, pixels, visitedBorder, queue);
                        TryEnqueueBorder(x, h - 1, w, h, pixels, visitedBorder, queue);
                    }
                    for (int y = 0; y < h; y++)
                    {
                        TryEnqueueBorder(0, y, w, h, pixels, visitedBorder, queue);
                        TryEnqueueBorder(w - 1, y, w, h, pixels, visitedBorder, queue);
                    }

                    while (queue.Count > 0)
                    {
                        int idx = queue.Dequeue();
                        if (pixels[idx].a != 0)
                        {
                            pixels[idx] = new Color32(0, 0, 0, 0);
                            modified = true;
                        }

                        int x = idx % w;
                        int y = idx / w;

                        if (x > 0) TryEnqueueBorder(x - 1, y, w, h, pixels, visitedBorder, queue);
                        if (x < w - 1) TryEnqueueBorder(x + 1, y, w, h, pixels, visitedBorder, queue);
                        if (y > 0) TryEnqueueBorder(x, y - 1, w, h, pixels, visitedBorder, queue);
                        if (y < h - 1) TryEnqueueBorder(x, y + 1, w, h, pixels, visitedBorder, queue);
                    }

                    // (b) Find enclosed pure-white regions (r,g,b >= 250) with area >= 400 px
                    bool[] visitedHole = new bool[w * h];
                    var component = new List<int>();

                    for (int i = 0; i < pixels.Length; i++)
                    {
                        if (visitedHole[i] || visitedBorder[i]) continue;
                        var p = pixels[i];
                        if (p.a == 0 || p.r < EnclosedPureWhiteThreshold || p.g < EnclosedPureWhiteThreshold || p.b < EnclosedPureWhiteThreshold)
                        {
                            continue;
                        }

                        // Explore connected component
                        component.Clear();
                        var compQueue = new Queue<int>();
                        compQueue.Enqueue(i);
                        visitedHole[i] = true;

                        while (compQueue.Count > 0)
                        {
                            int curr = compQueue.Dequeue();
                            component.Add(curr);

                            int cx = curr % w;
                            int cy = curr / w;

                            CheckHoleNeighbor(cx - 1, cy, w, h, pixels, visitedHole, visitedBorder, compQueue);
                            CheckHoleNeighbor(cx + 1, cy, w, h, pixels, visitedHole, visitedBorder, compQueue);
                            CheckHoleNeighbor(cx, cy - 1, w, h, pixels, visitedHole, visitedBorder, compQueue);
                            CheckHoleNeighbor(cx, cy + 1, w, h, pixels, visitedHole, visitedBorder, compQueue);
                        }

                        // If the enclosed region is large enough (>= EnclosedMinAreaPixels), clear it
                        if (component.Count >= EnclosedMinAreaPixels)
                        {
                            for (int c = 0; c < component.Count; c++)
                            {
                                pixels[component[c]] = new Color32(0, 0, 0, 0);
                                modified = true;
                            }
                        }
                    }
                }

                if (modified)
                {
                    var cleanTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                    cleanTex.SetPixels32(pixels);
                    cleanTex.Apply(false, false);
                    byte[] png = cleanTex.EncodeToPNG();
                    File.WriteAllBytes(file, png);
                    UnityEngine.Object.DestroyImmediate(cleanTex);
                    cleanedCount++;
                    Debug.Log("[AlphaCleanup] Cleaned background and negative space: " + fileName);
                }

                UnityEngine.Object.DestroyImmediate(tex);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[AlphaCleanup] Done. {cleanedCount} file(s) updated in {ArtPath}. Originals backed up to {BackupPath}.");
        }

        static void TryEnqueueBorder(int x, int y, int w, int h, Color32[] pixels, bool[] visited, Queue<int> queue)
        {
            int idx = y * w + x;
            if (visited[idx]) return;
            visited[idx] = true;

            var p = pixels[idx];
            if (p.a == 0 || (p.r >= BorderNearWhiteThreshold && p.g >= BorderNearWhiteThreshold && p.b >= BorderNearWhiteThreshold))
            {
                queue.Enqueue(idx);
            }
        }

        static void CheckHoleNeighbor(int x, int y, int w, int h, Color32[] pixels, bool[] visitedHole, bool[] visitedBorder, Queue<int> queue)
        {
            if (x < 0 || x >= w || y < 0 || y >= h) return;
            int idx = y * w + x;
            if (visitedHole[idx] || visitedBorder[idx]) return;

            var p = pixels[idx];
            if (p.a > 0 && p.r >= EnclosedPureWhiteThreshold && p.g >= EnclosedPureWhiteThreshold && p.b >= EnclosedPureWhiteThreshold)
            {
                visitedHole[idx] = true;
                queue.Enqueue(idx);
            }
        }
    }
}
