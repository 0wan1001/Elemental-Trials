using UnityEngine;

[CreateAssetMenu(fileName = "LightningSkillData", menuName = "ScriptableObjects/LightningSkillData")]
public class LightningSkill : ElementSkill
{
    [SerializeField] private LightningStrike _strikePrefab;
    [SerializeField] private float _indicatorTime;
    [SerializeField] private float _hitRadius;
    [SerializeField] private float _electricRadius;
    [SerializeField] private float _electricDuration;

    public override void Use(Vector2 playerPosition, Vector2 mousePosition, TileGrid grid)
    {
        LightningStrike strike = Instantiate(_strikePrefab, mousePosition, Quaternion.identity);
        strike.Init(this, mousePosition, grid, _indicatorTime);
    }

    public void Strike(Vector2 position, TileGrid grid)
    {
        grid.ApplyState(FloorState.Electric, position, _electricRadius, _electricDuration);
        DamageInRange(position, _hitRadius, Vector2.right);
    }
}
