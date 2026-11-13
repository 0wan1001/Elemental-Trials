using UnityEngine;

public class TileGrid : MonoBehaviour
{
    public const int GRID_WIDTH = 40;
    public const int GRID_HEIGHT = 22;
    public const float PIXELS_PER_UNIT = 2f;
    private const int STATE_COUNT = 5;
    
    private static readonly Color[] STATE_COLORS =
    {
        new Color(0.2f, 0.47f, 0.82f, 0.45f),   // Wet: 반투명 파랑
        new Color(1f, 0.35f, 0.08f, 0.6f),      // Fire: 주황
        new Color(0.88f, 0.91f, 0.94f, 0.5f),   // Steam: 흰 회색
        new Color(0.55f, 0.94f, 1f, 0.7f),      // Electric: 밝은 하늘색
        new Color(1f, 0.2f, 0.16f, 0.4f),       // Warning: 빨강
    };

    [SerializeField] private SpriteRenderer _floorOverlay;
    [SerializeField] private RectInt[] _poolAreas;
    
    private Texture2D _floorTexture;
    private Grid _grid;
    
    private float[][,] _timers = new float[STATE_COUNT][,];
    private float[,] _pendingFireTime = new float[GRID_WIDTH, GRID_HEIGHT];
    private bool[,] _isPool = new bool[GRID_WIDTH, GRID_HEIGHT];
    
    private void Awake()
    {
        foreach (var area in _poolAreas)
        {
            for (int x = area.xMin; x < area.xMax; x++)
            {
                for (int y = area.yMin; y < area.yMax; y++)
                {
                    _isPool[x, y] = true;
                }
            }
        }
        _grid = GetComponent<Grid>();

        for (int s = 0; s < STATE_COUNT; s++)
        {
            _timers[s] = new float[GRID_WIDTH, GRID_HEIGHT];
            
        }

        _floorTexture = new Texture2D(GRID_WIDTH, GRID_HEIGHT);
        _floorTexture.filterMode = FilterMode.Point;
        _floorOverlay.sprite = Sprite.Create(_floorTexture, new Rect(0, 0, GRID_WIDTH, GRID_HEIGHT), Vector2.zero,
            PIXELS_PER_UNIT);
    }

    private void Update()
    {
        UpdateTimer();
        DrawFloor();
    }

    public bool TryGetCell(Vector2 worldPosition, out Vector2Int cellNumber)
    {
        Vector3Int cell = _grid.WorldToCell(worldPosition);
        cellNumber = (Vector2Int)cell;
        return (cell.x >= 0 && cell.x < GRID_WIDTH && cell.y >= 0 && cell.y < GRID_HEIGHT);
    }

    private void UpdateTimer()
    {
        float[,] warning = _timers[(int)FloorState.Warning];
        float[,] fire = _timers[(int)FloorState.Fire];

        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                for (int s = 0; s < STATE_COUNT; s++)
                {
                    if (s == (int)FloorState.Warning) continue;
                    if (_timers[s][x, y] > 0f) _timers[s][x, y] -= Time.deltaTime;
                }
                if (_isPool[x, y]) _timers[(int)FloorState.Wet][x, y] = 1;
                
                if (warning[x, y] > 0f)
                {
                    warning[x, y] -= Time.deltaTime;
                    if (warning[x, y] <= 0f)
                    {
                        fire[x, y] = _pendingFireTime[x, y];
                    }
                }
            }
        }
    }

    private void DrawFloor()
    {
        float blink = 0.2f + 0.25f * (Mathf.Sin(Time.time * 20f) * 0.5f + 0.5f);

        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                Color color = Color.clear;
                
                for (int s = 0; s < STATE_COUNT; s++)
                {
                    if (_timers[s][x, y] > 0f) color = STATE_COLORS[s];
                }

                if (_timers[(int)FloorState.Warning][x, y] > 0f) color.a = blink;

                _floorTexture.SetPixel(x, y, color);
            }
        }
        _floorTexture.Apply();
    }

    public void ApplyState(FloorState state, Vector2 worldPosition, float radius, float duration)
    {
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                Vector2 centerWorldPos = _grid.GetCellCenterWorld(cell);
                if (Vector2.Distance(centerWorldPos, worldPosition) <= radius)
                {
                    _timers[(int)state][x, y] = duration;
                }
            }
        }
    }

    public void ApplyStateInCone(FloorState state, Vector2 worldPosition, float radius, Vector2 direction, float maxAngle, float duration)
    {
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                Vector2 centerWorldPos = _grid.GetCellCenterWorld(cell);
                if (Vector2.Distance(centerWorldPos, worldPosition) <= radius && Vector2.Angle(direction, centerWorldPos - worldPosition) <= maxAngle)
                {
                    _timers[(int)state][x, y] = duration;
                }
            }
        }
    }

    public void ApplyWarning(Vector2 worldPosition, float radius, float warningDuration, float fireDuration)
    {
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                Vector2 centerWorldPos = _grid.GetCellCenterWorld(cell);
                if (Vector2.Distance(centerWorldPos, worldPosition) <= radius)
                {
                    _timers[(int)FloorState.Warning][x, y] = warningDuration;
                    _pendingFireTime[x, y] = fireDuration;
                }
            }
        }
    }

    public bool HasState(FloorState state, Vector2 worldPosition)
    {
        if(!TryGetCell(worldPosition, out Vector2Int cell)) return false;
        return _timers[(int)state][cell.x, cell.y] > 0;
    }   
}
