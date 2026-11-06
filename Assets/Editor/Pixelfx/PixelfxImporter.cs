using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ElementalTrials.Pixelfx.Editor
{
    /// <summary>
    /// pixelfx 스프라이트 시트(PNG + Aseprite 형식 JSON)를 Unity 스프라이트·애니메이션으로 가져온다.
    /// 흐름: render.mjs --sheet → Assets/VFX/pixelfx/&lt;이름&gt;/ 로 복사 → 텍스처 설정·프레임 자르기 → .anim, .controller 생성
    /// </summary>
    public static class PixelfxImporter
    {
        private const string MENU_ROOT = "Tools/pixelfx/";
        private const string OUTPUT_ROOT = "Assets/VFX/pixelfx";
        private const string SETUP_SCRIPT = "Tools/pixelfx-setup.mjs";
        private const string CHECKOUT_DIR = "External/pixelfx";
        private const int PIXELS_PER_UNIT = 16;
        // 이보다 옅은 픽셀(약 6%)은 지운다. 거의 안 보이는 빛·연기 때문에 여백이 안 잘리는 것을 막는다.
        private const byte MIN_ALPHA = 16;

        private static string ProjectRoot => Path.GetDirectoryName(Application.dataPath);

        [MenuItem(MENU_ROOT + "Render && Import...")]
        private static void OpenRenderWindow()
        {
            EditorWindow.GetWindow<PixelfxRenderWindow>(true, "pixelfx Render & Import");
        }

        [MenuItem(MENU_ROOT + "Import Sheet...")]
        private static void ImportSheetFromPanel()
        {
            string jsonPath = EditorUtility.OpenFilePanel("pixelfx 시트 JSON 선택", Path.Combine(ProjectRoot, CHECKOUT_DIR, "records"), "json");
            if (string.IsNullOrEmpty(jsonPath))
            {
                return;
            }

            UnityEngine.Debug.Log(ImportSheet(jsonPath));
        }

        /// <summary>
        /// 이펙트를 렌더하고 가져온다. unity CLI(script eval)에서도 호출한다.
        /// </summary>
        /// <param name="effectId">예: "lightning/lightning-strike"</param>
        /// <param name="optionsJson">render.mjs --opts 값. 없으면 null</param>
        /// <returns>결과 요약</returns>
        /// <param name="fps">렌더 fps. 0이면 이펙트 기본값</param>
        public static string RenderAndImport(string effectId, string optionsJson, int fps = 0)
        {
            RunNode(ProjectRoot, SETUP_SCRIPT);

            string checkout = Path.Combine(ProjectRoot, CHECKOUT_DIR);
            var args = new List<string> { "tools/render.mjs", effectId, "--sheet" };
            if (!string.IsNullOrWhiteSpace(optionsJson))
            {
                args.Add("--opts");
                args.Add(optionsJson);
            }

            if (fps > 0)
            {
                args.Add("--fps");
                args.Add(fps.ToString());
            }

            RunNode(checkout, args.ToArray());

            string name = effectId.Split('/').Last();
            string jsonPath = Path.Combine(checkout, "records", effectId, name + ".json");
            return ImportSheet(jsonPath);
        }

        /// <summary>
        /// 렌더된 시트(같은 폴더의 PNG + JSON)를 가져온다.
        /// 여백을 잘라내고 끝의 빈 프레임을 지운 뒤, 프레임과 시트 크기를 2의 거듭제곱으로 다시 배치한다.
        /// </summary>
        public static string ImportSheet(string jsonPath)
        {
            var source = JsonUtility.FromJson<SheetJson>(File.ReadAllText(jsonPath));
            string name = Path.GetFileNameWithoutExtension(source.meta.image);
            string sourcePng = Path.Combine(Path.GetDirectoryName(jsonPath), source.meta.image);

            string folder = OUTPUT_ROOT + "/" + name;
            Directory.CreateDirectory(Path.Combine(ProjectRoot, folder));
            string pngAsset = folder + "/" + name + ".png";

            SheetJson sheet = RepackPowerOfTwo(source, File.ReadAllBytes(sourcePng), Path.Combine(ProjectRoot, pngAsset));
            PixelfxMeta fx = sheet.meta.pixelfx;
            File.WriteAllText(Path.Combine(ProjectRoot, folder, name + ".json"), JsonUtility.ToJson(sheet, true));

            // 처음 가져올 때만 먼저 임포트해서 TextureImporter를 만든다.
            // 이미 있으면 바로 설정을 바꾸고 한 번만 임포트한다 (옛 프레임 위치로 임포트되며 경고가 쌓이는 것을 막는다).
            if (AssetImporter.GetAtPath(pngAsset) == null)
            {
                AssetDatabase.ImportAsset(pngAsset, ImportAssetOptions.ForceSynchronousImport);
            }

            ConfigureTexture(pngAsset, sheet, fx);
            Sprite[] sprites = LoadSpritesInOrder(pngAsset, name, sheet.frames.Length);
            AnimationClip clip = CreateClip(folder + "/" + name + ".anim", sheet, fx, sprites);
            CreateController(folder + "/" + name + ".controller", clip);

            AssetDatabase.SaveAssets();
            return $"pixelfx imported {fx.id}: {sprites.Length}/{source.frames.Length} frames, {fx.fps} fps, loop={fx.loop}, " +
                   $"frame {source.meta.pixelfx.frameSize[0]}x{source.meta.pixelfx.frameSize[1]} -> content {fx.contentSize[0]}x{fx.contentSize[1]} -> {fx.frameSize[0]}x{fx.frameSize[1]}, " +
                   $"sheet {sheet.meta.size.w}x{sheet.meta.size.h}, pivot=({fx.pivot[0]},{fx.pivot[1]}) -> {folder}";
        }

        private static SheetJson RepackPowerOfTwo(SheetJson source, byte[] png, string outputPng)
        {
            var src = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            src.LoadImage(png);
            Color32[] pixels = src.GetPixels32();
            int srcW = src.width;
            int srcH = src.height;
            UnityEngine.Object.DestroyImmediate(src);
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a < MIN_ALPHA)
                {
                    pixels[i] = default;
                }
            }

            // 프레임 안의 좌표는 왼쪽 위가 원점이다. Unity 텍스처의 픽셀 배열은 왼쪽 아래가 원점이다.
            Color32 Pixel(FrameRect f, int x, int y) => pixels[(srcH - 1 - (f.y + y)) * srcW + f.x + x];

            // 1. 모든 프레임에서 그림이 있는 영역(합집합)과, 마지막으로 그림이 있는 프레임을 찾는다.
            int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1, lastFrame = 0;
            for (int i = 0; i < source.frames.Length; i++)
            {
                FrameRect f = source.frames[i].frame;
                for (int y = 0; y < f.h; y++)
                {
                    for (int x = 0; x < f.w; x++)
                    {
                        if (Pixel(f, x, y).a == 0)
                        {
                            continue;
                        }

                        minX = Mathf.Min(minX, x);
                        minY = Mathf.Min(minY, y);
                        maxX = Mathf.Max(maxX, x);
                        maxY = Mathf.Max(maxY, y);
                        lastFrame = i;
                    }
                }
            }

            if (maxX < 0)
            {
                throw new Exception("pixelfx sheet is empty: " + source.meta.image);
            }

            int contentW = maxX - minX + 1;
            int contentH = maxY - minY + 1;
            int count = lastFrame + 1;

            // 2. 프레임 크기를 2의 거듭제곱으로 올리고, 그림을 가운데에 둔다.
            int cellW = Mathf.NextPowerOfTwo(contentW);
            int cellH = Mathf.NextPowerOfTwo(contentH);
            int padX = (cellW - contentW) / 2;
            int padY = (cellH - contentH) / 2;

            // 3. 시트도 2의 거듭제곱이 되도록 열 수를 고른다 (가장 작은 넓이, 같으면 정사각형에 가까운 쪽).
            int bestCols = 1, bestW = 0, bestH = 0;
            long bestArea = long.MaxValue;
            for (int cols = 1; cols <= count * 2; cols *= 2)
            {
                int rows = (count + cols - 1) / cols;
                int w = cols * cellW;
                int h = Mathf.NextPowerOfTwo(rows * cellH);
                long area = (long)w * h;
                if (area < bestArea || (area == bestArea && Mathf.Abs(w - h) < Mathf.Abs(bestW - bestH)))
                {
                    bestArea = area;
                    bestCols = cols;
                    bestW = w;
                    bestH = h;
                }
            }

            var dst = new Color32[bestW * bestH];
            var frames = new FrameEntry[count];
            for (int i = 0; i < count; i++)
            {
                FrameRect f = source.frames[i].frame;
                int cellX = (i % bestCols) * cellW;
                int cellY = (i / bestCols) * cellH;
                for (int y = 0; y < contentH; y++)
                {
                    for (int x = 0; x < contentW; x++)
                    {
                        int dx = cellX + padX + x;
                        int dy = cellY + padY + y;
                        dst[(bestH - 1 - dy) * bestW + dx] = Pixel(f, minX + x, minY + y);
                    }
                }

                frames[i] = new FrameEntry
                {
                    frame = new FrameRect { x = cellX, y = cellY, w = cellW, h = cellH },
                    duration = source.frames[i].duration,
                };
            }

            var tex = new Texture2D(bestW, bestH, TextureFormat.RGBA32, false);
            tex.SetPixels32(dst);
            File.WriteAllBytes(outputPng, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);

            PixelfxMeta fx = source.meta.pixelfx;
            return new SheetJson
            {
                frames = frames,
                meta = new SheetMeta
                {
                    image = source.meta.image,
                    size = new SheetSize { w = bestW, h = bestH },
                    pixelfx = new PixelfxMeta
                    {
                        id = fx.id,
                        fps = fx.fps,
                        loop = fx.loop,
                        frameSize = new[] { cellW, cellH },
                        contentSize = new[] { contentW, contentH },
                        pivot = new[] { fx.pivot[0] - minX + padX, fx.pivot[1] - minY + padY },
                    },
                },
            };
        }

        private static void ConfigureTexture(string pngAsset, SheetJson sheet, PixelfxMeta fx)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(pngAsset);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = PIXELS_PER_UNIT;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = Mathf.Max(2048, Mathf.NextPowerOfTwo(Mathf.Max(sheet.meta.size.w, sheet.meta.size.h)));

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();

            // 다시 가져올 때 같은 이름의 스프라이트 ID를 유지해서 기존 참조가 깨지지 않게 한다.
            Dictionary<string, GUID> existingIds = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);
            var pivot = new Vector2((float)fx.pivot[0] / fx.frameSize[0], 1f - (float)fx.pivot[1] / fx.frameSize[1]);

            var rects = new SpriteRect[sheet.frames.Length];
            for (int i = 0; i < sheet.frames.Length; i++)
            {
                FrameRect f = sheet.frames[i].frame;
                string spriteName = FrameName(fx, i);
                rects[i] = new SpriteRect
                {
                    name = spriteName,
                    // JSON은 왼쪽 위가 원점, Unity 텍스처는 왼쪽 아래가 원점이다.
                    rect = new Rect(f.x, sheet.meta.size.h - f.y - f.h, f.w, f.h),
                    alignment = SpriteAlignment.Custom,
                    pivot = pivot,
                    spriteID = existingIds.TryGetValue(spriteName, out GUID id) ? id : GUID.Generate(),
                };
            }

            provider.SetSpriteRects(rects);
            var nameIds = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            nameIds?.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            provider.Apply();
            importer.SaveAndReimport();
        }

        private static Sprite[] LoadSpritesInOrder(string pngAsset, string name, int count)
        {
            Dictionary<string, Sprite> byName = AssetDatabase.LoadAllAssetsAtPath(pngAsset).OfType<Sprite>().ToDictionary(s => s.name);
            var result = new Sprite[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = byName[name + "_" + i.ToString("D3")];
            }

            return result;
        }

        private static AnimationClip CreateClip(string path, SheetJson sheet, PixelfxMeta fx, Sprite[] sprites)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.frameRate = fx.fps;

            // 프레임마다 JSON의 duration(ms)을 누적해서 키를 찍는다. 마지막 키는 마지막 프레임이 끝나는 시점.
            var keys = new ObjectReferenceKeyframe[sprites.Length + 1];
            float time = 0f;
            for (int i = 0; i < sprites.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = time, value = sprites[i] };
                time += sheet.frames[i].duration / 1000f;
            }

            keys[sprites.Length] = new ObjectReferenceKeyframe { time = time, value = sprites[sprites.Length - 1] };

            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = fx.loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void CreateController(string path, AnimationClip clip)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) == null)
            {
                AnimatorController.CreateAnimatorControllerAtPathWithClip(path, clip);
            }
        }

        private static string FrameName(PixelfxMeta fx, int index)
        {
            return fx.id.Split('/').Last() + "_" + index.ToString("D3");
        }

        private static void RunNode(string workingDirectory, params string[] args)
        {
            var info = new ProcessStartInfo("node")
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            foreach (string arg in args)
            {
                info.ArgumentList.Add(arg);
            }

            using Process process = Process.Start(info);
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception($"node {string.Join(" ", args)} failed ({process.ExitCode}):\n{error}\n{output}");
            }
        }

        // render.mjs --sheet 가 쓰는 JSON 형식 (필요한 필드만)
        [Serializable]
        private class SheetJson
        {
            public FrameEntry[] frames;
            public SheetMeta meta;
        }

        [Serializable]
        private class FrameEntry
        {
            public FrameRect frame;
            public int duration;
        }

        [Serializable]
        private class FrameRect
        {
            public int x;
            public int y;
            public int w;
            public int h;
        }

        [Serializable]
        private class SheetMeta
        {
            public string image;
            public SheetSize size;
            public PixelfxMeta pixelfx;
        }

        [Serializable]
        private class SheetSize
        {
            public int w;
            public int h;
        }

        [Serializable]
        private class PixelfxMeta
        {
            public string id;
            public int fps;
            public bool loop;
            public int[] frameSize;
            public int[] pivot;
            public int[] contentSize;
        }
    }

    /// <summary>
    /// Tools/pixelfx/Render &amp;&amp; Import... 창. 이펙트 id와 옵션을 입력받는다.
    /// </summary>
    public class PixelfxRenderWindow : EditorWindow
    {
        private string _effectId = "lightning/lightning-strike";
        private string _optionsJson = "";
        private int _fps;

        private void OnGUI()
        {
            EditorGUILayout.LabelField("이펙트 id (External/pixelfx/docs/CATALOG.md 참고)");
            _effectId = EditorGUILayout.TextField(_effectId);
            EditorGUILayout.LabelField("--opts JSON (선택, 예: {\"seed\": 3})");
            _optionsJson = EditorGUILayout.TextField(_optionsJson);
            _fps = EditorGUILayout.IntField("fps (0 = 기본값)", _fps);

            if (GUILayout.Button("Render && Import"))
            {
                try
                {
                    EditorUtility.DisplayProgressBar("pixelfx", "Rendering " + _effectId, 0.5f);
                    UnityEngine.Debug.Log(PixelfxImporter.RenderAndImport(_effectId, _optionsJson, _fps));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError(e.Message);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }
    }
}
