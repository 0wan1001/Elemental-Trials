using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
    public const int GRID_WIDTH = 40;
    public const int GRID_HEIGHT = 22;
    private const int STATE_COUNT = 5;
    private const float STEAM_DURATION = 3f;

    [SerializeField] private RectInt[] _poolAreas;

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
    }

    private void Update()
    {
        UpdateTimer();
    }

    public float GetTimer(FloorState state, int x, int y) => _timers[(int)state][x, y];

    public bool IsPool(int x, int y) => _isPool[x, y];

    public bool TryGetCell(Vector2 worldPosition, out Vector2Int cellNumber)
    {
        Vector3Int cell = _grid.WorldToCell(worldPosition);
        cellNumber = (Vector2Int)cell;
        return (cell.x >= 0 && cell.x < GRID_WIDTH && cell.y >= 0 && cell.y < GRID_HEIGHT);
    }

    private void UpdateTimer()
    {
        float[,] warning = _timers[(int)FloorState.Warning];

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
                        SetCell(FloorState.Fire, x, y, _pendingFireTime[x, y]);
                    }
                }
            }
        }
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
                    SetCell(state, x, y, duration);
                }
            }
        }
    }
    
    public void SpreadStateInCone(FloorState state, Vector2 worldPosition, float radius, Vector2 direction, float maxAngle, float duration, float spreadTime)
    {
        StartCoroutine(SpreadStateInConeRoutine(state, worldPosition, radius, direction, maxAngle, duration, spreadTime));
    }
    
    private IEnumerator SpreadStateInConeRoutine(FloorState state, Vector2 worldPosition, float radius, Vector2 direction, float maxAngle, float duration, float spreadTime)
    {
        float elapsed = 0f;
        float previousRadius = -1f;
        while (previousRadius < radius)
        {
            elapsed += Time.deltaTime;
            float currentRadius = spreadTime > 0f ? Mathf.Min(radius, radius * elapsed / spreadTime) : radius;
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    Vector2 centerWorldPos = _grid.GetCellCenterWorld(new Vector3Int(x, y, 0));
                    float distance = Vector2.Distance(centerWorldPos, worldPosition);
                    if (distance > previousRadius && distance <= currentRadius && Vector2.Angle(direction, centerWorldPos - worldPosition) <= maxAngle)
                    {
                        SetCell(state, x, y, duration);
                    }
                }
            }
            previousRadius = currentRadius;
            yield return null;
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
    
    private void SetCell(FloorState state, int x, int y, float duration)
    {
        float[,] wet = _timers[(int)FloorState.Wet];
        float[,] fire = _timers[(int)FloorState.Fire];
        bool waterOnFire = state == FloorState.Wet && fire[x, y] > 0f;
        bool fireOnWater = state == FloorState.Fire && wet[x, y] > 0f;
        if (waterOnFire || fireOnWater)
        {
            fire[x, y] = 0f;
            if (!_isPool[x, y]) wet[x, y] = 0f;
            _timers[(int)FloorState.Steam][x, y] = STEAM_DURATION;
            return;
        }
        _timers[(int)state][x, y] = duration;
    }
    
    public List<Vector2Int> Conduct(Vector2 worldPosition, int maxCells, float duration)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        if (!TryGetCell(worldPosition, out Vector2Int start)) return result;
        float[,] wet = _timers[(int)FloorState.Wet];
        float[,] electric = _timers[(int)FloorState.Electric];
        if (wet[start.x, start.y] <= 0f) return result;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        bool[,] visited = new bool[GRID_WIDTH, GRID_HEIGHT];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        while (queue.Count > 0 && result.Count < maxCells)
        {
            Vector2Int cell = queue.Dequeue();
            result.Add(cell);
            electric[cell.x, cell.y] = duration;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = cell + direction;
                if (next.x < 0 || next.x >= GRID_WIDTH || next.y < 0 || next.y >= GRID_HEIGHT) continue;
                if (visited[next.x, next.y] || wet[next.x, next.y] <= 0f) continue;
                visited[next.x, next.y] = true;
                queue.Enqueue(next);
            }
        }
        return result;
    }
}
