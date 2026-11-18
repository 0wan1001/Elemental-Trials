using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ElementalTrials.PlayerSheet.Editor
{
    // 큰 원본 그림(witch_<동작>_<방향>.png, 가로로 프레임이 이어진 띠)을 게임 크기(원본 칸 높이 → 32px) 8방향 시트로 줄여 만들고,
    // 열린 씬의 PlayerSpriteAnimator에 연결한다. 동작은 walk(걷기), idle(대기). 원본을 바꾸면 메뉴를 다시 실행하면 된다.
    // 원본 방향은 5개(front, front34, side, back34, back)이고 나머지 3방향은 좌우 반전으로 만든다.
    public static class PlayerSheetBuilder
    {
        private const string OUTPUT_DIR = "Assets/Art/Player";
        private const string SOURCE_PREF = "ElementalTrials.PlayerSheet.SourceDir";
        private const int CELL = 32;
        private const int PIXELS_PER_UNIT = 32;
        private const byte MIN_ALPHA = 20;

        // PlayerSpriteAnimator 방향 순서(위, 왼쪽 위, 왼쪽, 왼쪽 아래, 아래, 오른쪽 아래, 오른쪽, 오른쪽 위)에 쓸 원본과 반전 여부
        private static readonly (string source, bool flip)[] DIRECTIONS =
        {
            ("back", false), ("back34", true), ("side", false), ("front34", false),
            ("front", false), ("front34", true), ("side", true), ("back34", false),
        };

        [MenuItem("Tools/Player Sheet/Build Walk...")]
        private static void BuildWalkMenu() => BuildFromMenu("walk", "_walkFrames");

        [MenuItem("Tools/Player Sheet/Build Idle...")]
        private static void BuildIdleMenu() => BuildFromMenu("idle", "_idleFrames");

        private static void BuildFromMenu(string anim, string field)
        {
            string dir = EditorUtility.OpenFolderPanel($"witch_{anim}_*.png가 있는 폴더", EditorPrefs.GetString(SOURCE_PREF, ""), "");
            if (string.IsNullOrEmpty(dir)) return;
            EditorPrefs.SetString(SOURCE_PREF, dir);
            Debug.Log(Build(anim, field, dir));
        }

        // unity CLI에서도 부를 수 있다. 결과 요약 문자열을 돌려준다.
        public static string Build(string anim, string field, string sourceDir)
        {
            var strips = new Dictionary<string, Color32[][]>();
            int frameW = 0, frameH = 0, frameCount = -1;
            foreach (string name in DIRECTIONS.Select(d => d.source).Distinct())
            {
                string path = Path.Combine(sourceDir, $"witch_{anim}_{name}.png");
                if (!File.Exists(path)) throw new FileNotFoundException("원본이 없습니다", path);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(path));
                int count = CountFrames(tex);
                if (frameCount >= 0 && count != frameCount) throw new System.Exception($"{name}의 프레임 수({count})가 다른 파일({frameCount})과 다릅니다");
                frameCount = count;
                frameW = tex.width / count;
                frameH = tex.height;
                strips[name] = SplitFrames(tex, count);
                Object.DestroyImmediate(tex);
            }

            // 원본 칸 높이를 게임 칸 높이에 맞춘다. 그림 범위로 맞추면 동작마다(걷기·대기) 배율이 달라져 캐릭터 크기가 튄다.
            // 발은 원본 칸 맨 아래, 가로 가운데는 원본 칸 가운데를 기준으로 한다 (모든 동작이 같은 기준).
            var bounds = new RectInt(0, 0, frameW, frameH);
            float scale = (float)CELL / frameH;

            var sheet = new Texture2D(CELL * frameCount, CELL * DIRECTIONS.Length, TextureFormat.RGBA32, false);
            sheet.SetPixels32(new Color32[sheet.width * sheet.height]);
            for (int d = 0; d < DIRECTIONS.Length; d++)
            {
                var (source, flip) = DIRECTIONS[d];
                int row = DIRECTIONS.Length - 1 - d; // 위 방향이 시트 맨 위 줄
                for (int f = 0; f < frameCount; f++)
                {
                    Color32[] cell = Downscale(strips[source][f], frameW, frameH, bounds, scale, flip);
                    sheet.SetPixels32(f * CELL, row * CELL, CELL, CELL, cell);
                }
            }
            sheet.Apply();

            string assetPath = $"{OUTPUT_DIR}/player_{anim}.png";
            Directory.CreateDirectory(OUTPUT_DIR);
            File.WriteAllBytes(assetPath, sheet.EncodeToPNG());
            Object.DestroyImmediate(sheet);
            AssetDatabase.ImportAsset(assetPath);
            ConfigureTexture(assetPath, anim, frameCount);

            Sprite[] frames = LoadFrames(assetPath, anim, frameCount);
            string assigned = AssignToAnimator(field, frames);
            return $"player sheet {anim}: {DIRECTIONS.Length}방향 × {frameCount}프레임, 원본 {frameW}x{frameH} → {CELL}x{CELL} (배율 {scale:F3}) -> {assetPath}. {assigned}";
        }

        // 가로 띠에서 그림이 있는 열 덩어리 수 = 프레임 수
        private static int CountFrames(Texture2D tex)
        {
            Color32[] px = tex.GetPixels32();
            int runs = 0;
            bool prev = false;
            for (int x = 0; x < tex.width; x++)
            {
                bool any = false;
                for (int y = 0; y < tex.height && !any; y++) any = px[y * tex.width + x].a > MIN_ALPHA;
                if (any && !prev) runs++;
                prev = any;
            }
            return runs;
        }

        private static Color32[][] SplitFrames(Texture2D tex, int count)
        {
            int w = tex.width / count;
            var frames = new Color32[count][];
            for (int i = 0; i < count; i++)
            {
                frames[i] = tex.GetPixels(i * w, 0, w, tex.height).Select(c => (Color32)c).ToArray();
            }
            return frames;
        }

        // 칸 하나를 원본 블록 하나로 보고, 블록의 절반 이상이 그림이면 가장 많이 나온 색을 쓴다 (도트가 뭉개지지 않게)
        private static Color32[] Downscale(Color32[] src, int w, int h, RectInt bounds, float scale, bool flip)
        {
            var cell = new Color32[CELL * CELL];
            float block = 1f / scale;
            float centerX = bounds.x + bounds.width * 0.5f;
            var counts = new Dictionary<int, (int n, int r, int g, int b)>();
            for (int ty = 0; ty < CELL; ty++)
            {
                for (int tx = 0; tx < CELL; tx++)
                {
                    // 발(그림 맨 아래)을 칸 맨 아랫줄에, 그림 가운데를 칸 가운데에
                    float sx0 = centerX + (tx - CELL * 0.5f) * block;
                    float sy0 = bounds.y + ty * block;
                    int x0 = Mathf.FloorToInt(sx0), x1 = Mathf.CeilToInt(sx0 + block);
                    int y0 = Mathf.FloorToInt(sy0), y1 = Mathf.CeilToInt(sy0 + block);
                    counts.Clear();
                    int total = 0, opaque = 0;
                    for (int y = y0; y < y1; y++)
                    {
                        for (int x = x0; x < x1; x++)
                        {
                            total++;
                            if (x < 0 || x >= w || y < 0 || y >= h) continue;
                            Color32 c = src[y * w + x];
                            if (c.a <= 128) continue;
                            opaque++;
                            int key = (c.r >> 3) << 10 | (c.g >> 3) << 5 | (c.b >> 3);
                            counts.TryGetValue(key, out var e);
                            counts[key] = (e.n + 1, e.r + c.r, e.g + c.g, e.b + c.b);
                        }
                    }
                    if (total == 0 || opaque * 2 < total) continue;
                    var best = counts.Values.OrderByDescending(v => v.n).First();
                    int outX = flip ? CELL - 1 - tx : tx;
                    cell[ty * CELL + outX] = new Color32((byte)(best.r / best.n), (byte)(best.g / best.n), (byte)(best.b / best.n), 255);
                }
            }
            return cell;
        }

        private static string SpriteName(string anim, int direction, int frame) => $"player_{anim}_{direction}_{frame:D2}";

        private static void ConfigureTexture(string assetPath, string anim, int frameCount)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = PIXELS_PER_UNIT;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            ISpriteEditorDataProvider provider = factory.GetSpriteEditorDataProviderFromObject(importer);
            provider.InitSpriteEditorDataProvider();
            // 다시 만들 때 같은 이름의 스프라이트 ID를 유지해서 씬의 연결이 깨지지 않게 한다
            Dictionary<string, GUID> existingIds = provider.GetSpriteRects().ToDictionary(r => r.name, r => r.spriteID);

            var rects = new List<SpriteRect>();
            for (int d = 0; d < DIRECTIONS.Length; d++)
            {
                int row = DIRECTIONS.Length - 1 - d;
                for (int f = 0; f < frameCount; f++)
                {
                    string name = SpriteName(anim, d, f);
                    rects.Add(new SpriteRect
                    {
                        name = name,
                        rect = new Rect(f * CELL, row * CELL, CELL, CELL),
                        alignment = SpriteAlignment.Center, // 기존 그림과 같이 가운데 기준 (발은 중심에서 16px 아래)
                        pivot = new Vector2(0.5f, 0.5f),
                        spriteID = existingIds.TryGetValue(name, out GUID id) ? id : GUID.Generate(),
                    });
                }
            }
            provider.SetSpriteRects(rects.ToArray());
            var nameIds = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            nameIds?.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());
            provider.Apply();
            importer.SaveAndReimport();
        }

        private static Sprite[] LoadFrames(string assetPath, string anim, int frameCount)
        {
            Dictionary<string, Sprite> byName = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToDictionary(s => s.name);
            var frames = new Sprite[DIRECTIONS.Length * frameCount];
            for (int d = 0; d < DIRECTIONS.Length; d++)
            {
                for (int f = 0; f < frameCount; f++) frames[d * frameCount + f] = byName[SpriteName(anim, d, f)];
            }
            return frames;
        }

        private static string AssignToAnimator(string field, Sprite[] frames)
        {
            var animator = Object.FindFirstObjectByType<PlayerSpriteAnimator>();
            if (animator == null) return "열린 씬에 PlayerSpriteAnimator가 없어 연결하지 않았습니다.";
            var so = new SerializedObject(animator);
            SerializedProperty list = so.FindProperty(field);
            list.arraySize = frames.Length;
            for (int i = 0; i < frames.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
            so.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(animator.gameObject.scene);
            EditorSceneManager.SaveScene(animator.gameObject.scene);
            return $"{animator.gameObject.scene.name} 씬의 {field}에 {frames.Length}장 연결.";
        }
    }
}
