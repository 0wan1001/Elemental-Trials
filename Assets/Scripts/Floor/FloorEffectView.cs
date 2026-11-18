using System;
using UnityEngine;

// TileGrid의 상태 시간을 읽어 장판 효과를 그린다. 판정에는 관여하지 않는다.
public class FloorEffectView : MonoBehaviour
{
    private const float MASK_PIXELS_PER_UNIT = 2f;

    // 셰이더 키워드 _FX_<이름> 과 1:1로 맞는다
    private enum Effect
    {
        Water,
        Fire,
        Steam,
        SteamTop,
        Electric,
        Warning,
    }

    [Serializable]
    private class Layer
    {
        public Effect Effect;
        public FloorState State;
        public Color Color;
        public int SortingOrder;
        // 남은 시간이 이 값보다 작아지면 가장자리부터 줄어든다. 0이면 줄어들지 않는다.
        public float FadeDuration;
        // 상태가 끝난 뒤에도 보이게 남겨 두는 시간 (보이는 것만, 판정과 무관)
        public float LingerDuration;
    }

    [SerializeField] private TileGrid _tileGrid;
    [SerializeField] private Material _material;
    [SerializeField] private Layer[] _layers;

    private Texture2D[] _masks;
    private SpriteRenderer[] _renderers;
    private byte[] _pixels;
    private float[][,] _visibleTimers;

    private void Awake()
    {
        int count = _layers.Length;
        _masks = new Texture2D[count];
        _renderers = new SpriteRenderer[count];
        _pixels = new byte[TileGrid.GRID_WIDTH * TileGrid.GRID_HEIGHT];
        _visibleTimers = new float[count][,];

        for (int i = 0; i < count; i++)
        {
            Layer layer = _layers[i];
            var mask = new Texture2D(TileGrid.GRID_WIDTH, TileGrid.GRID_HEIGHT, TextureFormat.R8, false);
            mask.wrapMode = TextureWrapMode.Clamp;
            _masks[i] = mask;
            _visibleTimers[i] = new float[TileGrid.GRID_WIDTH, TileGrid.GRID_HEIGHT];

            var renderer = new GameObject(layer.Effect.ToString()).AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(transform, false);
            renderer.sprite = Sprite.Create(mask, new Rect(0, 0, TileGrid.GRID_WIDTH, TileGrid.GRID_HEIGHT),
                Vector2.zero, MASK_PIXELS_PER_UNIT);
            renderer.sortingOrder = layer.SortingOrder;
            renderer.material = _material;
            _renderers[i] = renderer;

            Material material = renderer.material;
            material.SetColor("_Color", layer.Color);
            material.EnableKeyword("_FX_" + layer.Effect.ToString().ToUpperInvariant());
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < _layers.Length; i++)
        {
            Layer layer = _layers[i];
            bool hasAny = FillMask(layer, _visibleTimers[i]);
            _renderers[i].enabled = hasAny;
            if (!hasAny) continue;
            _masks[i].SetPixelData(_pixels, 0);
            _masks[i].Apply(false);

            if (layer.Effect == Effect.Warning)
            {
                float blink = 0.2f + 0.25f * (Mathf.Sin(Time.time * 20f) * 0.5f + 0.5f);
                Color color = layer.Color;
                color.a *= blink;
                _renderers[i].sharedMaterial.SetColor("_Color", color);
            }
        }
    }

    private bool FillMask(Layer layer, float[,] visibleTimers)
    {
        bool hasAny = false;
        for (int y = 0; y < TileGrid.GRID_HEIGHT; y++)
        {
            for (int x = 0; x < TileGrid.GRID_WIDTH; x++)
            {
                FloorState state = layer.State;
                float timer = _tileGrid.GetTimer(state, x, y);
                if (state == FloorState.Wet && _tileGrid.IsPool(x, y)) timer = 0f;

                float visible = Mathf.Max(visibleTimers[x, y] - Time.deltaTime, 0f);
                if (timer > 0f) visible = Mathf.Max(visible, timer + layer.LingerDuration);
                visibleTimers[x, y] = visible;

                float strength = layer.FadeDuration > 0f ? Mathf.Clamp01(visible / layer.FadeDuration) : (visible > 0f ? 1f : 0f);
                _pixels[y * TileGrid.GRID_WIDTH + x] = (byte)(strength * 255f);
                hasAny |= strength > 0f;
            }
        }
        return hasAny;
    }
}
