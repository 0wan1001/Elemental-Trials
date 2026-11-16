using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightningSkillData", menuName = "ScriptableObjects/LightningSkillData")]
public class LightningSkill : ElementSkill
{
    [SerializeField] private LightningStrike _strikePrefab;
    [SerializeField] private float _indicatorTime;
    [SerializeField] private float _hitRadius;
    [SerializeField] private float _electricRadius;
    [SerializeField] private float _electricDuration;
    [SerializeField] private int _maxConductCells;
    [SerializeField] private float _conductDuration;

    public override void Use(Vector2 playerPosition, Vector2 mousePosition, TileGrid grid)
    {
        LightningStrike strike = Instantiate(_strikePrefab, mousePosition, Quaternion.identity);
        strike.Init(this, mousePosition, grid, _indicatorTime);
    }

    public void Strike(Vector2 position, TileGrid grid)
    {
        FeedbackManager.Instance.LightningFlash();
        grid.ApplyState(FloorState.Electric, position, _electricRadius, _electricDuration);
        DamageInRange(position, _hitRadius, Vector2.right);

        List<Vector2Int> cells = grid.Conduct(position, _maxConductCells, _conductDuration);
        if (cells.Count == 0) return;
        FeedbackManager.Instance.Conduct();
        foreach (ElementStatus status in FindObjectsByType<ElementStatus>(FindObjectsSortMode.None))
        {
            if (grid.TryGetCell(status.transform.position, out Vector2Int cell) && cells.Contains(cell))
            {
                status.Apply(ElementType.Lightning);
            }
        }
    }
}
